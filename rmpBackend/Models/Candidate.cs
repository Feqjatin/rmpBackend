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

    public string? PasswordHash { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? LinkedinUrl { get; set; }

    public string? GithubUrl { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? ProfileSummary { get; set; }

    public virtual ICollection<CandidateDocument> CandidateDocuments { get; set; } = new List<CandidateDocument>();

    public virtual ICollection<CandidateEducation> CandidateEducations { get; set; } = new List<CandidateEducation>();

    public virtual ICollection<CandidateExperience> CandidateExperiences { get; set; } = new List<CandidateExperience>();

    public virtual ICollection<CandidateSkillMap> CandidateSkillMaps { get; set; } = new List<CandidateSkillMap>();

    public virtual ICollection<InterviewRescheduleRequest> InterviewRescheduleRequests { get; set; } = new List<InterviewRescheduleRequest>();

    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    public virtual ICollection<JobCandidateMatchMap> JobCandidateMatchMaps { get; set; } = new List<JobCandidateMatchMap>();

    public virtual ICollection<JobCandidateSelected> JobCandidateSelecteds { get; set; } = new List<JobCandidateSelected>();

    public virtual ICollection<SkillAssessment> SkillAssessments { get; set; } = new List<SkillAssessment>();
}
