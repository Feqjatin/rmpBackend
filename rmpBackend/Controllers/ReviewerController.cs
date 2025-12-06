using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
 

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "reviewer")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewerController(AppDbContext db) : ControllerBase
    {
        
        [HttpGet("dashboard/{userName}")]
        public async Task<IActionResult> GetReviewerDashboard(string userName)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var assignedJobs = await db.JobReviewerMaps
                .Where(jrm => jrm.ReviewerUserId == user.UserId)
                .Include(jrm => jrm.Job)  
                .Select(jrm => jrm.Job)
                .ToListAsync();

            var dashboardResults = new List<ReviewerDashboardDto>();

            foreach (var job in assignedJobs)
            {
                var applicationsInThisJob = await db.JobApplications
                    .Where(ja => ja.JobId == job.JobId)
                    .Select(ja => ja.ApplicationId)
                    .ToListAsync();

                var reviewerActions = await db.ReviewerActions
                    .Where(ra => ra.ReviewerUserId == user.UserId && applicationsInThisJob.Contains(ra.ApplicationId))
                    .ToDictionaryAsync(ra => ra.ApplicationId, ra => ra);

                int acceptedCount = 0;
                int rejectedCount = 0;
                int onHoldCount = 0;
                int newCount = 0;
                int publishedCount = 0;

                foreach (var appId in applicationsInThisJob)
                {
                    if (reviewerActions.TryGetValue(appId, out var action))
                    {
                        if (action.Status == "Accepted") acceptedCount++;
                        else if (action.Status == "Rejected") rejectedCount++;
                        else if (action.Status == "OnHold") onHoldCount++;

                        if (action.IsPublished) publishedCount++;
                    }
                    else
                    {
                        newCount++;
                    }
                }

                dashboardResults.Add(new ReviewerDashboardDto
                {
                    JobId = job.JobId,
                    JobTitle = job.Title,
                    Accepted = acceptedCount,
                    Rejected = rejectedCount,
                    OnHold = onHoldCount,
                    New = newCount,
                    Published = publishedCount,
                    Total=acceptedCount+rejectedCount+onHoldCount+newCount,
                });
            }

            return Ok(dashboardResults);
        }
        [HttpGet("getApplicationsForReviewer/{jobId}")]
        public async Task<IActionResult> GetApplicationsForReviewer(int jobId)
        {
            var applications = await db.JobApplications
                .Where(ja => ja.JobId == jobId)
                .Select(ja => new { ja.ApplicationId,ja.CandidateId})
                 
                .ToListAsync();

            if (!applications.Any())
            {
                return Ok(new List<ReviewerApplicationActionDto>());
            }

            var applicationIds = applications.Select(a => a.ApplicationId).ToList();

            var existingActions = await db.ReviewerActions
                .Where(ra => applicationIds.Contains(ra.ApplicationId))
                .ToDictionaryAsync(ra => ra.ApplicationId);

            var results = applications.Select(app =>
            {
                if (existingActions.TryGetValue(app.ApplicationId, out var action))
                {
                    return new ReviewerApplicationActionDto
                    {
                        ApplicationId = app.ApplicationId,
                        CandidateId=app.CandidateId,
                        ReviewerUserId = action.ReviewerUserId,
                        Status = action.Status,
                        IsPublished = action.IsPublished,
                        PersonalNote = action.PersonalNote,
                        ActionDate = action.ActionDate
                    };
                }
                else
                {
                    return new ReviewerApplicationActionDto
                    {
                        ApplicationId = app.ApplicationId,
                        CandidateId = app.CandidateId,
                        ReviewerUserId = null,
                        Status = "New",
                        IsPublished = false,
                        PersonalNote = "n/a",
                        ActionDate = null
                    };
                }
            }).ToList();

            return Ok(results);
        }




        [HttpPost("bulk-update-status")]
        public async Task<IActionResult> BulkUpdateApplicationStatus([FromBody] BulkReviewerActionDto req)
        {
            if (req == null || req.Ids == null || !req.Ids.Any())
            {
                return BadRequest("No application IDs provided.");
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var existingActions = await db.ReviewerActions
                .Where(ra => ra.ReviewerUserId == user.UserId && req.Ids.Contains(ra.ApplicationId))
                .ToDictionaryAsync(ra => ra.ApplicationId);

            var newActionsToAdd = new List<ReviewerAction>();

            
            bool isPublishedStatus = req.Status?.Equals("Published", StringComparison.OrdinalIgnoreCase) ?? false;
            if (isPublishedStatus)
            {
                foreach (var appId in req.Ids)
                {
                    if (existingActions.TryGetValue(appId, out var actionToUpdate))
                    {

                 
                        actionToUpdate.ActionDate = DateTime.UtcNow;
                        actionToUpdate.IsPublished = true;
                    }
                    else
                    {
                         return BadRequest("application id not found");
                    }
                }
            }
            else
            {
                foreach (var appId in req.Ids)
                {
                    if (existingActions.TryGetValue(appId, out var actionToUpdate))
                    {

                        actionToUpdate.Status = req.Status;
                        actionToUpdate.ActionDate = DateTime.UtcNow;
                        
                    }
                    else
                    {

                        var newAction = new ReviewerAction
                        {
                            ApplicationId = appId,
                            ReviewerUserId = user.UserId,
                            Status = req.Status,
                            ActionDate = DateTime.UtcNow,
                            IsPublished = isPublishedStatus,
                            PersonalNote = "Bulk update action."
                        };
                        newActionsToAdd.Add(newAction);
                    }
                }
            }

            if (newActionsToAdd.Any())
            {
                await db.ReviewerActions.AddRangeAsync(newActionsToAdd);
            }

            await db.SaveChangesAsync();

            return Ok(new { message = $"Successfully processed {req.Ids.Count} applications." });
        }


        [HttpPost("update-note")]
        public async Task<IActionResult> UpdatePersonalNote([FromBody] UpdateNoteDto req)
        {
            if (req == null)
            {
                return BadRequest("Invalid request.");
            }

             
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
            {
                return NotFound("User not found.");
            }
 
            var action = await db.ReviewerActions
                .FirstOrDefaultAsync(ra => ra.ApplicationId == req.Id && ra.ReviewerUserId == user.UserId);

            if (action != null)
            {
                
                action.PersonalNote = req.PersonalNote;
                action.ActionDate = DateTime.UtcNow;
            }
            else
            { 
                var newAction = new ReviewerAction
                {
                    ApplicationId = req.Id,
                    ReviewerUserId = user.UserId,
                    PersonalNote = req.PersonalNote,
                    ActionDate = DateTime.UtcNow,
                    Status = "New", 
                    IsPublished = false 
                };
                db.ReviewerActions.Add(newAction);
                 

            }

             
            await db.SaveChangesAsync();

            return Ok(new { message = "Note updated successfully." });
        }
    }
}
