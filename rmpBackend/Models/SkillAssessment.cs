using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class SkillAssessment
{
    public int AssessmentId { get; set; }

    public int CandidateId { get; set; }

    public int SkillId { get; set; }

    public int? ApplicationId { get; set; }

    public decimal? YearsOfExperience { get; set; }

    public int AssessedByUserId { get; set; }

    public int AssessedInRoleId { get; set; }

    public string? Stage { get; set; }

    public DateTime AssessmentDate { get; set; }

    public string? Comment { get; set; }

    public virtual JobApplication? Application { get; set; }

    public virtual User AssessedByUser { get; set; } = null!;

    public virtual Role AssessedInRole { get; set; } = null!;

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
