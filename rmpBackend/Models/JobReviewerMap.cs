using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobReviewerMap
{
    public int JobId { get; set; }

    public int ReviewerUserId { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual JobOpening Job { get; set; } = null!;

    public virtual User ReviewerUser { get; set; } = null!;
}
