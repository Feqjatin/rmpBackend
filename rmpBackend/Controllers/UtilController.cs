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
        [HttpPost("addSkillAssessment")]
        public async Task<IActionResult> AddOrUpdateAssessment([FromBody] SkillAssessmentDto req)
        { 

            var assessment = new SkillAssessment
            {
                CandidateId = req.CandidateId,
                SkillId = req.SkillId,
                ApplicationId = req.ApplicationId,
                YearsOfExperience = req.YearsOfExperience,
                AssessedByUserId = req.AssessedByUserId,
                AssessedInRoleId = req.AssessedInRoleId,
                Comment = req.Comment,
                AssessmentDate = DateTime.UtcNow
            };

            db.SkillAssessments.Add(assessment);
            await db.SaveChangesAsync();



            return Ok(new { message = "Skill assessment recorded successfully.", assessmentId = assessment.AssessmentId });
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
    }
}

