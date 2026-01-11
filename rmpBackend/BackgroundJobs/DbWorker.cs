using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using rmpBackend.Queue;

namespace rmpBackend.BackgroundJobs
{
    public class DbWorker : BackgroundService
    {
            private readonly IServiceScopeFactory _scopeFactory;

            public DbWorker(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                if (InMemoryQueue.Queue.TryDequeue(out var ids))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var applicationJobs = await db.JobApplications
                        .Where(x => ids.Contains(x.ApplicationId))
                        .Select(x => new
                        {
                            x.ApplicationId,
                            x.JobId
                        })
                        .ToListAsync();

                                        var jobIds = applicationJobs
                        .Select(x => x.JobId)
                        .Distinct()
                        .ToList();

                    var roundTemplates = await db.InterviewRoundTemplates
                        .Where(rt => jobIds.Contains(rt.JobId))
                        .Select(rt => new
                        {
                            rt.RoundTemplateId,
                            rt.JobId,
                            rt.RoundOrder
                        })
                        .ToListAsync();

                    var schedules = new List<InterviewSchedule>();


                    foreach (var app in applicationJobs)
                    {
                        var roundsForJob = roundTemplates
                            .Where(rt => rt.JobId == app.JobId)
                            .OrderBy(rt => rt.RoundOrder);

                        foreach (var round in roundsForJob)
                        {
                            schedules.Add(new InterviewSchedule
                            {
                                ApplicationId = app.ApplicationId,
                                RoundTemplateId = round.RoundTemplateId,
                                RoundSequence = round.RoundOrder,

                                Status = "PENDING",
                                ScheduledStartTime = null,
                                ScheduledEndTime = null,
                                MeetingLink = null,
                                Location = null,
                                TestId = null,
                                TestScore = null,
                                RoundScore = null
                            });
                        }
                    }


                    db.InterviewSchedules.AddRange(schedules);
                    await db.SaveChangesAsync();
                    }

                    await Task.Delay(500);
                }
            }
        

    }
}
