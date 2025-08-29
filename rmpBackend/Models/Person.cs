using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rmpBackend.Models;

[Keyless]
public partial class Person
{
    public int? Id { get; set; }

    [StringLength(10)]
    public string? Name { get; set; }

    public int? Age { get; set; }
}
