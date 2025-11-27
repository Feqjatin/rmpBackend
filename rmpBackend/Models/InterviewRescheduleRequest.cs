using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class InterviewRescheduleRequest
{
    public int RequestId { get; set; }

    public int InterviewId { get; set; }

    public int CandidateId { get; set; }

    public DateTime RequestedNewStartTime { get; set; }

    public DateTime RequestedNewEndTime { get; set; }

    public string Reason { get; set; } = null!;

    public string? Status { get; set; }

    public string? AdminComment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual InterviewSchedule Interview { get; set; } = null!;
}
