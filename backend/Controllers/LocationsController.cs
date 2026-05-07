using EstagioCheck.API.Data;
using EstagioCheck.API.DTOs;
using EstagioCheck.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstagioCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
    {
        var locs = await db.Locations.OrderBy(l => l.Name).ToListAsync();
        return Ok(locs.Select(Map));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> Get(Guid id)
    {
        var loc = await db.Locations.FindAsync(id);
        return loc == null ? NotFound() : Ok(Map(loc));
    }

    [HttpPost]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult<LocationDto>> Create([FromBody] CreateLocationDto dto)
    {
        var loc = new Location
        {
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            RadiusMeters = dto.RadiusMeters > 0 ? dto.RadiusMeters : 100,
            IsInstitution = dto.IsInstitution,
            ShiftStart = string.IsNullOrEmpty(dto.ShiftStart) ? "07:00" : dto.ShiftStart,
            ShiftEnd = string.IsNullOrEmpty(dto.ShiftEnd) ? "13:00" : dto.ShiftEnd
        };

        db.Locations.Add(loc);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = loc.Id }, Map(loc));
    }

    [HttpPost("batch")]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult> BatchCreate([FromBody] List<CreateLocationDto> dtos)
    {
        if (dtos.Count == 0)
            return BadRequest(new { message = "Lista vazia." });

        var locs = dtos.Select(dto => new Location
        {
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            RadiusMeters = dto.RadiusMeters > 0 ? dto.RadiusMeters : 100,
            IsInstitution = dto.IsInstitution,
            ShiftStart = string.IsNullOrEmpty(dto.ShiftStart) ? "07:00" : dto.ShiftStart,
            ShiftEnd = string.IsNullOrEmpty(dto.ShiftEnd) ? "13:00" : dto.ShiftEnd
        }).ToList();

        db.Locations.AddRange(locs);
        await db.SaveChangesAsync();
        return Ok(new { inserted = locs.Count });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "supervisor")]
    public async Task<ActionResult<LocationDto>> Update(Guid id, [FromBody] UpdateLocationDto dto)
    {
        var loc = await db.Locations.FindAsync(id);
        if (loc == null) return NotFound();

        loc.Name = dto.Name.Trim();
        loc.Address = dto.Address?.Trim();
        loc.Latitude = dto.Latitude;
        loc.Longitude = dto.Longitude;
        loc.RadiusMeters = dto.RadiusMeters > 0 ? dto.RadiusMeters : 100;
        loc.IsInstitution = dto.IsInstitution;
        loc.ShiftStart = string.IsNullOrEmpty(dto.ShiftStart) ? "07:00" : dto.ShiftStart;
        loc.ShiftEnd = string.IsNullOrEmpty(dto.ShiftEnd) ? "13:00" : dto.ShiftEnd;

        await db.SaveChangesAsync();
        return Ok(Map(loc));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "supervisor")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var loc = await db.Locations.FindAsync(id);
        if (loc == null) return NotFound();
        db.Locations.Remove(loc);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static LocationDto Map(Location l) => new()
    {
        Id = l.Id, Name = l.Name, Address = l.Address,
        Latitude = l.Latitude, Longitude = l.Longitude,
        RadiusMeters = l.RadiusMeters, IsInstitution = l.IsInstitution,
        ShiftStart = l.ShiftStart, ShiftEnd = l.ShiftEnd
    };
}
