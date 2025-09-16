using Microsoft.AspNetCore.Mvc;
using quickBook.Data;
using quickBook.Models;
using quickBook.Dtos;
using quickBook.Mappers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using quickBook.DTOs;
using System.Security.Claims;

namespace quickBook.Controllers
{
    [ApiController]
    [Route("api/[controller]"), Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Create Appointment
        [HttpPost("create")]
        public async Task<IActionResult> Create(AppointmentCreateDto dto)
        {
            try
            {
                // ✅ Check Organizer
                var organizerExists = await _context.Users.AnyAsync(u => u.UserId == dto.OrganizerId);
                if (!organizerExists)
                    return BadRequest(new { message = $"Organizer with ID {dto.OrganizerId} does not exist." });

                // ✅ Check Status
                var statusExists = await _context.AppointmentStatuses.AnyAsync(s => s.StatusId == dto.StatusId);
                if (!statusExists)
                    return BadRequest(new { message = $"Status with ID {dto.StatusId} does not exist." });

                // ✅ Validate Participants
                var missingParticipants = dto.ParticipantIds
                    .Where(id => !_context.Users.Any(u => u.UserId == id))
                    .ToList();

                if (missingParticipants.Any())
                    return BadRequest(new { message = $"Participants not found: {string.Join(", ", missingParticipants)}" });

                var conflicts = await CheckConflictsAsync(dto.OrganizerId, dto.StartTime, dto.EndTime, dto.ParticipantIds);
                if (conflicts.Any())
                    return Conflict(new { message = "Conflicting appointments found.", conflicts });

                // ✅ Map to entity
                var appointment = AppointmentMapper.ToEntity(dto);

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // ✅ Insert Participants into AppointmentParticipants table
                foreach (var userId in dto.ParticipantIds)
                {
                    _context.AppointmentParticipants.Add(new AppointmentParticipant
                    {
                        AppointmentId = appointment.AppointmentId,
                        UserId = userId
                    });
                }

                await _context.SaveChangesAsync();

                // ✅ Return the created appointment with details
                var createdAppointment = await _context.Appointments
                    .Include(a => a.Organizer)
                    .Include(a => a.Status)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                // After saving appointment and participants
                if (!string.IsNullOrWhiteSpace(dto.Frequency))
                {
                    var recurrence = new AppointmentRecurrence
                    {
                        AppointmentId = appointment.AppointmentId,
                        Frequency = dto.Frequency,
                        Interval = dto.Interval ?? 1,
                        StartDate = dto.RecurrenceStartDate ?? appointment.StartTime,
                        EndDate = dto.RecurrenceEndDate,
                        DaysOfWeek = dto.DaysOfWeek != null ? JsonSerializer.Serialize(dto.DaysOfWeek) : null,
                        DayOfMonth = dto.DayOfMonth,
                        MonthOfYear = dto.MonthOfYear
                    };

                    _context.AppointmentRecurrences.Add(recurrence);
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(Create), new { id = appointment.AppointmentId }, createdAppointment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpPut("addAppointmentStatus")]
        public async Task<IActionResult> AddAppointmentStatus()
        {
            if (await _context.AppointmentStatuses.AnyAsync())
                return Conflict("Statuses already seeded.");

            var statuses = new List<AppointmentStatus>
            {
                new AppointmentStatus { Name = "Scheduled", ColorCode = "#28a745" }, // green
                new AppointmentStatus { Name = "Re Scheduled", ColorCode = "#ffc107" }, // yellow
                new AppointmentStatus { Name = "Completed", ColorCode = "#007bff" }, // blue
                new AppointmentStatus { Name = "Canceled", ColorCode = "#dc3545" }  // red
            };

            _context.AppointmentStatuses.AddRange(statuses);
            await _context.SaveChangesAsync();

            return Ok("Appointment statuses seeded successfully.");
        }
        
        [HttpPut("cancel"), Authorize]
        public async Task<IActionResult> Cancel(AppointmentCancelDto dto)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);

                if (appointment == null)
                    return NotFound("Appointment not found.");

                // Assume Cancelled = StatusId = 2
                int cancelledStatusId = 2;

                AppointmentMapper.ApplyCancel(appointment, dto, cancelledStatusId);

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    Message = "Appointment cancelled successfully.",
                    AppointmentId = appointment.AppointmentId,
                    CancelReason = appointment.CancelReason
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
            
        }
        
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] AppointmentUpdateDto dto)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Status)
                    .FirstOrDefaultAsync(a => a.AppointmentId == dto.AppointmentId);

                var conflicts = await CheckConflictsAsync(dto.OrganizerId, dto.StartTime, dto.EndTime, dto.ParticipantIds, excludeAppointmentId: dto.AppointmentId);
                if (conflicts.Any())
                    return Conflict(new { message = "Conflicting appointments found.", conflicts });

                if (appointment == null)
                {
                    return NotFound(new { message = $"Appointment {dto.AppointmentId} not found." });
                }

                if (appointment.IsCancelled)
                {
                    return BadRequest(new { message = "Cannot update a cancelled appointment." });
                }
                
                // ✅ Check Organizer exists
                var organizerExists = await _context.Users.AnyAsync(u => u.UserId == dto.OrganizerId);
                if (!organizerExists)
                    return BadRequest(new { message = $"Organizer with ID {dto.OrganizerId} does not exist." });

                // ✅ Check Status exists
                var statusExists = await _context.AppointmentStatuses.AnyAsync(s => s.StatusId == dto.StatusId);
                if (!statusExists)
                    return BadRequest(new { message = $"Status with ID {dto.StatusId} does not exist." });

                // ✅ Validate Participants
                if (dto.ParticipantIds.Any())
                {
                    var existingUsers = await _context.Users
                        .Where(u => dto.ParticipantIds.Contains(u.UserId))
                        .Select(u => u.UserId)
                        .ToListAsync();

                    var missingUsers = dto.ParticipantIds.Except(existingUsers).ToList();
                    if (missingUsers.Any())
                        return BadRequest(new { message = $"Participants not found: {string.Join(", ", missingUsers)}" });
                }
                
                // ✅ Collect all users (organizer + participants)
                var usersInvolved = new List<int> { appointment.OrganizerId };
                usersInvolved.AddRange(dto.ParticipantIds);

                // ✅ Conflict check
                var conflictingAppointments = await _context.Appointments
                    .Where(a => !a.IsCancelled &&
                                a.AppointmentId != appointment.AppointmentId && // exclude current
                                (
                                    // Check if any of the participants or organizer are involved
                                    _context.AppointmentParticipants
                                        .Any(ap => ap.AppointmentId == a.AppointmentId && usersInvolved.Contains(ap.UserId))
                                    || usersInvolved.Contains(a.OrganizerId)
                                ) &&
                                (
                                    // Time overlap check
                                    (dto.StartTime < a.EndTime && dto.EndTime > a.StartTime)
                                )
                          )
                    .ToListAsync();

                if (conflictingAppointments.Any())
                {
                    return Conflict(new
                    {
                        message = "Scheduling conflict detected.",
                        conflicts = conflictingAppointments.Select(c => new
                        {
                            c.AppointmentId,
                            c.Title,
                            c.StartTime,
                            c.EndTime
                        })
                    });
                }

                // ✅ Update appointment fields
                appointment.Title = dto.Title;
                appointment.Description = dto.Description;
                appointment.Location = dto.Location;
                appointment.MeetingLink = dto.MeetingLink;
                appointment.StartTime = dto.StartTime;
                appointment.EndTime = dto.EndTime;
                appointment.StatusId = dto.StatusId;

                // ✅ Update participants (clear + re-add)
                var existingParticipants = _context.AppointmentParticipants
                    .Where(ap => ap.AppointmentId == appointment.AppointmentId);
                _context.AppointmentParticipants.RemoveRange(existingParticipants);

                foreach (var userId in dto.ParticipantIds)
                {
                    _context.AppointmentParticipants.Add(new AppointmentParticipant
                    {
                        AppointmentId = appointment.AppointmentId,
                        UserId = userId
                    });
                }

                // ✅ Handle recurrence
                var recurrence = await _context.AppointmentRecurrences
                    .FirstOrDefaultAsync(r => r.AppointmentId == appointment.AppointmentId);

                if (!string.IsNullOrEmpty(dto.Frequency) && dto.Interval.HasValue && dto.RecurrenceStartDate.HasValue)
                {
                    if (recurrence == null)
                    {
                        recurrence = new AppointmentRecurrence
                        {
                            AppointmentId = appointment.AppointmentId
                        };
                        _context.AppointmentRecurrences.Add(recurrence);
                    }

                    recurrence.Frequency = dto.Frequency;
                    recurrence.Interval = dto.Interval ?? 1;
                    recurrence.StartDate = dto.RecurrenceStartDate ?? dto.StartTime;
                    recurrence.EndDate = dto.RecurrenceEndDate;
                    recurrence.DaysOfWeek = dto.DaysOfWeek != null ? JsonSerializer.Serialize(dto.DaysOfWeek) : null;
                    recurrence.DayOfMonth = dto.DayOfMonth;
                    recurrence.MonthOfYear = dto.MonthOfYear;
                }
                else if (recurrence != null)
                {
                    _context.AppointmentRecurrences.Remove(recurrence);
                }

                await _context.SaveChangesAsync();

                var updated = await _context.Appointments
                    .Include(a => a.Organizer)
                    .Include(a => a.Status)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        } 

        private int GetLoggedInUserId()
        {
            // Try standard NameIdentifier first
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("userId")
                        ?? User.FindFirst("nameid");

            if (claim == null)
                throw new UnauthorizedAccessException("User ID not found in token.");

            return int.Parse(claim.Value);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAppointments([FromQuery] AppointmentListRequestDto dto)
        {
            var currentUserId = GetLoggedInUserId();

            // Include organizer OR participant; filter only by not cancelled
            var baseAppointments = await _context.Appointments
                .Include(a => a.Organizer)
                .Include(a => a.Status)
                .Where(a => !a.IsCancelled &&
                            (a.OrganizerId == currentUserId ||
                             _context.AppointmentParticipants.Any(p => p.AppointmentId == a.AppointmentId && p.UserId == currentUserId)))
                .ToListAsync();

            var result = new List<AppointmentListDto>();

            foreach (var appointment in baseAppointments)
            {
                var recurrence = await _context.AppointmentRecurrences
                    .FirstOrDefaultAsync(r => r.AppointmentId == appointment.AppointmentId);

                var participants = await _context.AppointmentParticipants
                    .Where(ap => ap.AppointmentId == appointment.AppointmentId)
                    .Select(ap => ap.User)
                    .ToListAsync();

                if (recurrence == null)
                {
                    // Overlap check (not "fully inside")
                    if (appointment.EndTime >= dto.StartDate && appointment.StartTime <= dto.EndDate)
                    {
                        result.Add(AppointmentMapper.ToListDto(appointment, participants));
                    }
                }
                else 
                {
                    var duration = appointment.EndTime - appointment.StartTime;
                    var freq = recurrence.Frequency?.Trim().ToLowerInvariant();
                    var interval = Math.Max(1, recurrence.Interval);
                    var occurrenceStart = recurrence.StartDate;

                    // Deserialize custom days if weekly
                    List<string>? daysOfWeek = null;
                    if (!string.IsNullOrWhiteSpace(recurrence.DaysOfWeek))
                    {
                        daysOfWeek = JsonSerializer.Deserialize<List<string>>(recurrence.DaysOfWeek);
                    }

                    // Loop until end of requested window
                    while (occurrenceStart <= dto.EndDate &&
                           (!recurrence.EndDate.HasValue || occurrenceStart <= recurrence.EndDate.Value))
                    {
                        var occurrenceEnd = occurrenceStart + duration;

                        // ✅ Overlap check
                        if (occurrenceEnd >= dto.StartDate && occurrenceStart <= dto.EndDate)
                        {
                            var dtoItem = AppointmentMapper.ToListDto(appointment, participants);
                            dtoItem.StartTime = occurrenceStart;
                            dtoItem.EndTime = occurrenceEnd;
                            result.Add(dtoItem);
                        }

                        // ✅ Move to next occurrence
                        switch (freq)
                        {
                            case "daily":
                                occurrenceStart = occurrenceStart.AddDays(interval);
                                break;

                            case "weekly":
                                if (daysOfWeek != null && daysOfWeek.Any())
                                {
                                    // Find next valid weekday
                                    var next = occurrenceStart.AddDays(1);
                                    while (!daysOfWeek.Contains(next.DayOfWeek.ToString(),
                                               StringComparer.OrdinalIgnoreCase))
                                    {
                                        next = next.AddDays(1);
                                        if (next > dto.EndDate) break;
                                    }

                                    occurrenceStart = next;
                                }
                                else
                                {
                                    occurrenceStart = occurrenceStart.AddDays(7 * interval);
                                }

                                break;

                            case "monthly":
                                if (recurrence.DayOfMonth.HasValue)
                                {
                                    var nextMonth = occurrenceStart.AddMonths(interval);
                                    var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                                    var day = Math.Min(recurrence.DayOfMonth.Value, daysInMonth);
                                    occurrenceStart = new DateTime(nextMonth.Year, nextMonth.Month, day,
                                        occurrenceStart.Hour, occurrenceStart.Minute, occurrenceStart.Second);
                                }
                                else
                                {
                                    occurrenceStart = occurrenceStart.AddMonths(interval);
                                }

                                break;

                            case "yearly":
                                if (recurrence.MonthOfYear.HasValue && recurrence.DayOfMonth.HasValue)
                                {
                                    var nextYear = occurrenceStart.Year + interval;
                                    var daysInMonth = DateTime.DaysInMonth(nextYear, recurrence.MonthOfYear.Value);
                                    var day = Math.Min(recurrence.DayOfMonth.Value, daysInMonth);
                                    occurrenceStart = new DateTime(nextYear, recurrence.MonthOfYear.Value, day,
                                        occurrenceStart.Hour, occurrenceStart.Minute, occurrenceStart.Second);
                                }
                                else
                                {
                                    occurrenceStart = occurrenceStart.AddYears(interval);
                                }

                                break;

                            default:
                                // Stop if frequency is invalid
                                occurrenceStart = DateTime.MaxValue;
                                break;
                        }
                    }
                }
            }

            return Ok(result.OrderBy(r => r.StartTime));
        }

        private async Task<List<Appointment>> CheckConflictsAsync(
    	int organizerId,
    DateTime startTime,
    DateTime endTime,
    List<int> participantIds,
    int? excludeAppointmentId = null)	
{
    var usersInvolved = new List<int> { organizerId };
    usersInvolved.AddRange(participantIds ?? new List<int>());

    // get candidate appointments that involve any of the users
    var baseAppointments = await _context.Appointments
        .Where(a => !a.IsCancelled &&
                    (excludeAppointmentId == null || a.AppointmentId != excludeAppointmentId) &&
                    (
                        usersInvolved.Contains(a.OrganizerId) ||
                        _context.AppointmentParticipants.Any(ap => ap.AppointmentId == a.AppointmentId &&
                                                                   usersInvolved.Contains(ap.UserId))
                    ))
        .Include(a => a.Organizer)
        .ToListAsync();

    var conflicts = new List<Appointment>();
    var seen = new HashSet<int>();

    foreach (var appointment in baseAppointments)
    {
        if (seen.Contains(appointment.AppointmentId))
            continue;

        var recurrence = await _context.AppointmentRecurrences
            .FirstOrDefaultAsync(r => r.AppointmentId == appointment.AppointmentId);

        if (recurrence == null)
        {
            // simple overlap
            if (startTime < appointment.EndTime && endTime > appointment.StartTime)
            {
                conflicts.Add(appointment);
                seen.Add(appointment.AppointmentId);
            }
        }
        else
        {
            var duration = appointment.EndTime - appointment.StartTime;
            var freq = recurrence.Frequency?.Trim().ToLowerInvariant();
            var interval = Math.Max(1, recurrence.Interval);
            var occurrenceStart = recurrence.StartDate;

            // weekly: parse custom days
            List<string>? daysOfWeek = null;
            if (!string.IsNullOrWhiteSpace(recurrence.DaysOfWeek))
            {
                daysOfWeek = JsonSerializer.Deserialize<List<string>>(recurrence.DaysOfWeek);
            }

            while (occurrenceStart <= endTime &&
                   (!recurrence.EndDate.HasValue || occurrenceStart <= recurrence.EndDate.Value))
            {
                var occurrenceEnd = occurrenceStart + duration;

                // ✅ overlap check
                if (startTime < occurrenceEnd && endTime > occurrenceStart)
                {
                    conflicts.Add(appointment);
                    seen.Add(appointment.AppointmentId);
                    break; // no need to check further occurrences for this appointment
                }

                // ✅ move to next occurrence
                switch (freq)
                {
                    case "daily":
                        occurrenceStart = occurrenceStart.AddDays(interval);
                        break;

                    case "weekly":
                        if (daysOfWeek != null && daysOfWeek.Any())
                        {
                            // step one day at a time until next valid weekday
                            var next = occurrenceStart.AddDays(1);
                            while (!daysOfWeek.Contains(next.DayOfWeek.ToString(),
                                       StringComparer.OrdinalIgnoreCase))
                            {
                                next = next.AddDays(1);
                                if (next > endTime) break;
                            }
                            occurrenceStart = next;
                        }
                        else
                        {
                            occurrenceStart = occurrenceStart.AddDays(7 * interval);
                        }
                        break;

                    case "monthly":
                        if (recurrence.DayOfMonth.HasValue)
                        {
                            var nextMonth = occurrenceStart.AddMonths(interval);
                            var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                            var day = Math.Min(recurrence.DayOfMonth.Value, daysInMonth);

                            occurrenceStart = new DateTime(
                                nextMonth.Year,
                                nextMonth.Month,
                                day,
                                occurrenceStart.Hour,
                                occurrenceStart.Minute,
                                occurrenceStart.Second
                            );
                        }
                        else
                        {
                            occurrenceStart = occurrenceStart.AddMonths(interval);
                        }
                        break;

                    case "yearly":
                        if (recurrence.MonthOfYear.HasValue && recurrence.DayOfMonth.HasValue)
                        {
                            var nextYear = occurrenceStart.Year + interval;
                            var daysInMonth = DateTime.DaysInMonth(nextYear, recurrence.MonthOfYear.Value);
                            var day = Math.Min(recurrence.DayOfMonth.Value, daysInMonth);

                            occurrenceStart = new DateTime(
                                nextYear,
                                recurrence.MonthOfYear.Value,
                                day,
                                occurrenceStart.Hour,
                                occurrenceStart.Minute,
                                occurrenceStart.Second
                            );
                        }
                        else
                        {
                            occurrenceStart = occurrenceStart.AddYears(interval);
                        }
                        break;

                    default:
                        occurrenceStart = DateTime.MaxValue; // stop
                        break;
                }
            }
        }
    }

    return conflicts;
}

        

        
        [HttpPost("check-conflict")]
        public async Task<IActionResult> CheckConflict([FromBody] AppointmentConflictCheckDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "StartTime must be before EndTime." });

            var conflicts = await CheckConflictsAsync(dto.OrganizerId, dto.StartTime, dto.EndTime, dto.ParticipantIds);

            if (conflicts.Any())
                return Conflict(new { message = "Conflicting appointments found.", conflicts });

            return Ok(new { message = "No conflicts found." });
        }
    }
}