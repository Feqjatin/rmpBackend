using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Table("Permission")]
[Index("PermissionName", Name = "UQ__Permissi__81C0F5A2E7C94E42", IsUnique = true)]
public partial class Permission
{
    [Key]
    [Column("permission_id")]
    public int PermissionId { get; set; }

    [Column("permission_name")]
    [StringLength(100)]
    public string PermissionName { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [ForeignKey("PermissionId")]
    [InverseProperty("Permissions")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
