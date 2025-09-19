using quickBook.Models;
using quickBook.Dtos;
using quickBook.DTOs;
using System.Text.Json;


namespace quickBook.Mappers
{
    public static class AppointmentMapper
    {
        public static Appointment ToEntity(AppointmentCreateDto dto)
        {
            return new Appointment
            {
                OrganizerId = dto.OrganizerId,
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                MeetingLink = dto.MeetingLink,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                StatusId = dto.StatusId,
                IsCancelled = false
            };
        }
        
        public static void ApplyCancel(Appointment appointment, AppointmentCancelDto dto, int cancelledStatusId)
        {
            appointment.StatusId = cancelledStatusId;
            appointment.CancelReason = dto.CancelReason;
            appointment.IsCancelled = true;
        }
        public static void ApplyUpdate(Appointment appointment, AppointmentUpdateDto dto)
        {
            appointment.Title = dto.Title ?? appointment.Title;
            appointment.Description = dto.Description ?? appointment.Description;
            appointment.Location = dto.Location ?? appointment.Location;
            appointment.MeetingLink = dto.MeetingLink ?? appointment.MeetingLink;
            appointment.StartTime = dto.StartTime;
            appointment.EndTime = dto.EndTime;
            appointment.StatusId = dto.StatusId;
        }
        public static AppointmentListDto ToListDto(Appointment appointment, List<User> participants, AppointmentRecurrence? recurrence = null)
        {
            return new AppointmentListDto
            {
                AppointmentId = appointment.AppointmentId,
                Title = appointment.Title,
                Description = appointment.Description,
                Location = appointment.Location,
                MeetingLink = appointment.MeetingLink,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status.Name,
                StatusColor = appointment.Status.ColorCode,
                OrganizerId = appointment.Organizer.UserId,
                Participants = participants
                    .Select(p => p.UserId)
                    .ToList(),
Frequency = recurrence?.Frequency,
        Interval = recurrence?.Interval,
        RecurrenceStartDate = recurrence?.StartDate,
        RecurrenceEndDate = recurrence?.EndDate,
        DaysOfWeek = !string.IsNullOrEmpty(recurrence?.DaysOfWeek)
                        ? JsonSerializer.Deserialize<List<string>>(recurrence.DaysOfWeek)
                        : null,
        DayOfMonth = recurrence?.DayOfMonth,
        MonthOfYear = recurrence?.MonthOfYear
            };
        }
    }
}