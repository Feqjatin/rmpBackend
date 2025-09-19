using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<JobSkillMap> JobSkillMaps { get; set; } = new List<JobSkillMap>();
}
