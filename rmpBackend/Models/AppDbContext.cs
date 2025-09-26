using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<JobOpening> JobOpenings { get; set; }

    public virtual DbSet<JobSkillMap> JobSkillMaps { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=jatinYY;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.CandidateId).HasName("PK__Candidat__39BD4C18A7C26F08");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<JobOpening>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Job_Open__6E32B6A5D8DE1BE7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobOpenings).HasConstraintName("fk_job_created_by");
        });

        modelBuilder.Entity<JobSkillMap>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.SkillId }).HasName("PK__Job_Skil__E1891E923FE9DEC4");

            entity.HasOne(d => d.Job).WithMany(p => p.JobSkillMaps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_skill_job");

            entity.HasOne(d => d.Skill).WithMany(p => p.JobSkillMaps).HasConstraintName("fk_job_skill_skill");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__E5331AFA3794D483");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC640F240D");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermissionMap",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK__RolePermi__permi__656C112C"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__RolePermi__role___6477ECF3"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__RolePerm__C85A54638683F428");
                        j.ToTable("RolePermissionMap");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<int>("PermissionId").HasColumnName("permission_id");
                    });
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skill__FBBA8379BE727607");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F3B35AF8C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoleMap",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__UserRoleM__role___619B8048"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserRoleM__user___60A75C0F"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__6EDEA153DE741675");
                        j.ToTable("UserRoleMap");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
