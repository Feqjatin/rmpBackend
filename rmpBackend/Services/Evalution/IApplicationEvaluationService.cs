
using rmpBackend.Models.DTOs;

namespace rmpBackend.Services.Evalution
{
    public interface IApplicationEvaluationService
    {
        Task<List<ApplicationFinalScoreDto>> EvaluateApplicationsByJob(int jobId);
    }

}
