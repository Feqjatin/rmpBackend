using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using rmpBackend.Models;
using rmpBackend.Services;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "candidate")]
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController(AppDbContext db, RankingService rankingService) : ControllerBase
    {



        [HttpPost("update-password/{candidateId}")]
        public async Task<IActionResult> UpdatePassword(int candidateId, [FromBody] string newPassword)
        {
            var candidate = await db.Candidates.FindAsync(candidateId);
            if (candidate == null) return NotFound("Candidate not found.");


            candidate.PasswordHash = newPassword;
            await db.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }




        [HttpGet("profile/{candidateId}")]
        public async Task<IActionResult> GetProfile(int candidateId)
        {
            var candidate = await db.Candidates
                .Where(c => c.CandidateId == candidateId)
                .Select(c => new
                {
                    c.CandidateId,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.ResumePath,
                    c.Status,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.PasswordHash,
                    c.Address,
                    c.City,
                    c.State,
                    c.ZipCode,
                    c.LinkedinUrl,
                    c.GithubUrl,
                    c.PortfolioUrl,
                    c.ProfileSummary,

                    candidateDocuments = c.CandidateDocuments.Select(d => new
                    {
                        d.DocumentId,
                        d.CandidateId,
                        d.ApplicationId,
                        d.DocumentType,
                        d.FilePath,
                        d.UploadedAt
                    }).ToList(),

                    candidateEducations = c.CandidateEducations.Select(e => new
                    {
                        e.EducationId,
                        e.CandidateId,
                        e.Degree,
                        e.Institution,
                        e.FieldOfStudy,
                        e.StartDate,
                        e.EndDate,
                        e.Grade,
                        e.Description
                    }).ToList(),

                    candidateExperiences = c.CandidateExperiences.Select(ex => new
                    {
                        ex.ExperienceId,
                        ex.CandidateId,
                        ex.JobTitle,
                        ex.CompanyName,
                        ex.StartDate,
                        ex.EndDate,
                        ex.IsCurrentJob,
                        ex.Description,
                        ex.Location
                    }).ToList(),

                    candidateSkillMaps = c.CandidateSkillMaps.Select(sm => new
                    {
                        sm.CandidateId,
                        sm.SkillId,
                        sm.ProficiencyLevel,
                        Skill = new
                        {
                            sm.Skill.SkillId,
                            sm.Skill.SkillName
                        }
                    }).ToList(),

                    interviewRescheduleRequests = c.InterviewRescheduleRequests.Select(r => new
                    {
                        r.RequestId,
                        r.InterviewId,
                        r.CandidateId,
                        r.RequestedNewStartTime,
                        r.RequestedNewEndTime,
                        r.Reason,
                        r.Status,
                        r.AdminComment,
                        r.CreatedAt
                    }).ToList(),

                    jobApplications = c.JobApplications
                        .Select(j => new
                        {
                            j.ApplicationId,
                            j.CandidateId,
                            j.JobId,
                            j.AppliedAt,
                            j.UpdatedAt,
                            j.ApplicationStatus,


                            Interview = j.ApplicationStatus.ToLower() == "interview"
                                ? db.InterviewSchedules
                                    .Where(i => i.ApplicationId == j.ApplicationId)
                                    .Select(i => new
                                    {
                                        i.InterviewId,
                                        MeetingLink = i.MeetingLink,
                                        i.ScheduledStartTime,
                                        i.ScheduledEndTime,
                                        i.TestId,
                                        i.TestScore,
                                    })
                                    .FirstOrDefault()
                                : null
                        })
                        .ToList(),



                })
                .FirstOrDefaultAsync();

            if (candidate == null)
                return NotFound("Candidate not found.");

            return Ok(candidate);
        }

        [HttpPut("profile/{candidateId}")]
        public async Task<IActionResult> UpdateProfile(int candidateId, [FromBody] CandidateProfileUpdateDto dto)
        {
            var candidate = await db.Candidates.FindAsync(candidateId);
            if (candidate == null) return NotFound("Candidate not found.");

            if (dto.Name != null) candidate.Name = dto.Name;
            if (dto.Phone != null) candidate.Phone = dto.Phone;
            if (dto.Address != null) candidate.Address = dto.Address;
            if (dto.City != null) candidate.City = dto.City;
            if (dto.State != null) candidate.State = dto.State;
            if (dto.ZipCode != null) candidate.ZipCode = dto.ZipCode;
            if (dto.LinkedinUrl != null) candidate.LinkedinUrl = dto.LinkedinUrl;
            if (dto.GithubUrl != null) candidate.GithubUrl = dto.GithubUrl;
            if (dto.PortfolioUrl != null) candidate.PortfolioUrl = dto.PortfolioUrl;
            if (dto.ProfileSummary != null) candidate.ProfileSummary = dto.ProfileSummary;

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                candidate.PasswordHash = dto.NewPassword;
            }

            candidate.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }






        [HttpPost("education/{candidateId}")]
        public async Task<IActionResult> AddEducation(int candidateId, [FromBody] CandidateEducationDto dto)
        {
            var education = new CandidateEducation
            {
                CandidateId = candidateId,
                Degree = dto.Degree,
                Institution = dto.Institution,
                FieldOfStudy = dto.FieldOfStudy,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Grade = dto.Grade,
                Description = dto.Description
            };

            db.CandidateEducations.Add(education);
            await db.SaveChangesAsync();

            var result = new
            {
                education.EducationId,
                education.CandidateId,
                education.Degree,
                education.Institution,
                education.FieldOfStudy,
                education.StartDate,
                education.EndDate,
                education.Grade,
                education.Description
            };

            return Ok(result);
        }


        [HttpPut("education/{educationId}")]
        public async Task<IActionResult> UpdateEducation(int educationId, [FromBody] CandidateEducationDto dto)
        {
            var edu = await db.CandidateEducations.FindAsync(educationId);
            if (edu == null) return NotFound("Education record not found.");

            edu.Degree = dto.Degree;
            edu.Institution = dto.Institution;
            edu.FieldOfStudy = dto.FieldOfStudy;
            edu.StartDate = dto.StartDate;
            edu.EndDate = dto.EndDate;
            edu.Grade = dto.Grade;
            edu.Description = dto.Description;

            await db.SaveChangesAsync();

            var result = new
            {
                edu.EducationId,
                edu.CandidateId,
                edu.Degree,
                edu.Institution,
                edu.FieldOfStudy,
                edu.StartDate,
                edu.EndDate,
                edu.Grade,
                edu.Description
            };

            return Ok(result);
        }


        [HttpDelete("education/{educationId}")]
        public async Task<IActionResult> DeleteEducation(int educationId)
        {
            var edu = await db.CandidateEducations.FindAsync(educationId);
            if (edu == null) return NotFound();
            db.CandidateEducations.Remove(edu);
            await db.SaveChangesAsync();
            return Ok("Education deleted.");
        }




        [HttpPost("experience/{candidateId}")]
        public async Task<IActionResult> AddExperience(int candidateId, [FromBody] CandidateExperienceDto dto)
        {
            var exp = new CandidateExperience
            {
                CandidateId = candidateId,
                JobTitle = dto.JobTitle,
                CompanyName = dto.CompanyName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrentJob = dto.IsCurrentJob,
                Description = dto.Description,
                Location = dto.Location
            };

            db.CandidateExperiences.Add(exp);
            await db.SaveChangesAsync();

            var result = new
            {
                exp.ExperienceId,
                exp.CandidateId,
                exp.JobTitle,
                exp.CompanyName,
                exp.StartDate,
                exp.EndDate,
                exp.IsCurrentJob,
                exp.Description,
                exp.Location
            };

            return Ok(result);
        }
        [HttpPut("experience/{experienceId}")]
        public async Task<IActionResult> UpdateExperience(int experienceId, [FromBody] CandidateExperienceDto dto)
        {
            var exp = await db.CandidateExperiences.FindAsync(experienceId);
            if (exp == null) return NotFound("Experience record not found.");

            exp.JobTitle = dto.JobTitle;
            exp.CompanyName = dto.CompanyName;
            exp.StartDate = dto.StartDate;
            exp.EndDate = dto.EndDate;
            exp.IsCurrentJob = dto.IsCurrentJob;
            exp.Description = dto.Description;
            exp.Location = dto.Location;

            await db.SaveChangesAsync();

            var result = new
            {
                exp.ExperienceId,
                exp.CandidateId,
                exp.JobTitle,
                exp.CompanyName,
                exp.StartDate,
                exp.EndDate,
                exp.IsCurrentJob,
                exp.Description,
                exp.Location
            };

            return Ok(result);
        }



        [HttpDelete("experience/{experienceId}")]
        public async Task<IActionResult> DeleteExperience(int experienceId)
        {
            var exp = await db.CandidateExperiences.FindAsync(experienceId);
            if (exp == null) return NotFound();
            db.CandidateExperiences.Remove(exp);
            await db.SaveChangesAsync();
            return Ok("Experience deleted.");
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


        [HttpPost("skill/{candidateId}")]
        public async Task<IActionResult> AddSkill(int candidateId, [FromBody] CandidateSkillDto dto)
        {

            var skillExists = await db.Skills.AnyAsync(s => s.SkillId == dto.SkillId);
            if (!skillExists)
                return NotFound("Skill ID not found in master list.");


            var existingMap = await db.CandidateSkillMaps
                .FirstOrDefaultAsync(m => m.CandidateId == candidateId && m.SkillId == dto.SkillId);

            if (existingMap != null)
            {
                existingMap.ProficiencyLevel = dto.ProficiencyLevel;
            }
            else
            {
                var newMap = new CandidateSkillMap
                {
                    CandidateId = candidateId,
                    SkillId = dto.SkillId,
                    ProficiencyLevel = dto.ProficiencyLevel
                };
                db.CandidateSkillMaps.Add(newMap);
            }

            await db.SaveChangesAsync();

            await rankingService.UpdateForExistingCandidate(candidateId);

            return Ok("Skill added/updated successfully.");
        }

        [HttpDelete("skill")]
        public async Task<IActionResult> RemoveSkill([FromQuery] int candidateId, [FromQuery] int skillId)
        {
            var map = await db.CandidateSkillMaps
                .FirstOrDefaultAsync(m => m.CandidateId == candidateId && m.SkillId == skillId);

            if (map == null) return NotFound("Skill not found for this candidate.");

            db.CandidateSkillMaps.Remove(map);
            await db.SaveChangesAsync();
            return Ok("Skill removed.");
        }



        [HttpPost("document/{candidateId}")]
        public async Task<IActionResult> UploadDocument(int candidateId, [FromBody] CandidateDocumentDto dto)
        {
            var doc = new CandidateDocument
            {
                CandidateId = candidateId,
                ApplicationId = dto.ApplicationId,
                DocumentType = dto.DocumentType,
                FilePath = dto.FilePath,
                UploadedAt = DateTime.UtcNow
            };


            if (dto.DocumentType.ToLower() == "resume")
            {
                var candidate = await db.Candidates.FindAsync(candidateId);
                if (candidate != null)
                    candidate.ResumePath = dto.FilePath;
            }

            db.CandidateDocuments.Add(doc);
            await db.SaveChangesAsync();

            var result = new
            {
                doc.DocumentId,
                doc.CandidateId,
                doc.ApplicationId,
                doc.DocumentType,
                doc.FilePath,
                doc.UploadedAt
            };

            return Ok(result);
        }


        [HttpGet("document/{candidateId}")]
        public async Task<IActionResult> GetDocuments(int candidateId)
        {
            var docs = await db.CandidateDocuments
                .Where(d => d.CandidateId == candidateId)
                .Select(d => new
                {
                    d.DocumentId,
                    d.CandidateId,
                    d.ApplicationId,
                    d.DocumentType,
                    d.FilePath,
                    d.UploadedAt
                })
                .ToListAsync();

            return Ok(docs);
        }


        [HttpDelete("document/{documentId}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            var doc = await db.CandidateDocuments.FindAsync(documentId);
            if (doc == null) return NotFound("Document not found.");

            db.CandidateDocuments.Remove(doc);
            await db.SaveChangesAsync();

            return Ok("Document deleted successfully.");
        }




        [HttpPost("reschedule-request")]
        public async Task<IActionResult> CreateRescheduleRequest([FromBody] RescheduleRequestDto dto, [FromQuery] int candidateId)
        {
            var interview = await db.InterviewSchedules.FindAsync(dto.InterviewId);
            if (interview == null) return NotFound("Interview not found.");

            var application = await db.JobApplications.FindAsync(interview.ApplicationId);
            if (application == null || application.CandidateId != candidateId)
                return BadRequest("Invalid interview for this candidate.");

            var request = new InterviewRescheduleRequest
            {
                InterviewId = dto.InterviewId,
                CandidateId = candidateId,
                RequestedNewStartTime = dto.RequestedNewStartTime,
                RequestedNewEndTime = dto.RequestedNewEndTime,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            db.InterviewRescheduleRequests.Add(request);
            await db.SaveChangesAsync();

            var response = new RescheduleRequestResponseDto
            {
                RequestId = request.RequestId,
                InterviewId = request.InterviewId,
                CandidateId = request.CandidateId,
                RequestedNewStartTime = request.RequestedNewStartTime,
                RequestedNewEndTime = request.RequestedNewEndTime,
                Reason = request.Reason,
                Status = request.Status,
                CreatedAt = request.CreatedAt
            };

            return Ok(response);
        }


        [HttpGet("reschedule-request/{candidateId}")]
        public async Task<IActionResult> GetRescheduleRequests(int candidateId)
        {
            var requests = await db.InterviewRescheduleRequests
                .Where(r => r.CandidateId == candidateId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RescheduleRequestResponseDto
                {
                    RequestId = r.RequestId,
                    InterviewId = r.InterviewId,
                    CandidateId = r.CandidateId,
                    RequestedNewStartTime = r.RequestedNewStartTime,
                    RequestedNewEndTime = r.RequestedNewEndTime,
                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }


        [HttpDelete("reschedule-request/{requestId}")]
        public async Task<IActionResult> DeleteRescheduleRequest(int requestId)
        {
            var req = await db.InterviewRescheduleRequests.FindAsync(requestId);
            if (req == null) return NotFound();

            if (req.Status != "Pending")
            {
                return BadRequest("Cannot delete a request that has already been processed.");
            }

            db.InterviewRescheduleRequests.Remove(req);
            await db.SaveChangesAsync();
            return Ok("Reschedule request withdrawn.");
        }
    

        [HttpPost("invitationResponse")]
        public async Task<IActionResult> InvitationResponse([FromBody] CandidateResponse req)
        {
            var application = await db.JobApplications.Where(a => a.ApplicationId == req.ApplicationId).FirstAsync();
            if (application == null) return NotFound();
            application.ApplicationStatus = req.Response;
            application.StatusReason = "done by candidate";
            await db.SaveChangesAsync();
            return Ok("done");

        }
    }
}
