using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Table("Skill")]
public partial class Skill
{
    [Key]
    [Column("skill_id")]
    public int SkillId { get; set; }

    [Column("skill_name")]
    [StringLength(100)]
    public string SkillName { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [InverseProperty("Skill")]
    public virtual ICollection<JobSkillMap> JobSkillMaps { get; set; } = new List<JobSkillMap>();
}
