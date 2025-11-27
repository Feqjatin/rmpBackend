using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class CandidateEducation
{
    public int EducationId { get; set; }

    public int CandidateId { get; set; }

    public string Degree { get; set; } = null!;

    public string Institution { get; set; } = null!;

    public string? FieldOfStudy { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Grade { get; set; }

    public string? Description { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;
}
