using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ApplicationFeedback> ApplicationFeedbacks { get; set; } = new List<ApplicationFeedback>();

    public virtual ICollection<BulkInterviewEvent> BulkInterviewEvents { get; set; } = new List<BulkInterviewEvent>();

    public virtual ICollection<JobOpening> JobOpenings { get; set; } = new List<JobOpening>();

    public virtual ICollection<JobReviewerMap> JobReviewerMaps { get; set; } = new List<JobReviewerMap>();

    public virtual ICollection<SkillAssessment> SkillAssessments { get; set; } = new List<SkillAssessment>();

    public virtual ICollection<InterviewInterviewerMap> InterviewInterviewerMaps { get; set; } = new List<InterviewInterviewerMap>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
