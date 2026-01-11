using rmpBackend.Models;
using rmpBackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Services.Evalution
{
    public class ApplicationEvaluationService : IApplicationEvaluationService
    {
        private readonly AppDbContext db;

        public ApplicationEvaluationService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<List<ApplicationFinalScoreDto>> EvaluateApplicationsByJob(int jobId)
        {
            var applications = await db.JobApplications
                .Where(a => a.JobId == jobId)
                .Where(a => a.InterviewSchedules.Any())
                .Where(a => a.InterviewSchedules.All(i =>
                    i.Status == "Completed" || i.Status == "Canceled"))

                .Select(a => new
                {
                    a.ApplicationId,
                    a.CandidateId,

                    Interviews = a.InterviewSchedules
                        .Where(i => i.Status == "Completed" && i.RoundScore.HasValue)
                        .Select(i => new
                        {
                            Score = i.RoundScore!.Value,
                            Weight = i.RoundTemplate.Weightage
                        })
                })
                .ToListAsync();

            var result = new List<ApplicationFinalScoreDto>();

            foreach (var app in applications)
            {
                if (!app.Interviews.Any())
                    continue;

                decimal totalWeightedScore = app.Interviews.Sum(i => i.Score * i.Weight);
                decimal totalWeight = app.Interviews.Sum(i => i.Weight);

                decimal finalScore = totalWeight == 0
                    ? 0
                    : totalWeightedScore / totalWeight;


                result.Add(new ApplicationFinalScoreDto
                {
                    ApplicationId = app.ApplicationId,
                    CandidateId = app.CandidateId,
                    FinalWeightedScore = Math.Round(finalScore, 2),
                    SentimentScore = 0,
                    //send ApplicationFeedback,skillAssessment and JobSkillMap to Sentiment analysis, not implemented yet
                });
            }

            return result;
        }
    }

}
