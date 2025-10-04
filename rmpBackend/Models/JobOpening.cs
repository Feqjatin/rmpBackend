using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobOpening
{
    public int JobId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public int? MinExperience { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ClosedReason { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    public virtual ICollection<JobReviewerMap> JobReviewerMaps { get; set; } = new List<JobReviewerMap>();

    public virtual ICollection<JobSkillMap> JobSkillMaps { get; set; } = new List<JobSkillMap>();
}
