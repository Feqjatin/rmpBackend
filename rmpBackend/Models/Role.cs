using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Table("Role")]
[Index("RoleName", Name = "UQ__Role__783254B105250013", IsUnique = true)]
public partial class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("role_name")]
    [StringLength(50)]
    public string RoleName { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Roles")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [ForeignKey("RoleId")]
    [InverseProperty("Roles")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
