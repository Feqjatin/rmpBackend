 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using rmpBackend.Models;
using rmpBackend.Services;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "recruiter, admin, reviewer,candidate,interviewer")]
    [Route("api/[controller]")]
    [ApiController]
    public class UtilController(AppDbContext db, RankingService rankingService) : ControllerBase
    {
      
        [HttpPost("save-skill-assessments")]  
        public async Task<IActionResult> SaveSkillAssessmentsWithRoleStage([FromBody] SaveSkillAssessmentsWithRoleStageDto req)
        {
             
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null) return NotFound(new { message = "User not found." });

            
            var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == req.Role.ToLower());
            if (role == null) return NotFound(new { message = $"Role '{req.Role}' not found." });
             
            var candidateId = await db.JobApplications
                                        .Where(ja => ja.ApplicationId == req.ApplicationId)
                                        .Select(ja => (int?)ja.CandidateId)  
                                        .FirstOrDefaultAsync();

            if (candidateId == null) return NotFound(new { message = $"Application with ID {req.ApplicationId} not found." });


            
             

            var newAssessmentsToAdd = new List<SkillAssessment>();

            
            foreach (var item in req.Assessments)
            {
                decimal? yearsExperience = null;
                if (item.Years.HasValue)  
                {
                    yearsExperience = item.Years;
                }
                else if (item.Years != null && !string.IsNullOrWhiteSpace(item.Years.ToString()))  
                {
                    if (decimal.TryParse(item.Years.ToString(), out decimal parsedYears))
                    {
                        yearsExperience = parsedYears;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not parse years '{item.Years}' for skill {item.SkillId}");
                    }
                }

                
                     
                    newAssessmentsToAdd.Add(new SkillAssessment
                    {
                        ApplicationId = req.ApplicationId,
                        CandidateId = candidateId.Value, 
                        SkillId = item.SkillId,
                        YearsOfExperience = yearsExperience,
                        Comment = item.Comment,
                        AssessedByUserId = user.UserId,
                        AssessedInRoleId = role.RoleId, 
                        AssessmentDate = DateTime.UtcNow,
                        Stage = req.Stage 
                    });
                
            }

             
            if (newAssessmentsToAdd.Any())
            {
                await db.SkillAssessments.AddRangeAsync(newAssessmentsToAdd);
            }
 
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving skill assessments: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, new { message = "An error occurred while saving assessments." });
            }
            await rankingService.UpdateForExistingCandidate(candidateId.Value);
            return Ok(new { message = "Skill assessments saved successfully." });
        }
        [HttpGet("getSkillAssessments/{candidateId}")]
        public async Task<IActionResult> GetSkillAssessmentsForCandidate(int candidateId)
        {
         
            var assessments = await db.SkillAssessments
              
                .Where(sa => sa.CandidateId == candidateId)

                
                .Include(sa => sa.Skill)
                .Include(sa => sa.AssessedByUser)
                .Include(sa => sa.AssessedInRole)
 
                .OrderByDescending(sa => sa.AssessmentDate)
 
                .Select(sa => new SkillAssessmentViewDto
                {
                    AssessmentId = sa.AssessmentId,
                    ApplicationId = sa.ApplicationId,
                    SkillName = sa.Skill.SkillName,
                    YearsOfExperience = sa.YearsOfExperience,
                    Comment = sa.Comment,
                    AssessedByUserName = sa.AssessedByUser.Username,
                    AssessedInRoleName = sa.AssessedInRole.RoleName,
                    Stage = sa.Stage,
                    AssessmentDate = sa.AssessmentDate
                })
                .ToListAsync();


            if (assessments == null)
            {
                return Ok(new List<SkillAssessmentViewDto>());
            }

            return Ok(assessments);
        }

            [HttpPost("feedback-create")]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDto createDto)
        {
            var feedback = new ApplicationFeedback
            {
                ApplicationId = createDto.ApplicationId,
                UserId = createDto.UserId,
                UserRoleId = createDto.UserRoleId,
                FeedbackStage = createDto.FeedbackStage,
                CommentText = createDto.CommentText
            };

            db.ApplicationFeedbacks.Add(feedback);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.FeedbackId }, feedback);
        }

         
        [HttpGet("feedback/{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {

            var feedback = await db.ApplicationFeedbacks
                .Include(f => f.User)
                .Include(f => f.UserRole)
                .Where(f => f.ApplicationId == id)
                .Select(f => new FeedbackViewDto
                {
                    FeedbackId = f.FeedbackId,
                    ApplicationId = f.ApplicationId,
                    UserName = f.User.Username,
                    UserRole = f.UserRole.RoleName,
                    FeedbackStage = f.FeedbackStage,
                    CommentText = f.CommentText,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
               .ToListAsync();

            if (feedback == null)
            {
                return NotFound(id);
            }

            return Ok(feedback);
        }

        [HttpGet("feedbackByApp_id{applicationId}")]
        public async Task<IActionResult> GetFeedbackForApplication(int applicationId)
        {
            var feedbacks = await db.ApplicationFeedbacks
                .Include(f => f.User)
                .Include(f => f.UserRole)
                .Where(f => f.ApplicationId == applicationId)
                .Select(f => new FeedbackViewDto
                {
                    FeedbackId = f.FeedbackId,
                    ApplicationId = f.ApplicationId,
                    UserName = f.User.Username,
                    UserRole = f.UserRole.RoleName,
                    FeedbackStage = f.FeedbackStage,
                    CommentText = f.CommentText,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        [HttpPut("feedback-update{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackDto updateDto)
        {
            var feedback = await db.ApplicationFeedbacks.FindAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            feedback.CommentText = updateDto.CommentText;
            feedback.UpdatedAt = DateTime.UtcNow;

            db.ApplicationFeedbacks.Update(feedback);
            await db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("feedback-delete{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await db.ApplicationFeedbacks.FindAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            db.ApplicationFeedbacks.Remove(feedback);
            await db.SaveChangesAsync();

            return NoContent();
        }






        [HttpGet("jobViaId/{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await db.JobOpenings
                .Where(j=>j.JobId==id)
                .Include(j => j.JobSkillMaps)
                .ThenInclude(js => js.Skill)
                .Select(j => new
                {
                    j.JobId,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.Status,
                    j.MinExperience,
                    j.CreatedBy,
                    j.CreatedAt,
                    j.UpdatedAt,
                    j.ClosedReason,
                    Skills = j.JobSkillMaps.Select(js => new
                    {
                        js.SkillId,
                        js.Skill.SkillName,
                        js.SkillType
                    }).ToList()
                })
                .ToListAsync();

            return Ok(job);
        }

        [HttpPost("save-comment")]
        public async Task<IActionResult> SaveApplicationComment([FromBody] ApplicationCommentDto req)
        {
             
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

             
            var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == req.Role.ToLower());
            if (role == null)
            {
                return NotFound(new { message = $"Role '{req.Role}' not found." });
            }

             
            

            
            
                
                var newComment = new ApplicationFeedback
                {
                    ApplicationId = req.ApplicationId,
                    UserId = user.UserId,
                    UserRoleId = role.RoleId,
                    CommentText = req.Comment,
                    FeedbackStage = req.Role, 
                    CreatedAt = DateTime.UtcNow
                };
                db.ApplicationFeedbacks.Add(newComment);
            

          
            await db.SaveChangesAsync();
            return Ok(new { message = "Comment saved successfully." });
        }

        [HttpGet("getApplicationByJob/{jobId}")]
        public async Task<IActionResult> GetApplicationByJob(int jobId)
        {
            var applications = await db.JobApplications
                .Where(j => j.JobId == jobId)
                .Include(j => j.Candidate)
                .Select(j => new { 
                    j.ApplicationId,
                    j.CandidateId ,
                    j.Candidate.Name,
                    j.Candidate.Email,
                    j.Candidate.Phone,
                    j.ApplicationStatus,
                    j.StatusReason,
                    j.AppliedAt,
                    j.UpdatedAt,
                }
                )
                .ToListAsync();


            return Ok(applications);
        }
        [HttpGet("getMatchByJob/{jobId}")]
        public async Task<IActionResult> GetMatchByJob(int jobId)
        {
            var applications = await db.JobCandidateMatchMaps
                .Where(j => j.JobId == jobId)
                .Include(j => j.Candidate)
                .Select(j => new
                {
                    j.CandidateId,
                    j.Candidate.Name,
                    j.Candidate.Email,
                    j.Candidate.Phone,
                    j.Candidate.ResumePath,
                    j.Rank
                }
                )
                .ToListAsync();


            return Ok(applications);
        }

            [HttpGet("getMatchByCandidateId/{candidateId}")]
            public async Task<IActionResult> GetMatchByCandidate(int candidateId)
            {
                var applications = await db.JobCandidateMatchMaps
                    .Where(j => j.CandidateId == candidateId)
                    .Include(j => j.Job)
                    .Select(j => new {
                        j.JobId,
                        j.Job.Title, 
                        j.Job.Description,
                        j.Job.Status,
                        j.Job.Location,
                        j.Rank
                    }
                    )
                    .ToListAsync();


                return Ok(applications);
            }



        private async Task<(bool Success, string Message)> AddInterviewerInternal(int interviewId,int userId)
        {
            if (!await db.InterviewSchedules.AnyAsync(i => i.InterviewId == interviewId))
                return (false, $"Interview {interviewId} not found");

            if (!await db.Users.AnyAsync(u => u.UserId == userId))
                return (false, $"User {userId} not found");

            bool alreadyMapped = await db.InterviewInterviewerMaps
                .AnyAsync(m => m.InterviewId == interviewId &&
                               m.InterviewerUserId == userId);

            if (alreadyMapped)
                return (false, $"User already assigned to interview {interviewId}");

            db.InterviewInterviewerMaps.Add(new InterviewInterviewerMap
            {
                InterviewId = interviewId,
                InterviewerUserId = userId
            });

            return (true, "Added");
        }

        private async Task<(bool Success, string Message)> RemoveInterviewerInternal( int interviewId,int userId)
        {
            var mapping = await db.InterviewInterviewerMaps
                .FirstOrDefaultAsync(m =>
                    m.InterviewId == interviewId &&
                    m.InterviewerUserId == userId);

            if (mapping == null)
                return (false, $"User not assigned to interview {interviewId}");

            db.InterviewInterviewerMaps.Remove(mapping);
            return (true, "Removed");
        }
        [HttpPost("add-interviewer")]
        public async Task<IActionResult> AddInterviewer(int interviewId, int userId)
        {
            var result = await AddInterviewerInternal(interviewId, userId);

            if (!result.Success)
                return BadRequest(result.Message);

            await db.SaveChangesAsync();
            return Ok("Success");
        }

        [HttpDelete("remove-interviewer")]
        public async Task<IActionResult> RemoveInterviewer(int interviewId, int userId)
        {
            var result = await RemoveInterviewerInternal(interviewId, userId);

            if (!result.Success)
                return BadRequest(result.Message);

            await db.SaveChangesAsync();
            return Ok("Success");
        }

        [HttpPost("bulk-add-interviewer")]
        public async Task<IActionResult> BulkAddInterviewer([FromBody] BulkInterviewerRequest request)
        {
            var errors = new List<string>();

            foreach (var interviewId in request.InterviewIds)
            {
                var result = await AddInterviewerInternal(interviewId, request.UserId);
                if (!result.Success)
                    errors.Add(result.Message);
            }

            if (errors.Any())
                return BadRequest(errors);

            await db.SaveChangesAsync();
            return Ok("All interviewers added successfully");
        }

        [HttpDelete("bulk-remove-interviewer")]
        public async Task<IActionResult> BulkRemoveInterviewer([FromBody] BulkInterviewerRequest request)
        {
            var errors = new List<string>();

            foreach (var interviewId in request.InterviewIds)
            {
                var result = await RemoveInterviewerInternal(interviewId, request.UserId);
                if (!result.Success)
                    errors.Add(result.Message);
            }

            if (errors.Any())
                return BadRequest(errors);

            await db.SaveChangesAsync();
            return Ok("All interviewers removed successfully");
        }


        private void ApplyScheduleUpdate( InterviewSchedule schedule, InterviewScheduleUpdateDto dto)
        {
            if (dto.ApplicationId.HasValue)
                schedule.ApplicationId = dto.ApplicationId.Value;

            if (dto.RoundTemplateId.HasValue)
                schedule.RoundTemplateId = dto.RoundTemplateId.Value;

            if (!string.IsNullOrEmpty(dto.Status))
                schedule.Status = dto.Status;

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

            if (dto.RoundSequence.HasValue)
                schedule.RoundSequence = dto.RoundSequence.Value;
        }

        [HttpPut("schedule/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id,[FromBody] InterviewScheduleUpdateDto dto)
        {
            var schedule = await db.InterviewSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            ApplyScheduleUpdate(schedule, dto);

            await db.SaveChangesAsync();
            return Ok("success");
        }
        [HttpPut("schedule/bulk")]
        public async Task<IActionResult> BulkUpdateSchedule([FromBody] BulkInterviewScheduleUpdateDto dto)
        {
            var schedules = await db.InterviewSchedules
                .Where(s => dto.ScheduleIds.Contains(s.InterviewId))
                .ToListAsync();

            if (!schedules.Any())
                return NotFound("No schedules found");

            foreach (var schedule in schedules)
            {
                ApplyScheduleUpdate(schedule, dto.Update);
            }

            await db.SaveChangesAsync();
            return Ok(new
            {
                Message = "Bulk update successful",
                UpdatedCount = schedules.Count
            });
        }


    }
}

