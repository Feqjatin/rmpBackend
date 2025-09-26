using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Table("Candidate")]
[Index("Email", Name = "UQ__Candidat__AB6E6164A031D789", IsUnique = true)]
public partial class Candidate
{
    [Key]
    [Column("candidate_id")]
    public int CandidateId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("resume_path")]
    public string? ResumePath { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
