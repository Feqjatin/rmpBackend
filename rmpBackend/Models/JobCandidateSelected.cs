using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class JobCandidateSelected
{
    public int JobCandidateSelectedId { get; set; }

    public int JobId { get; set; }

    public int CandidateId { get; set; }

    public DateTime? SelectedOn { get; set; }

    public int ApplicationId { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public bool IsMovedToEmpTable { get; set; }

    public bool IsDocumentVerified { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Comment { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual JobOpening Job { get; set; } = null!;
}
