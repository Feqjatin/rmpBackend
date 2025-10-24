 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using rmpBackend.Models;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "recruiter, admin, reviewer")]
    [Route("api/[controller]")]
    [ApiController]
    public class UtilController(AppDbContext db ) : ControllerBase
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

            return Ok(new { message = "Skill assessments saved successfully." });
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

         
        [HttpGet("feedback{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await db.ApplicationFeedbacks
                .Include(f => f.User)
                .Include(f => f.UserRole)
                .Where(f => f.FeedbackId == id)
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
                .FirstOrDefaultAsync();

            if (feedback == null)
            {
                return NotFound();
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


        [HttpPost("updateForNewJob")]
        public async Task<IActionResult> UpdateForNewJob([FromBody] JobMatchingRequestDto req)
        {
            var allCandidateIds = await db.Candidates.Select(c => c.CandidateId).ToListAsync();
            foreach (var candidateId in allCandidateIds)
            {
                await CalculateAndStoreRank(req.Id, candidateId);
            }
            return Ok($"Ranking complete for new job {req.Id} against {allCandidateIds.Count} candidates.");
        }

        [HttpPost("updateForExistingJob")]
        public async Task<IActionResult> UpdateForExistingJob([FromBody] JobMatchingRequestDto req)
        {

            var existingApplications = db.JobApplications.Where(ja => ja.JobId == req.Id);
            db.JobApplications.RemoveRange(existingApplications);
            await db.SaveChangesAsync();

            return await UpdateForNewJob(req);
        }

         
        [HttpPost("updateForNewCandidate")]
        public async Task<IActionResult> UpdateForNewCandidate([FromBody] JobMatchingRequestDto req)
        {
            var allJobIds = await db.JobOpenings.Where(j => j.Status == "Open").Select(j => j.JobId).ToListAsync();
            foreach (var jobId in allJobIds)
            {
                await CalculateAndStoreRank(jobId, req.Id);
            }
            return Ok($"Ranking complete for new candidate {req.Id} against {allJobIds.Count} open jobs.");
        }

         
        [HttpPost("updateForExistingCandidate")]
        public async Task<IActionResult> UpdateForExistingCandidate([FromBody] JobMatchingRequestDto req)
        {
             
            var existingApplications = db.JobApplications.Where(ja => ja.CandidateId == req.Id);
            db.JobApplications.RemoveRange(existingApplications);
            await db.SaveChangesAsync();

           
            return await UpdateForNewCandidate(req);
        }


      
        private async Task CalculateAndStoreRank(int jobId, int candidateId)
        { 
            const decimal requiredSkillWeight = 0.70m;
            const decimal preferredSkillWeight = 0.30m;
            const decimal experienceWeight = 0.6m;
            const decimal sentimentWeight = 0.4m;
 
            var jobSkills = await db.JobSkillMaps
                .Where(jsm => jsm.JobId == jobId)
                .ToListAsync();

            var candidateAssessments = await db.SkillAssessments
                .Where(sa => sa.CandidateId == candidateId)
                .ToListAsync();

            if (!jobSkills.Any()) return;  

            decimal totalRank = 0;
            decimal totalWeight = 0;
 
            foreach (var jobSkill in jobSkills)
            {
                var matchingAssessments = candidateAssessments.Where(ca => ca.SkillId == jobSkill.SkillId).ToList();
                decimal skillScore = 0;

                if (matchingAssessments.Any())
                {
                     
                    decimal avgExperience = matchingAssessments.Average(m => m.YearsOfExperience ?? 0);
                    decimal avgSentiment = matchingAssessments.Average(m => decimal.Parse(m.Comment ?? "5.0"));  

                    
                    skillScore = (avgExperience * experienceWeight) + (avgSentiment * sentimentWeight);
                }
 
                bool isRequired = jobSkill.SkillType?.ToLower() == "required";
                if (isRequired)
                {
                    totalRank += skillScore * requiredSkillWeight;
                    totalWeight += requiredSkillWeight;
                }
                else
                {
                    totalRank += skillScore * preferredSkillWeight;
                    totalWeight += preferredSkillWeight;
                }
            }

          
            decimal finalRank = (totalWeight > 0) ? (totalRank / totalWeight) * 10 : 0;
            if (finalRank > 100) finalRank = 100;


        
            var newApplication = new JobApplication
            {
                JobId = jobId,
                CandidateId = candidateId,
                ApplicationStatus = "Ranked",
                AppliedAt = DateTime.UtcNow,
                Rank = finalRank
            };

            db.JobApplications.Add(newApplication);
            await db.SaveChangesAsync();
        }
        //[HttpPost("bulk-event")]
        //public async Task<IActionResult> CreateBulkEvent([FromBody] BulkInterviewEventDto dto)
        //{
        //    var bulkEvent = new BulkInterviewEvent
        //    {
        //        EventName = dto.EventName,
        //        EventDate = DateOnly.FromDateTime(dto.EventDate),
        //        Location = dto.Location,
        //        Description = dto.Description,
        //        CreatedByUserId = dto.CreatedByUserId
        //    };
        //    db.BulkInterviewEvents.Add(bulkEvent);
        //    await db.SaveChangesAsync();
        //    return Ok(bulkEvent);
        //}




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

             
            var existingComment = await db.ApplicationFeedbacks
                .FirstOrDefaultAsync(f =>
                    f.ApplicationId == req.ApplicationId &&
                    f.UserId == user.UserId &&
                    f.UserRoleId == role.RoleId);

            if (existingComment != null)
            {
                 
                existingComment.CommentText = req.Comment;
                existingComment.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                
                var newComment = new ApplicationFeedback
                {
                    ApplicationId = req.ApplicationId,
                    UserId = user.UserId,
                    UserRoleId = role.RoleId,
                    CommentText = req.Comment,
                    FeedbackStage = "Review", 
                    CreatedAt = DateTime.UtcNow
                };
                db.ApplicationFeedbacks.Add(newComment);
            }

          
            await db.SaveChangesAsync();
            return Ok(new { message = "Comment saved successfully." });
        }
    }
}

