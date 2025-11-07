using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "interviewer, admin")]
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

    

        [HttpGet("schedule/by-user/{userName}")]
        public async Task<IActionResult> GetSchedulesByUserName(string username)
        {

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(username);
            }


            var schedules = await db.InterviewSchedules

                .Include(s => s.RoundTemplate)
                .Include(s => s.Application.Candidate)
                .Include(s => s.Application.Job.JobSkillMaps)
                    .ThenInclude(jsm => jsm.Skill)
                .Include(s => s.InterviewInterviewerMaps)
                    .ThenInclude(map => map.InterviewerUser)


                .Where(s => s.InterviewInterviewerMaps.Any(map => map.InterviewerUserId == user.UserId))
                .OrderBy(s => s.ScheduledStartTime)
                .Select(s => new
                {

                    InterviewId = s.InterviewId,
                    Status = s.Status,
                    ScheduledStartTime = s.ScheduledStartTime,
                    ScheduledEndTime = s.ScheduledEndTime,
                    MeetingLink = s.MeetingLink,
                    Location = s.Location,
                    applicationId =s.ApplicationId,

                    RoundInfo = new
                    {
                        s.RoundTemplate.RoundOrder,
                        s.RoundTemplate.RoundName,
                        s.RoundTemplate.RoundType
                    },

                    CandidateInfo = new
                    {   s.Application.Candidate.CandidateId,
                        s.Application.Candidate.Name,
                        s.Application.Candidate.Email,
                        s.Application.Candidate.Phone
                    },


                    Interviewers = s.InterviewInterviewerMaps.Select(m => new
                    {
                        m.InterviewerUser.Username,
                        m.InterviewerUser.Email
                    }).ToList(),

                    JobInfo = new
                    {
                        s.Application.Job.JobId,
                        s.Application.Job.Title,
                        s.Application.Job.Description,
                        JobLocation = s.Application.Job.Location,
                        JobStatus = s.Application.Job.Status,
                        s.Application.Job.MinExperience,
                        s.Application.Job.CreatedBy,
                        s.Application.Job.CreatedAt,
                        s.Application.Job.UpdatedAt,
                        s.Application.Job.ClosedReason,
                        Skills = s.Application.Job.JobSkillMaps.Select(js => new
                        {
                            js.SkillId,
                            js.Skill.SkillName,
                            js.SkillType
                        }).ToList()
                    }
                })
                .ToListAsync();

            return Ok(schedules);
        }
            //[HttpGet("schedule/{id}")]
            //public async Task<IActionResult> GetScheduleById(int id)
            //{
            //    var schedule = await db.InterviewSchedules.FindAsync(id);
            //    return schedule == null ? NotFound() : Ok(schedule);
            //}


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
