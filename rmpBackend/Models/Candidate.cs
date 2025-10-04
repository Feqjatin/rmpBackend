using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class Candidate
{
    public int CandidateId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? ResumePath { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    public virtual ICollection<SkillAssessment> SkillAssessments { get; set; } = new List<SkillAssessment>();
}
