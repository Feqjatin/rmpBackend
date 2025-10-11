using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "interviewer")]
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewerController(AppDbContext db) : ControllerBase
    {
        [HttpPost("schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] InterviewScheduleDto dto)
        {
            var schedule = new InterviewSchedule
            {
                ApplicationId = dto.ApplicationId,
                RoundTemplateId = dto.RoundTemplateId,
                Status = dto.Status,
                ScheduledStartTime = dto.ScheduledStartTime,
                ScheduledEndTime = dto.ScheduledEndTime,
                MeetingLink = dto.MeetingLink,
                Location = dto.Location
            };
            db.InterviewSchedules.Add(schedule);
            await db.SaveChangesAsync();
            return Ok(schedule);
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetAllSchedules()
        {
            return Ok(await db.InterviewSchedules.ToListAsync());
        }

        [HttpGet("schedule/{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var schedule = await db.InterviewSchedules.FindAsync(id);
            return schedule == null ? NotFound() : Ok(schedule);
        }

        [HttpGet("schedule/by-application/{applicationId}")]
        public async Task<IActionResult> GetSchedulesByApplicationId(int applicationId)
        {
            var schedules = await db.InterviewSchedules
                .Where(s => s.ApplicationId == applicationId)
                .OrderBy(s => s.ScheduledStartTime)
                .ToListAsync();
            return Ok(schedules);
        }

        [HttpPut("schedule/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] InterviewScheduleDto dto)
        {
            var schedule = await db.InterviewSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            schedule.ApplicationId = dto.ApplicationId;
            schedule.RoundTemplateId = dto.RoundTemplateId;
            schedule.Status = dto.Status;
            schedule.ScheduledStartTime = dto.ScheduledStartTime;
            schedule.ScheduledEndTime = dto.ScheduledEndTime;
            schedule.MeetingLink = dto.MeetingLink;
            schedule.Location = dto.Location;

            await db.SaveChangesAsync();
            return Ok(schedule);
        }

        [HttpDelete("schedule/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await db.InterviewSchedules.FindAsync(id);
            if (schedule == null) return NotFound();
            db.InterviewSchedules.Remove(schedule);
            await db.SaveChangesAsync();
            return Ok("Schedule deleted successfully.");
        }

        [HttpPost("interviewer")]
        public async Task<IActionResult> AssignInterviewer([FromBody] InterviewInterviewerMapDto dto)
        {
            var assignment = new InterviewInterviewerMap
            {
                InterviewId = dto.InterviewId,
                InterviewerUserId = dto.InterviewerUserId
            };
            db.InterviewInterviewerMaps.Add(assignment);
            await db.SaveChangesAsync();
            return Ok(assignment);
        }

        [HttpGet("interviewer")]
        public async Task<IActionResult> GetAllAssignments()
        {
            return Ok(await db.InterviewInterviewerMaps.ToListAsync());
        }

        [HttpGet("interviewer/by-interview/{interviewId}")]
        public async Task<IActionResult> GetAssignmentsByInterview(int interviewId)
        {
            return Ok(await db.InterviewInterviewerMaps.Where(m => m.InterviewId == interviewId).ToListAsync());
        }

        [HttpGet("interviewer/by-user/{userId}")]
        public async Task<IActionResult> GetAssignmentsByUser(int userId)
        {
            return Ok(await db.InterviewInterviewerMaps.Where(m => m.InterviewerUserId == userId).ToListAsync());
        }

        [HttpDelete("interviewer")]
        public async Task<IActionResult> DeleteAssignment([FromQuery] int interviewId, [FromQuery] int interviewerUserId)
        {
            var assignment = await db.InterviewInterviewerMaps
                .FirstOrDefaultAsync(m => m.InterviewId == interviewId && m.InterviewerUserId == interviewerUserId);

            if (assignment == null) return NotFound();

            db.InterviewInterviewerMaps.Remove(assignment);
            await db.SaveChangesAsync();
            return Ok("Interviewer assignment deleted successfully.");
        }




    }
}
