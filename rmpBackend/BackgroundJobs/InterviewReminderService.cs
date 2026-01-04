using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using rmpBackend.Services.Email;

namespace rmpBackend.BackgroundJobs
{
    public class InterviewReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InterviewReminderService> _logger;

        public InterviewReminderService(
            IServiceScopeFactory scopeFactory,
            ILogger<InterviewReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var upcomingInterviews = await db.InterviewSchedules
                        .Include(i => i.Application)
                            .ThenInclude(a => a.Candidate)
                        .Include(i => i.InterviewInterviewerMaps)
                            .ThenInclude(m => m.InterviewerUser)
                        .Where(i =>
                            i.ScheduledStartTime != null &&
                            i.ScheduledStartTime <= DateTime.UtcNow.AddHours(1) &&
                            i.Status == "Scheduled"
                           )
                        .ToListAsync(stoppingToken);

                    foreach (var interview in upcomingInterviews)
                    {
                        var toEmails = new List<string>();

                        
                        var interviewerEmails = interview.InterviewInterviewerMaps
                            .Select(m => m.InterviewerUser.Email)
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .ToList();
                        var recruiterId = await db.JobApplications
                                   .Where(a => a.ApplicationId == interview.ApplicationId)
                                   .Select(j => j.Job.CreatedBy).FirstAsync();
                        var recruiterEmail = await db.Users.Where(u => u.UserId == recruiterId).Select(u => u.Email).FirstOrDefaultAsync();

                        toEmails.AddRange(interviewerEmails);
                        toEmails.Add(recruiterEmail);
                        
                        var candidateEmail = interview.Application?.Candidate?.Email;
                        if (!string.IsNullOrWhiteSpace(candidateEmail))
                            toEmails.Add(candidateEmail);

                        if (!toEmails.Any())
                            continue;

                        //await emailService.SendAsync(new EmailRequest
                        //{
                        //    EventType = EmailEventType.InterviewReminder,
                        //    ToEmails = toEmails,
                        //    Data = new()
                        //    {
                        //        ["InterviewRound"] = interview.RoundSequence?.ToString() ?? "N/A",
                        //        ["InterviewId"] = interview.InterviewId.ToString(),
                        //        ["StartTime"] = interview.ScheduledStartTime?.ToString("f") ?? "N/A",
                        //         ["MeetingLink"]=interview.MeetingLink,
                        //        ["EndTime"] = interview.ScheduledEndTime?.ToString("f") ?? "N/A"
                        //    }
                        //});
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "InterviewReminderService failed");
                  
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
