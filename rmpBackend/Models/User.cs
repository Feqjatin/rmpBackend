using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Table("User")]
[Index("Email", Name = "UQ__User__AB6E61642A211C2A", IsUnique = true)]
[Index("Username", Name = "UQ__User__F3DBC572529596A6", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Column("email")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<JobOpening> JobOpenings { get; set; } = new List<JobOpening>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
