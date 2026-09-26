using Healthcare.Api.Data;
using Healthcare.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/v1/doctors")]
public class DoctorsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? specialization)
    {
        var q = db.DoctorProfiles.AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.ApprovalStatus == DoctorApprovalStatus.Approved && x.User.Status == UserStatus.Active && !x.User.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.User.Name.Contains(search) || x.Specialization.Contains(search));
        if (!string.IsNullOrWhiteSpace(specialization))
            q = q.Where(x => x.Specialization == specialization);

        var result = await q.OrderBy(x => x.User.Name).Select(x => new
        {
            x.Id, userId = x.UserId, x.User.Name, x.User.ImageUrl, x.Specialization, x.Qualifications,
            x.ExperienceYears, x.ConsultationFee, x.Bio, x.AverageRating, x.ReviewCount
        }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var doctor = await db.DoctorProfiles.AsNoTracking().Include(x => x.User)
            .Where(x => x.Id == id && x.ApprovalStatus == DoctorApprovalStatus.Approved)
            .Select(x => new
            {
                x.Id, x.User.Name, x.User.ImageUrl, x.Specialization, x.Qualifications, x.ExperienceYears,
                x.ConsultationFee, x.Bio, x.AverageRating, x.ReviewCount
            }).FirstOrDefaultAsync();
        return doctor is null ? NotFound() : Ok(doctor);
    }
}
