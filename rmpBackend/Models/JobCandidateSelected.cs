using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobCandidateSelected
{
    public int JobCandidateSelectedId { get; set; }

    public int JobId { get; set; }

    public int CandidateId { get; set; }

    public DateTime? SelectedOn { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual JobOpening Job { get; set; } = null!;
}
