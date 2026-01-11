using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class ReviewerEvaluationCriterion
{
    public int CriteriaId { get; set; }

    public int JobId { get; set; }

    public string CriteriaName { get; set; } = null!;

    public int MaxScore { get; set; }

    public virtual JobOpening Job { get; set; } = null!;

    public virtual ICollection<ReviewerEvaluationScore> ReviewerEvaluationScores { get; set; } = new List<ReviewerEvaluationScore>();
}