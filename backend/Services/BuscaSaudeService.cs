using System.Text.Json;
using System.Text.Json.Serialization;

namespace EstagioCheck.API.Services;

/// <summary>Consulta estabelecimentos de saúde do DF via API pública do OpenDataSUS (CNES).</summary>
public class BuscaSaudeService(IHttpClientFactory httpClientFactory, ILogger<BuscaSaudeService> logger)
{
    private const string BaseUrl = "https://apidadosabertos.saude.gov.br/cnes/estabelecimentos";
    // Código UF do Distrito Federal
    private const int CoUfDf = 53;

    public async Task<List<BuscaSaudeEstabelecimentoDto>> BuscarAsync(
        string? termo = null,
        string? municipio = null,
        int limit = 50,
        int offset = 0)
    {
        var client = httpClientFactory.CreateClient("BuscaSaude");

        var queryParams = new List<string>
        {
            $"co_uf={CoUfDf}",
            $"limit={limit}",
            $"offset={offset}"
        };

        if (!string.IsNullOrWhiteSpace(municipio))
            queryParams.Add($"no_municipio={Uri.EscapeDataString(municipio.ToUpper())}");

        if (!string.IsNullOrWhiteSpace(termo))
            queryParams.Add($"no_fantasia={Uri.EscapeDataString(termo.ToUpper())}");

        var url = $"{BaseUrl}?{string.Join("&", queryParams)}";

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonSerializer.Deserialize<CnesRoot>(json, JsonOptions);

            return root?.Estabelecimentos
                .Where(e => e.Latitude.HasValue && e.Longitude.HasValue)
                .Select(e => new BuscaSaudeEstabelecimentoDto(
                    CodigoCnes: e.CodigoCnes.ToString(),
                    Nome: !string.IsNullOrEmpty(e.NomeFantasia) ? e.NomeFantasia : e.NomeRazaoSocial,
                    Endereco: FormatarEndereco(e),
                    Latitude: e.Latitude!.Value,
                    Longitude: e.Longitude!.Value,
                    Telefone: e.Telefone,
                    TurnoAtendimento: e.DescricaoTurnoAtendimento
                ))
                .ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar API BuscaSaúde DF. URL: {Url}", url);
            throw;
        }
    }

    private static string FormatarEndereco(CnesEstabelecimento e)
    {
        var partes = new List<string>();
        if (!string.IsNullOrEmpty(e.EnderecoLogradouro)) partes.Add(e.EnderecoLogradouro);
        if (!string.IsNullOrEmpty(e.NumeroEstabelecimento)) partes.Add(e.NumeroEstabelecimento);
        if (!string.IsNullOrEmpty(e.BairroEstabelecimento)) partes.Add(e.BairroEstabelecimento);
        if (!string.IsNullOrEmpty(e.CodigoCep)) partes.Add(e.CodigoCep);
        return string.Join(", ", partes);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── Modelos internos de deserialização ────────────────────────────────────
    private sealed class CnesRoot
    {
        [JsonPropertyName("estabelecimentos")]
        public List<CnesEstabelecimento> Estabelecimentos { get; set; } = [];
    }

    private sealed class CnesEstabelecimento
    {
        [JsonPropertyName("codigo_cnes")]
        public long CodigoCnes { get; set; }

        [JsonPropertyName("nome_razao_social")]
        public string NomeRazaoSocial { get; set; } = string.Empty;

        [JsonPropertyName("nome_fantasia")]
        public string? NomeFantasia { get; set; }

        [JsonPropertyName("endereco_estabelecimento")]
        public string? EnderecoLogradouro { get; set; }

        [JsonPropertyName("numero_estabelecimento")]
        public string? NumeroEstabelecimento { get; set; }

        [JsonPropertyName("bairro_estabelecimento")]
        public string? BairroEstabelecimento { get; set; }

        [JsonPropertyName("codigo_cep_estabelecimento")]
        public string? CodigoCep { get; set; }

        [JsonPropertyName("numero_telefone_estabelecimento")]
        public string? Telefone { get; set; }

        [JsonPropertyName("descricao_turno_atendimento")]
        public string? DescricaoTurnoAtendimento { get; set; }

        [JsonPropertyName("latitude_estabelecimento_decimo_grau")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude_estabelecimento_decimo_grau")]
        public double? Longitude { get; set; }
    }
}

/// <summary>DTO de retorno para o frontend.</summary>
public record BuscaSaudeEstabelecimentoDto(
    string CodigoCnes,
    string Nome,
    string Endereco,
    double Latitude,
    double Longitude,
    string? Telefone,
    string? TurnoAtendimento
);
