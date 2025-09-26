using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Table("Job_Opening")]
public partial class JobOpening
{
    [Key]
    [Column("job_id")]
    public int JobId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("location")]
    [StringLength(255)]
    public string? Location { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("min_experience")]
    public int? MinExperience { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column("closed_reason")]
    public string? ClosedReason { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("JobOpenings")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Job")]
    public virtual ICollection<JobSkillMap> JobSkillMaps { get; set; } = new List<JobSkillMap>();
}
