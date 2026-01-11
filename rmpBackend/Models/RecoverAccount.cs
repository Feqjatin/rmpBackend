using System;
using System.Collections.Generic;

namespace rmpBackend.Models;

public partial class RecoverAccount
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string OtpHash { get; set; } = null!;

    public bool IsCandidate { get; set; }

    public DateTime Expiry { get; set; }

    public DateTime CreatedAt { get; set; }
}
