using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[PrimaryKey("JobId", "SkillId")]
[Table("Job_Skill_Map")]
public partial class JobSkillMap
{
    [Key]
    [Column("job_id")]
    public int JobId { get; set; }

    [Key]
    [Column("skill_id")]
    public int SkillId { get; set; }

    [Column("skill_type")]
    [StringLength(50)]
    public string? SkillType { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("JobSkillMaps")]
    public virtual JobOpening Job { get; set; } = null!;

    [ForeignKey("SkillId")]
    [InverseProperty("JobSkillMaps")]
    public virtual Skill Skill { get; set; } = null!;
}
