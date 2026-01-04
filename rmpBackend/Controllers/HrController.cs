using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using rmpBackend.Services.Email;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "hr")]
    [Route("api/[controller]")]
    [ApiController]
    public class HrController(AppDbContext db, IEmailService emailService) : ControllerBase
    {

        [HttpPut("candidate-document")]
        public async Task<IActionResult> UpdateCandidateDocument( int id,string username,[FromBody] CandidateDocumentUpdateDto dto)
        {   
            var userId =await db.Users.Where(u=>u.Username == username).Select(u=>u.UserId).FirstAsync();
            var doc = await db.CandidateDocuments.FindAsync(id);
            if (doc == null)
                return NotFound();

            if (!string.IsNullOrEmpty(dto.Status))
                doc.Status = dto.Status;

            if (!string.IsNullOrEmpty(dto.Comment))
                doc.Comment = dto.Comment;

            doc.VerifiedBy = userId;

            await db.SaveChangesAsync();
            return Ok("Updated successfully");
        }
        [HttpGet("candidate-documents")]
        public async Task<IActionResult> GetCandidateDocuments( int candidateId, int applicationId)
        {
            var docs = await db.CandidateDocuments
                .Where(d =>
                    d.CandidateId == candidateId &&
                    d.ApplicationId == applicationId)
                .Select(d => new
                {
                    d.DocumentId,
                    d.DocumentType,
                    d.FilePath,
                    d.Status,
                    d.Comment,
                    d.UploadedAt
                })
                .ToListAsync();

            return Ok(docs);
        }

        [HttpPut("job-candidate-selected")]
        public async Task<IActionResult> UpdateJobCandidateSelected(int id,string username,
    [FromBody] JobCandidateSelectedUpdateDto dto)
        {
            var userId = await db.Users.Where(u => u.Username == username).Select(u => u.UserId).FirstAsync();
            var entity = await db.JobCandidateSelecteds.FindAsync(id);
            if (entity == null)
                return NotFound("this id is not valid");
            

            if (dto.JoiningDate.HasValue)
                entity.JoiningDate = dto.JoiningDate;

            if (dto.IsMovedToEmpTable.HasValue)
            {
                
                entity.IsMovedToEmpTable = dto.IsMovedToEmpTable.Value;
                if(dto.IsMovedToEmpTable.Value)
                {   
                    var candidate = await db.Candidates.Where(c=>c.CandidateId==entity.CandidateId).FirstAsync();
                    await emailService.SendAsync(new EmailRequest
                    {
                        EventType = EmailEventType.OnBoarding,
                        ToEmails = new List<string> { candidate.Email },
                        Data = new()
                        {
                            ["CandidateName"] = candidate.Name,
                            
                        }
                    });
                }
            }

            if (dto.IsDocumentVerified.HasValue)
                entity.IsDocumentVerified = dto.IsDocumentVerified.Value;

            if (!string.IsNullOrEmpty(dto.Comment))
                entity.Comment = dto.Comment;

            entity.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Ok("Updated successfully");
        }

        [HttpGet("job-candidate-selected")]
        public async Task<IActionResult> GetAllJobCandidateSelected()
        {
            var data = await db.JobCandidateSelecteds
                .Include(s=>s.Candidate)
                .Include(s=>s.Job)
                .Select(x => new
                {
                    x.JobCandidateSelectedId,
                    x.JobId,
                    x.Job.Title,
                    x.CandidateId,
                    x.Candidate.Name,
                    x.Candidate.Email,
                    x.ApplicationId,
                    x.JoiningDate,
                    x.IsMovedToEmpTable,
                    x.IsDocumentVerified,
                    x.Comment
                })
                .ToListAsync();

            return Ok(data);
        }


    }
}
