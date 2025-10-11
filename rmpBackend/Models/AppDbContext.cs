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

    public virtual DbSet<ApplicationFeedback> ApplicationFeedbacks { get; set; }

    public virtual DbSet<BulkInterviewEvent> BulkInterviewEvents { get; set; }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<InterviewRoundTemplate> InterviewRoundTemplates { get; set; }
    public virtual DbSet<InterviewInterviewerMap> InterviewInterviewerMaps { get; set; }

    public virtual DbSet<InterviewSchedule> InterviewSchedules { get; set; }

    public virtual DbSet<JobApplication> JobApplications { get; set; }

    public virtual DbSet<JobOpening> JobOpenings { get; set; }

    public virtual DbSet<JobReviewerMap> JobReviewerMaps { get; set; }

    public virtual DbSet<JobSkillMap> JobSkillMaps { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<SkillAssessment> SkillAssessments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=jatinYY;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Applicat__7A6B2B8CA255DA0F");

            entity.ToTable("ApplicationFeedback");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.CommentText).HasColumnName("comment_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.FeedbackStage)
                .HasMaxLength(100)
                .HasColumnName("feedback_stage");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationFeedbacks)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("fk_feedback_application");

            entity.HasOne(d => d.User).WithMany(p => p.ApplicationFeedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_feedback_user");

            entity.HasOne(d => d.UserRole).WithMany(p => p.ApplicationFeedbacks)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_feedback_role");
        });

        modelBuilder.Entity<BulkInterviewEvent>(entity =>
        {
            entity.HasKey(e => e.BulkEventId).HasName("PK__BulkInte__737C577DE1900B68");

            entity.ToTable("BulkInterviewEvent");

            entity.Property(e => e.BulkEventId).HasColumnName("bulk_event_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EventDate).HasColumnName("event_date");
            entity.Property(e => e.EventName)
                .HasMaxLength(255)
                .HasColumnName("event_name");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.BulkInterviewEvents)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bulkevent_user");
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.CandidateId).HasName("PK__Candidat__39BD4C18A7C26F08");

            entity.ToTable("Candidate");

            entity.HasIndex(e => e.Email, "UQ__Candidat__AB6E6164A031D789").IsUnique();

            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.ResumePath).HasColumnName("resume_path");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<InterviewRoundTemplate>(entity =>
        {
            entity.HasKey(e => e.RoundTemplateId).HasName("PK__Intervie__3902DA969A1BDA1A");

            entity.ToTable("InterviewRoundTemplate");

            entity.Property(e => e.RoundTemplateId).HasColumnName("round_template_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.RoundName)
                .HasMaxLength(255)
                .HasColumnName("round_name");
            entity.Property(e => e.RoundOrder).HasColumnName("round_order");
            entity.Property(e => e.RoundType)
                .HasMaxLength(100)
                .HasColumnName("round_type");

            entity.HasOne(d => d.Job).WithMany(p => p.InterviewRoundTemplates)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_roundtemplate_job");


        });

        modelBuilder.Entity<InterviewSchedule>(entity =>
        {
            entity.HasKey(e => e.InterviewId).HasName("PK__Intervie__141E55522EBD0542");

            entity.ToTable("InterviewSchedule");

            entity.Property(e => e.InterviewId).HasColumnName("interview_id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.MeetingLink).HasColumnName("meeting_link");
            entity.Property(e => e.RoundTemplateId).HasColumnName("round_template_id");
            entity.Property(e => e.ScheduledEndTime).HasColumnName("scheduled_end_time");
            entity.Property(e => e.ScheduledStartTime).HasColumnName("scheduled_start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Scheduled")
                .HasColumnName("status");

            entity.HasOne(d => d.Application).WithMany(p => p.InterviewSchedules)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("fk_interviewschedule_application");

            entity.HasOne(d => d.RoundTemplate).WithMany(p => p.InterviewSchedules)
                .HasForeignKey(d => d.RoundTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_interviewschedule_roundtemplate");
        });

        modelBuilder.Entity<InterviewInterviewerMap>(entity =>
        {
            entity.HasKey(e => new { e.InterviewId, e.InterviewerUserId });

            entity.ToTable("InterviewInterviewerMap");

            entity.HasOne(d => d.Interview)
                .WithMany(p => p.InterviewInterviewerMaps)
                .HasForeignKey(d => d.InterviewId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_interviewmap_schedule");

            entity.HasOne(d => d.InterviewerUser)
                .WithMany(p => p.InterviewInterviewerMaps)
                .HasForeignKey(d => d.InterviewerUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_interviewmap_user");
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__JobAppli__3BCBDCF214B8A4F0");

            entity.ToTable("JobApplication");

            entity.HasIndex(e => new { e.JobId, e.CandidateId }, "uq_job_candidate").IsUnique();

            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.ApplicationStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Applied")
                .HasColumnName("application_status");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("applied_at");
            entity.Property(e => e.BulkEventId).HasColumnName("bulk_event_id");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Rank).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StatusReason).HasColumnName("status_reason");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.BulkEvent).WithMany(p => p.JobApplications)
                .HasForeignKey(d => d.BulkEventId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_jobapplication_bulkevent");

            entity.HasOne(d => d.Candidate).WithMany(p => p.JobApplications)
                .HasForeignKey(d => d.CandidateId)
                .HasConstraintName("fk_application_candidate");

            entity.HasOne(d => d.Job).WithMany(p => p.JobApplications)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_application_job");
        });

        modelBuilder.Entity<JobOpening>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Job_Open__6E32B6A5D8DE1BE7");

            entity.ToTable("Job_Opening");

            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ClosedReason).HasColumnName("closed_reason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.MinExperience).HasColumnName("min_experience");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("fk_job_created_by");
        });

        modelBuilder.Entity<JobReviewerMap>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.ReviewerUserId }).HasName("PK__JobRevie__FCE3F171E5038C61");

            entity.ToTable("JobReviewerMap");

            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ReviewerUserId).HasColumnName("reviewer_user_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("assigned_at");

            entity.HasOne(d => d.Job).WithMany(p => p.JobReviewerMaps)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_jobreviewermap_job");

            entity.HasOne(d => d.ReviewerUser).WithMany(p => p.JobReviewerMaps)
                .HasForeignKey(d => d.ReviewerUserId)
                .HasConstraintName("fk_jobreviewermap_user");
        });

        modelBuilder.Entity<JobSkillMap>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.SkillId }).HasName("PK__Job_Skil__E1891E923FE9DEC4");

            entity.ToTable("Job_Skill_Map");

            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SkillType)
                .HasMaxLength(50)
                .HasColumnName("skill_type");

            entity.HasOne(d => d.Job).WithMany(p => p.JobSkillMaps)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_job_skill_job");

            entity.HasOne(d => d.Skill).WithMany(p => p.JobSkillMaps)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("fk_job_skill_skill");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__E5331AFA3794D483");

            entity.ToTable("Permission");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__81C0F5A2E7C94E42").IsUnique();

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(100)
                .HasColumnName("permission_name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC640F240D");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__783254B105250013").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");

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

            entity.ToTable("Skill");

            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SkillName)
                .HasMaxLength(100)
                .HasColumnName("skill_name");
        });

        modelBuilder.Entity<SkillAssessment>(entity =>
        {
            entity.HasKey(e => e.AssessmentId).HasName("PK__SkillAss__00B98C26FD9479F2");

            entity.ToTable("SkillAssessment");

            entity.Property(e => e.AssessmentId).HasColumnName("assessment_id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.AssessedByUserId).HasColumnName("assessed_by_user_id");
            entity.Property(e => e.AssessedInRoleId).HasColumnName("assessed_in_role_id");
            entity.Property(e => e.AssessmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("assessment_date");
            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Stage).HasColumnName("stage");
            entity.Property(e => e.YearsOfExperience)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("years_of_experience");

            entity.HasOne(d => d.Application).WithMany(p => p.SkillAssessments)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("fk_skillassessment_application");

            entity.HasOne(d => d.AssessedByUser).WithMany(p => p.SkillAssessments)
                .HasForeignKey(d => d.AssessedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_skillassessment_user");

            entity.HasOne(d => d.AssessedInRole).WithMany(p => p.SkillAssessments)
                .HasForeignKey(d => d.AssessedInRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_skillassessment_role");

            entity.HasOne(d => d.Candidate).WithMany(p => p.SkillAssessments)
                .HasForeignKey(d => d.CandidateId)
                .HasConstraintName("fk_skillassessment_candidate");

            entity.HasOne(d => d.Skill).WithMany(p => p.SkillAssessments)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("fk_skillassessment_skill");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F3B35AF8C");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E61642A211C2A").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__User__F3DBC572529596A6").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

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

