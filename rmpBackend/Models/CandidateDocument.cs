using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class CandidateDocument
{
    public int DocumentId { get; set; }

    public int CandidateId { get; set; }

    public int? ApplicationId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? Comment { get; set; }

    public int? VerifiedBy { get; set; }

    public virtual JobApplication? Application { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;
}
