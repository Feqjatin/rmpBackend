using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobSkillMap
{
    public int JobId { get; set; }

    public int SkillId { get; set; }

    public string? SkillType { get; set; }

    public virtual JobOpening Job { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
