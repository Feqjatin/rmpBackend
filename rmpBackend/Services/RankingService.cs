using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;

namespace rmpBackend.Services
{
    public class RankingService(AppDbContext db)
    {

        [NonAction]
        public async Task UpdateForNewJob(int id)
        {

            var allCandidateIds = await db.Candidates.Select(c => c.CandidateId).ToListAsync();
            foreach (var candidateId in allCandidateIds)
            {
                await CalculateAndStoreRank(id, candidateId);

            }


        }

        [NonAction]
        public async Task UpdateForExistingJob(int id)
        {

            //var existingApplications = db.JobCandidateMatchMaps.
            //Where(ja => ja.JobId == req.Id);
            //db.JobCandidateMatchMaps.
            //RemoveRange(existingApplications);
            //await db.SaveChangesAsync();

            await UpdateForNewJob(id);
        }

        [NonAction]
        public async Task UpdateForNewCandidate(int id)
        {
            var allJobIds = await db.JobOpenings.Where(j => j.Status == "Open").Select(j => j.JobId).ToListAsync();
            foreach (var jobId in allJobIds)
            {
                await CalculateAndStoreRank(jobId, id);
            }

        }

        [NonAction]
        public async Task UpdateForExistingCandidate(int id)
        {

            //var existingApplications = db.JobCandidateMatchMaps.Where(ja => ja.CandidateId == req.Id);
            //db.JobCandidateMatchMaps.RemoveRange(existingApplications);
            //await db.SaveChangesAsync();


            await UpdateForNewCandidate(id);
        }


        private async Task CalculateAndStoreRank(int jobId, int candidateId)
        {
            const decimal requiredSkillWeight = 0.70m;
            const decimal preferredSkillWeight = 0.30m;
            const decimal experienceWeight = 0.4m;
            const decimal sentimentWeight = 0.3m;
            const decimal proficiencyWeight = 0.3m;

            var jobSkills = await db.JobSkillMaps
                .Where(jsm => jsm.JobId == jobId)
                .ToListAsync();

            var candidateAssessments = await db.SkillAssessments
                .Where(sa => sa.CandidateId == candidateId)
                .ToListAsync();

            var candidateDeclaredSkills = await db.CandidateSkillMaps
                .Where(csm => csm.CandidateId == candidateId)
                .ToListAsync();

            if (!jobSkills.Any())
            {
                var newMatch = new JobCandidateMatchMap
                {
                    JobId = jobId,
                    CandidateId = candidateId,
                    Rank = 0
                };
                db.JobCandidateMatchMaps.Add(newMatch);
                return;
            }

            decimal totalRank = 0;
            decimal totalWeight = 0;

            foreach (var jobSkill in jobSkills)
            {
                var matchingAssessments = candidateAssessments
                    .Where(ca => ca.SkillId == jobSkill.SkillId)
                    .ToList();

                var declaredSkill = candidateDeclaredSkills
                    .FirstOrDefault(cs => cs.SkillId == jobSkill.SkillId);

                decimal skillScore = 0;


                decimal experienceScore = 0;
                if (matchingAssessments.Any())
                {
                    experienceScore = matchingAssessments.Average(m => m.YearsOfExperience ?? 0);
                }

                decimal sentimentScore = 0;
                if (matchingAssessments.Any())
                {
                    sentimentScore = matchingAssessments.Average(m =>
                    {
                        if (decimal.TryParse(m.Comment ?? "5.0", out decimal sentiment))
                            return sentiment;
                        return 5.0m;
                    });
                }

                decimal proficiencyScore = 0;
                if (declaredSkill != null)
                {

                    proficiencyScore = declaredSkill.ProficiencyLevel?.ToLower() switch
                    {
                        "beginner" => 2.5m,
                        "intermediate" => 5.0m,
                        "advanced" => 7.5m,
                        "expert" => 10.0m,
                        _ => 0m
                    };
                }


                skillScore = (experienceScore * experienceWeight) +
                             (sentimentScore * sentimentWeight) +
                             (proficiencyScore * proficiencyWeight);


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


            var existingMatch = await db.JobCandidateMatchMaps
                .FirstOrDefaultAsync(jcm => jcm.CandidateId == candidateId && jcm.JobId == jobId);

            if (existingMatch != null)
            {

                existingMatch.Rank = (int)Math.Round(finalRank);
            }
            else
            {

                var newMatch = new JobCandidateMatchMap
                {
                    JobId = jobId,
                    CandidateId = candidateId,
                    Rank = (int)Math.Round(finalRank)
                };
                db.JobCandidateMatchMaps.Add(newMatch);
            }




            await db.SaveChangesAsync();
        }
    }
}
