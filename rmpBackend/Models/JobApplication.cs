using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobApplication
{
    public int ApplicationId { get; set; }

    public int JobId { get; set; }

    public int CandidateId { get; set; }

    public string ApplicationStatus { get; set; } = null!;

    public string? StatusReason { get; set; }

    public DateTime AppliedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? Rank { get; set; }

    public int? BulkEventId { get; set; }

    public virtual ICollection<ApplicationFeedback> ApplicationFeedbacks { get; set; } = new List<ApplicationFeedback>();

    public virtual BulkInterviewEvent? BulkEvent { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual ICollection<CandidateDocument> CandidateDocuments { get; set; } = new List<CandidateDocument>();

    public virtual ICollection<InterviewSchedule> InterviewSchedules { get; set; } = new List<InterviewSchedule>();

    public virtual JobOpening Job { get; set; } = null!;

    public virtual ICollection<ReviewerAction> ReviewerActions { get; set; } = new List<ReviewerAction>();

    public virtual ICollection<ReviewerEvaluationScore> ReviewerEvaluationScores { get; set; } = new List<ReviewerEvaluationScore>();

    public virtual ICollection<SkillAssessment> SkillAssessments { get; set; } = new List<SkillAssessment>();
}
