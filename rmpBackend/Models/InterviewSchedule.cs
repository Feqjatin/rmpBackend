using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class InterviewSchedule
{
    public int InterviewId { get; set; }

    public int ApplicationId { get; set; }

    public int RoundTemplateId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime ScheduledStartTime { get; set; }

    public DateTime ScheduledEndTime { get; set; }

    public string? MeetingLink { get; set; }

    public string? Location { get; set; }

    public virtual JobApplication Application { get; set; } = null!;

    public virtual InterviewRoundTemplate RoundTemplate { get; set; } = null!;

    public virtual ICollection<InterviewInterviewerMap> InterviewInterviewerMaps { get; set; } = new List<InterviewInterviewerMap>();
}
