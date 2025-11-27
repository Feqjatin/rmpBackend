using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class CandidateSkillMap
{
    public int CandidateId { get; set; }

    public int SkillId { get; set; }

    public string? ProficiencyLevel { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
