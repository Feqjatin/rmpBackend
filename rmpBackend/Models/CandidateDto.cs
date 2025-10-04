using System.ComponentModel.DataAnnotations;

namespace rmpBackend.Models
{
    public class CreateCandidateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        public string? ResumePath { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }
    public class UpdateCandidateDto
    {
        
        [Required]
        public int CandidateId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        public string? ResumePath { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }


    public class SkillAssessmentDto
    {
        public int CandidateId { get; set; }
        public int SkillId { get; set; }
        public int? ApplicationId { get; set; }  
        public decimal? YearsOfExperience { get; set; }
        public int AssessedByUserId { get; set; }
        public int AssessedInRoleId { get; set; }
        public string? Comment { get; set; }
        public string? Stage { get; set; }
    }
}
