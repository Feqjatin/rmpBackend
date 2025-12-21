 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using rmpBackend.Models;
using rmpBackend.Services;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "recruiter, admin, reviewer,candidate")]
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
        }
}

