using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class ReviewerAction
{
    public int ApplicationId { get; set; }

    public int ReviewerUserId { get; set; }

    public string Status { get; set; } = null!;

    public bool IsPublished { get; set; }

    public string? PersonalNote { get; set; }

    public DateTime ActionDate { get; set; }

    public virtual JobApplication Application { get; set; } = null!;

    public virtual User ReviewerUser { get; set; } = null!;
}
