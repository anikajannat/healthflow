using Healthcare.Api.Data;
using Healthcare.Api.DTOs;
using Healthcare.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController(AppDbContext db) : ControllerBase
{
    [Authorize(Roles = "Patient")]
    [HttpPut("patient")]
    public async Task<IActionResult> UpdatePatient(UpdatePatientProfileRequest request)
    {
        var user = await db.Users.Include(x => x.PatientProfile).FirstAsync(x => x.Id == User.UserId());
        var p = user.PatientProfile ?? new Models.PatientProfile { UserId = user.Id };
        p.Phone = request.Phone; p.DateOfBirth = request.DateOfBirth; p.Gender = request.Gender; p.Address = request.Address;
        p.BloodGroup = request.BloodGroup; p.Allergies = request.Allergies; p.CurrentMedications = request.CurrentMedications; p.EmergencyContact = request.EmergencyContact;
        if (user.PatientProfile is null) db.PatientProfiles.Add(p);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { message = "Patient profile updated." });
    }
}
