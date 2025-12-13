using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobCandidateMatchMap
{
    public int CandidateId { get; set; }

    public int JobId { get; set; }

    public int Rank { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual JobOpening Job { get; set; } = null!;
}
