using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class ApplicationFeedback
{
    public int FeedbackId { get; set; }

    public int ApplicationId { get; set; }

    public int UserId { get; set; }

    public int UserRoleId { get; set; }

    public string FeedbackStage { get; set; } = null!;

    public string CommentText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual JobApplication Application { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Role UserRole { get; set; } = null!;
}
