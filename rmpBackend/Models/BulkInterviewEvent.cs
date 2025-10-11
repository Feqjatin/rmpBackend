using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class BulkInterviewEvent
{
    public int BulkEventId { get; set; }

    public string EventName { get; set; } = null!;

    public DateOnly EventDate { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}
