using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class InterviewRoundTemplate
{
    public int RoundTemplateId { get; set; }

    public int JobId { get; set; }

    public int RoundOrder { get; set; }

    public string RoundType { get; set; } = null!;

    public string RoundName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<InterviewSchedule> InterviewSchedules { get; set; } = new List<InterviewSchedule>();

    public virtual JobOpening Job { get; set; } = null!;
}
