using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rmpBackend.Models;
using Microsoft.EntityFrameworkCore;   
using System.Linq;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "recruiter, admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RecruiterController(AppDbContext db) : ControllerBase
    {
        [HttpGet("job-all")]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await db.JobOpenings
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

            return Ok(jobs);
        }
        [HttpGet("job-all/{username}")]
        public async Task<IActionResult> GetJobsByUsername(string username)
        {
           
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { Message = $"No user found with username '{username}'" });
            }

            
            var jobs = await db.JobOpenings
                .Where(j => j.CreatedBy == user.UserId)
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

            if (jobs.Count == 0)
            {
                return NotFound(new { Message = $"No jobs found for user '{username}'" });
            }

            return Ok(jobs);
        }

        [HttpPost("job-update")]
        public async Task<IActionResult> UpdateJob([FromBody] UpdateJobDto req)
        {
            var job = await db.JobOpenings
                .Include(j => j.JobSkillMaps)
                .FirstOrDefaultAsync(j => j.JobId == req.JobId);

            if (job == null)
                return BadRequest("Job not found.");

            job.Title = req.Title;
            job.Description = req.Description;
            job.Location = req.Location;
            job.Status = req.Status;
            job.MinExperience = req.MinExperience;
            job.UpdatedAt = DateTime.UtcNow;

            var skillIds = req.Skills.Select(s => s.Id).ToList();

            var skillsToAttach = await db.Skills
                .Where(s => skillIds.Contains(s.SkillId))
                .ToListAsync();

            db.JobSkillMaps.RemoveRange(job.JobSkillMaps);
            job.JobSkillMaps = req.Skills.Select(s => new JobSkillMap
            {
                JobId = job.JobId,
                SkillId = s.Id,
                SkillType = s.Type
            }).ToList();

            await db.SaveChangesAsync();
            return Ok("Job updated successfully.");
        }



        [HttpPost("job-create")]
        public async Task<IActionResult> CreateJob([FromBody] NewJobDto req)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
            {
                return BadRequest("Invalid username.");
            }

            var job = new JobOpening
            {
                Title = req.Title,
                Description = req.Description,
                Location = req.Location,
                Status = req.Status,
                MinExperience = req.MinExperience,
                CreatedBy = user.UserId,
                CreatedAt = DateTime.UtcNow,
            };

            await db.JobOpenings.AddAsync(job);
            await db.SaveChangesAsync();

            foreach (var s in req.Skills)
            {
                db.JobSkillMaps.Add(new JobSkillMap
                {
                    JobId = job.JobId,
                    SkillId = s.Id,
                    SkillType = s.Type
                });
            }

            await db.SaveChangesAsync();
            return Ok("done");
        }



        [HttpDelete("job-delete")]
        public async Task<IActionResult> DeleteJob([FromBody] DeleteJobDto req)
        {
            var job = await db.JobOpenings
                .Include(j => j.JobSkillMaps)
                .FirstOrDefaultAsync(j => j.JobId == req.JobId);

            if (job == null) return BadRequest("Job not found.");

            db.JobSkillMaps.RemoveRange(job.JobSkillMaps);
            db.JobOpenings.Remove(job);

            await db.SaveChangesAsync();
            return Ok("Job deleted successfully.");
        }
        [HttpPost("skill-create")]
        public async Task<IActionResult> CreateSkill([FromBody] NewSkillDto req)
        {
            var skill = new Skill
            {
                SkillName = req.SkillName,
                Description = req.Description
            };

            await db.Skills.AddAsync(skill);
            await db.SaveChangesAsync();
            return Ok(skill);
        }

         
        [HttpDelete("skill-delete")]
        public async Task<IActionResult> DeleteSkill([FromBody] DeleteSkillDto req)
        {
            var skill = await db.Skills.FirstOrDefaultAsync(s => s.SkillId == req.SkillId);
            if (skill == null) return BadRequest("Skill not found.");

            db.Skills.Remove(skill);
            await db.SaveChangesAsync();
            return Ok("Skill deleted successfully.");
        }

        
        [HttpPost("skill-update")]
        public async Task<IActionResult> UpdateSkill([FromBody] UpdateSkillDto req)
        {
            var skill = await db.Skills.FirstOrDefaultAsync(s => s.SkillId == req.SkillId);
            if (skill == null) return BadRequest("Skill not found.");

            skill.SkillName = req.SkillName;
            skill.Description = req.Description;

            await db.SaveChangesAsync();
            return Ok("Skill updated successfully.");
        }
         
        [HttpGet("skill-all")]
        public async Task<IActionResult> GetAllSkills()
        {
            var skills = await db.Skills
                .Select(s => new {
                    s.SkillId,
                    s.SkillName,
                    s.Description
                })
                .ToListAsync();

            return Ok(skills);
        }


 
        [HttpPost("candidate-create")]
        public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var candidate = new Candidate
            {
                Name = req.Name,
                Email = req.Email,
                Phone = req.Phone,
                ResumePath = req.ResumePath,
                Status = req.Status,
                CreatedAt = DateTime.UtcNow // Set creation timestamp
            };

            await db.Candidates.AddAsync(candidate);
            await db.SaveChangesAsync();

            return Ok(candidate);
        }

        
        [HttpGet("candidate-all")]
        public async Task<IActionResult> GetAllCandidates()
        {
            var candidates = await db.Candidates.ToListAsync();
            return Ok(candidates);
        }

        
        [HttpGet("candidate/{id}")]
        public async Task<IActionResult> GetCandidateById(int id)
        {
            var candidate = await db.Candidates.FindAsync(id);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            return Ok(candidate);
        }


        [HttpPut("candidate-update")]  
        public async Task<IActionResult> UpdateCandidate([FromBody] UpdateCandidateDto req)  
        {
             
            var candidate = await db.Candidates.FindAsync(req.CandidateId);
            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            
            candidate.Name = req.Name;
            candidate.Email = req.Email;
            candidate.Phone = req.Phone;
            candidate.ResumePath = req.ResumePath;
            candidate.Status = req.Status;
            candidate.UpdatedAt = DateTime.UtcNow;  

            await db.SaveChangesAsync();

            return Ok("Candidate updated successfully.");
        }


        [HttpDelete("candidate-delete/{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var candidate = await db.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            db.Candidates.Remove(candidate);
            await db.SaveChangesAsync();

            return Ok("Candidate deleted successfully.");
        }




        [HttpPost("assignReviewer")]
        public async Task<IActionResult> AssignReviewer([FromBody] AssignReviewerDto req)
        {
            var user = await db.Users
               .Include(u => u.Roles)  
               .FirstOrDefaultAsync(u => u.Username == req.UserName);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

        
            var jobOpening = await db.JobOpenings.FindAsync(req.JobId);
            if (jobOpening == null)
            {
                return BadRequest("Job opening not found.");
            }
 
            bool isReviewer = user.Roles.Any(role => role.RoleName.ToLower() == "reviewer");
            if (!isReviewer)
            {
                return BadRequest("This user does not have the 'Reviewer' role and cannot be assigned.");
            }

       
            var alreadyAssigned = await db.JobReviewerMaps
                .AnyAsync(jrm => jrm.JobId == req.JobId && jrm.ReviewerUserId == user.UserId);

            if (alreadyAssigned)
            {
                return Ok("User is already assigned as a reviewer to this job opening.");
            }
 
            var jobReviewerMap = new JobReviewerMap
            {
                JobId = req.JobId,
                ReviewerUserId = user.UserId
            };

            db.JobReviewerMaps.Add(jobReviewerMap);
            await db.SaveChangesAsync();

            return Ok("Reviewer assigned to the job opening successfully!");
        }
 
        [HttpDelete("dischargeReviewer")]
        public async Task<IActionResult> DischargeReviewer([FromBody] AssignReviewerDto req)
        {
            
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.UserName);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
             
            var assignment = await db.JobReviewerMaps
                .FirstOrDefaultAsync(jrm => jrm.JobId == req.JobId && jrm.ReviewerUserId == user.UserId);

            if (assignment == null)
            {
                return Ok("This user is not assigned as a reviewer to this job opening.");
            }

            
            db.JobReviewerMaps.Remove(assignment);
            await db.SaveChangesAsync();

            return Ok("Reviewer discharged from the job opening successfully!");
        }

         
    }
}


