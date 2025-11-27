using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class CandidateExperience
{
    public int ExperienceId { get; set; }

    public int CandidateId { get; set; }

    public string JobTitle { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool? IsCurrentJob { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;
}
