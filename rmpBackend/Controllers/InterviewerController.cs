using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using rmpBackend.Services.Email;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "interviewer, admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewerController(AppDbContext db,IEmailService emailService) : ControllerBase
    {
        [HttpPost("schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] InterviewScheduleDto dto)
        {
            var schedule = new InterviewSchedule
            {
                ApplicationId = dto.ApplicationId,
                RoundTemplateId = dto.RoundTemplateId,
                Status = "PENDING",
                
                MeetingLink = dto.MeetingLink,
                Location = dto.Location,
                RoundSequence = dto.RoundSequence

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
                    ApplicationId =s.ApplicationId,
                    RoundScore = s.RoundScore,
                    TestId=s.TestId,
                    TestScore=s.TestScore,

                    RoundInfo = new
                    {
                        s.RoundSequence,
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
                        m.InterviewerUser.Email,
                        m.InterviewerUser.UserId
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
        public async Task<IActionResult> UpdateSchedule(int id,[FromBody] InterviewScheduleUpdateDto dto)
        {
            var schedule = await db.InterviewSchedules
             .Include(s => s.Application)
                 .ThenInclude(a => a.Candidate)
             .FirstOrDefaultAsync(s => s.InterviewId == id);

            if (schedule == null)
                return NotFound();


            if (dto.ApplicationId.HasValue)
                schedule.ApplicationId = dto.ApplicationId.Value;

            if (dto.RoundTemplateId.HasValue)
                schedule.RoundTemplateId = dto.RoundTemplateId.Value;
            List<string> ToEmail = new List<string>();
            if (!string.IsNullOrEmpty(dto.Status))
            {
                schedule.Status = dto.Status;
               
                var allInterviewerEmail = await db.InterviewInterviewerMaps
                   .Where(i => i.InterviewId == id)
                   .Select(i => i.InterviewerUser.Email)
                   .ToListAsync();
                var recruiterId = await db.JobApplications
                    .Where(a => a.ApplicationId == schedule.ApplicationId)
                    .Select(j => j.Job.CreatedBy).FirstAsync();
                var recruiterEmail = await db.Users.Where(u => u.UserId == recruiterId).Select(u => u.Email).FirstOrDefaultAsync();
                ToEmail.AddRange(allInterviewerEmail);
                ToEmail.Add(recruiterEmail);


                await emailService.SendAsync(new EmailRequest
                {
                    EventType = EmailEventType.CandidateMovedToNextRound,
                    ToEmails =  ToEmail,
                    Data = new()
                    {
                        ["CandidateName"] = schedule.Application.Candidate.Name,
                        ["CandidateMail"] = schedule.Application.Candidate.Email,
                        ["MeetingLink"] =schedule.MeetingLink,
                        ["ScheduleSeq"] =schedule.RoundSequence.ToString(),
                        ["ApplicationId"] = schedule.ApplicationId.ToString()
                    }
                });
            }

            if (dto.ScheduledStartTime.HasValue)
                schedule.ScheduledStartTime = dto.ScheduledStartTime.Value;

            if (dto.ScheduledEndTime.HasValue)
                schedule.ScheduledEndTime = dto.ScheduledEndTime.Value;

            if (!string.IsNullOrEmpty(dto.MeetingLink))
                schedule.MeetingLink = dto.MeetingLink;

            if (!string.IsNullOrEmpty(dto.Location))
                schedule.Location = dto.Location;

            if (dto.TestScore.HasValue)
                schedule.TestScore = dto.TestScore.Value;

            if (dto.TestId.HasValue)
                schedule.TestId = dto.TestId.Value;

            if (dto.RoundScore.HasValue)
                schedule.RoundScore = dto.RoundScore.Value;
            if(dto.RoundSequence.HasValue)
                schedule.RoundSequence= dto.RoundSequence.Value;

            await db.SaveChangesAsync();
            return Ok(ToEmail);
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

        [HttpGet("reschedule-request/{username}")]
        public async Task<IActionResult> GetRescheduleRequest(string username)
        {
            
            var userId = await db.Users
                .Where(u => u.Username == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            if (userId == 0)
                return NotFound("User not found");

             
            var interviewIds = await db.InterviewInterviewerMaps
                .Where(i => i.InterviewerUserId == userId)
                .Select(i => i.InterviewId)
                .ToListAsync();

            if (!interviewIds.Any())
                return Ok(new List<object>());

             
            var requests = await db.InterviewRescheduleRequests
                .Where(r => interviewIds.Contains(r.InterviewId))
                .Include(r => r.Candidate)
                .Include(r => r.Interview)
                .Select(r => new
                {
                    r.RequestId,
                    r.InterviewId,
                    r.CandidateId,
                    CandidateName = r.Candidate.Name,
                    r.RequestedNewStartTime,
                    r.RequestedNewEndTime,
                    r.Reason,
                    r.Status,
                    r.AdminComment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("reschedule-request")]
        public async Task<IActionResult> UpdateRescheduleRequest([FromBody] UpdateRescheduleRequestDto req)
        {
            var request = await db.InterviewRescheduleRequests
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
                return NotFound("Reschedule request not found");

            request.Status = req.Status;
            request.AdminComment = req.AdminComment;
            

            await db.SaveChangesAsync();

            return Ok(new { message = "Reschedule request updated successfully" });
        }

         
        [HttpGet("getRounds/{applicationId}")]
        public async Task<IActionResult> GetRounds(int applicationId)
        {
            var allInterview = await db.InterviewSchedules
                .Where(i => i.ApplicationId == applicationId)
                .Include(i => i.RoundTemplate)
                .Select(i=>new
                {
                    i.ApplicationId,
                    i.InterviewId,
                    i.RoundTemplateId,
                    i.RoundTemplate.JobId,
                    i.RoundTemplate.Weightage,
                    i.RoundTemplate.Description,
                    i.RoundTemplate.RoundName,
                    i.RoundTemplate.RoundType,
                    i.RoundTemplate.RoundOrder,
                    i.RoundTemplate.IsCustomRound,
                   i.Status,
                   i.ScheduledEndTime,
                   i.ScheduledStartTime,
                   i.MeetingLink,
                   i.Location,
                   i.TestId,
                   i.TestScore,
                   i.RoundScore,
                   i.RoundSequence



    })
                .ToArrayAsync();
            if(!allInterview.Any())
            {
                return NotFound("no interview found");
            }

            return Ok(allInterview);
        }


    }
}
