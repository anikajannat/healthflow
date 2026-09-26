using Healthcare.Api.Data;
using Healthcare.Api.DTOs;
using Healthcare.Api.Helpers;
using Healthcare.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/v1/schedules")]
public class SchedulesController(AppDbContext db) : ControllerBase
{
    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateScheduleRequest request)
    {
        var doctor = await db.DoctorProfiles.FirstOrDefaultAsync(x => x.UserId == User.UserId() && x.ApprovalStatus == DoctorApprovalStatus.Approved);
        if (doctor is null) return StatusCode(403, new { message = "Approved doctor profile required." });

        var error = ValidateRange(request.Date, request.StartTime, request.EndTime);
        if (error is not null) return BadRequest(new { message = error });
        if (await db.DoctorSchedules.AnyAsync(x => x.DoctorProfileId == doctor.Id && x.Date == request.Date))
            return Conflict(new { message = "Only one schedule per date is allowed." });

        var item = new DoctorSchedule
        {
            DoctorProfileId = doctor.Id, Date = request.Date, StartTime = request.StartTime,
            EndTime = request.EndTime, MeetLink = request.MeetLink, Status = ScheduleStatus.Draft
        };
        db.DoctorSchedules.Add(item);
        await db.SaveChangesAsync();
        return Ok(new { item.Id, message = "Draft schedule created.", totalSlots = TotalSlots(item.StartTime, item.EndTime) });
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var uid = User.UserId();
        var items = await db.DoctorSchedules.AsNoTracking().Where(x => x.DoctorProfile.UserId == uid)
            .OrderByDescending(x => x.Date).Select(x => new
            {
                x.Id, x.Date, x.StartTime, x.EndTime, status = x.Status.ToString(), x.MeetLink,
                totalSlots = (int)((x.EndTime.ToTimeSpan() - x.StartTime.ToTimeSpan()).TotalMinutes / 20),
                bookedSlots = x.Appointments.Count(a => a.Status != AppointmentStatus.Cancelled)
            }).ToListAsync();
        return Ok(items);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateScheduleRequest request)
    {
        var schedule = await db.DoctorSchedules.Include(x => x.DoctorProfile).Include(x => x.Appointments)
            .FirstOrDefaultAsync(x => x.Id == id && x.DoctorProfile.UserId == User.UserId());
        if (schedule is null) return NotFound();

        var nextStart = request.StartTime ?? schedule.StartTime;
        var nextEnd = request.EndTime ?? schedule.EndTime;
        if (schedule.Status == ScheduleStatus.Published && (request.StartTime.HasValue || request.EndTime.HasValue) &&
            schedule.Appointments.Any(x => x.Status != AppointmentStatus.Cancelled))
            return BadRequest(new { message = "Time range is locked after the first booking." });

        if (schedule.Status == ScheduleStatus.Published && request.StartTime.HasValue == false && request.EndTime.HasValue == false) { }
        else
        {
            var error = ValidateRange(schedule.Date, nextStart, nextEnd);
            if (error is not null) return BadRequest(new { message = error });
            schedule.StartTime = nextStart; schedule.EndTime = nextEnd;
        }

        if (!string.IsNullOrWhiteSpace(request.MeetLink)) schedule.MeetLink = request.MeetLink;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ScheduleStatus>(request.Status, true, out var status))
            schedule.Status = status;

        await db.SaveChangesAsync();
        return Ok(new { message = "Schedule updated." });
    }


    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<IActionResult> ForDoctor(Guid doctorId, [FromQuery] int days = 30)
    {
        days = Math.Clamp(days, 1, 60);
        var today = DateOnly.FromDateTime(DateTime.Now);
        var lastDate = today.AddDays(days);

        var doctorExists = await db.DoctorProfiles.AsNoTracking().AnyAsync(x => x.Id == doctorId &&
            x.ApprovalStatus == DoctorApprovalStatus.Approved && x.User.Status == UserStatus.Active && !x.User.IsDeleted);
        if (!doctorExists) return NotFound(new { message = "Doctor not found or unavailable." });

        var items = await db.DoctorSchedules.AsNoTracking()
            .Include(x => x.DoctorProfile).ThenInclude(x => x.User)
            .Include(x => x.Appointments)
            .Where(x => x.DoctorProfileId == doctorId && x.Status == ScheduleStatus.Published &&
                        x.Date >= today && x.Date <= lastDate)
            .OrderBy(x => x.Date).ThenBy(x => x.StartTime)
            .ToListAsync();

        var result = items.Select(x =>
        {
            var total = TotalSlots(x.StartTime, x.EndTime);
            var booked = x.Appointments.Where(a => a.Status != AppointmentStatus.Cancelled).Select(a => a.SlotIndex).ToHashSet();
            var now = DateTime.Now;
            var unavailable = new HashSet<int>(booked);
            if (x.Date == today)
            {
                for (var i = 0; i < total; i++)
                {
                    var slotStart = x.Date.ToDateTime(x.StartTime.AddMinutes(i * 20));
                    if (slotStart <= now) unavailable.Add(i);
                }
            }

            return new
            {
                x.Id, x.Date, x.StartTime, x.EndTime,
                totalSlots = total,
                bookedSlots = booked.OrderBy(i => i),
                unavailableSlots = unavailable.OrderBy(i => i),
                availableSlots = total - unavailable.Count,
                doctor = new
                {
                    x.DoctorProfile.Id, x.DoctorProfile.UserId, x.DoctorProfile.User.Name, x.DoctorProfile.User.ImageUrl,
                    x.DoctorProfile.Specialization, x.DoctorProfile.Qualifications, x.DoctorProfile.ExperienceYears,
                    x.DoctorProfile.ConsultationFee, x.DoctorProfile.Bio, x.DoctorProfile.AverageRating, x.DoctorProfile.ReviewCount
                }
            };
        }).Where(x => x.availableSlots > 0);

        return Ok(result);
    }

    [HttpGet("available-today")]
    public async Task<IActionResult> AvailableToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var now = TimeOnly.FromDateTime(DateTime.Now);

        var items = await db.DoctorSchedules.AsNoTracking().Include(x => x.DoctorProfile).ThenInclude(x => x.User)
            .Include(x => x.Appointments)
            .Where(x => x.Date == today && x.Status == ScheduleStatus.Published && x.EndTime > now &&
                        x.DoctorProfile.ApprovalStatus == DoctorApprovalStatus.Approved &&
                        x.DoctorProfile.User.Status == UserStatus.Active)
            .ToListAsync();

        var result = items.Select(x =>
        {
            var total = TotalSlots(x.StartTime, x.EndTime);
            var booked = x.Appointments.Where(a => a.Status != AppointmentStatus.Cancelled).Select(a => a.SlotIndex).ToHashSet();
            return new
            {
                x.Id, x.Date, x.StartTime, x.EndTime, x.MeetLink,
                doctor = new { x.DoctorProfile.Id, x.DoctorProfile.User.Name, x.DoctorProfile.Specialization, x.DoctorProfile.ConsultationFee, x.DoctorProfile.AverageRating },
                totalSlots = total, bookedSlots = booked.OrderBy(i => i), availableSlots = total - booked.Count
            };
        }).Where(x => x.availableSlots > 0);
        return Ok(result);
    }

    private static int TotalSlots(TimeOnly start, TimeOnly end) => (int)((end.ToTimeSpan() - start.ToTimeSpan()).TotalMinutes / 20);

    private static string? ValidateRange(DateOnly date, TimeOnly start, TimeOnly end)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today)) return "Schedule date cannot be in the past.";
        if (end <= start) return "End time must be later than start time on the same date.";
        var hours = (end.ToTimeSpan() - start.ToTimeSpan()).TotalHours;
        if (hours < 3 || hours > 8) return "Schedule duration must be between 3 and 8 hours.";
        return null;
    }
}
