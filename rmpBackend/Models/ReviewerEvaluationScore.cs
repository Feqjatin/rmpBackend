using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class ReviewerEvaluationScore
{
    public int ApplicationId { get; set; }

    public int CriteriaId { get; set; }

    public int ReviewerUserId { get; set; }

    public int Score { get; set; }

    public virtual JobApplication Application { get; set; } = null!;

    public virtual ReviewerEvaluationCriterion Criteria { get; set; } = null!;

    public virtual User ReviewerUser { get; set; } = null!;
}