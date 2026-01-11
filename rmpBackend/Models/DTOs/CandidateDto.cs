using System.ComponentModel.DataAnnotations;

namespace rmpBackend.Models.DTOs
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
    public class SelectedCandidateDto
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
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
 
   
        public class CandidateLoginDto
        {
            [Required]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
        }



    public class CandidateProfileCreateDto
    {

        public string Name { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? ProfileSummary { get; set; }
 
    }

    public class CandidateProfileUpdateDto
        {
            public string? Name { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? ZipCode { get; set; }
            public string? LinkedinUrl { get; set; }
            public string? GithubUrl { get; set; }
            public string? PortfolioUrl { get; set; }
            public string? ProfileSummary { get; set; }

            
            public string? NewPassword { get; set; }
        }
         
        public class CandidateEducationDto
        {
            [Required]
            public string Degree { get; set; }
            [Required]
            public string Institution { get; set; }
            public string? FieldOfStudy { get; set; }
            [Required]
            public DateOnly StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Grade { get; set; }
            public string? Description { get; set; }
        }

        
        public class CandidateExperienceDto
        {
            [Required]
            public string JobTitle { get; set; }
            [Required]
            public string CompanyName { get; set; }
            [Required]
            public DateOnly StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public bool? IsCurrentJob { get; set; }
            public string? Description { get; set; }
            public string? Location { get; set; }
        }

        
        public class CandidateSkillDto
        {
            [Required]
            public int SkillId { get; set; }
            public string? ProficiencyLevel { get; set; }
        }
        public class CandidateResponse
        {
            public int ApplicationId { get; set; }
            public string Response { get; set; }
        }


    public class CandidateDocumentDto
        {
            public int? ApplicationId { get; set; }
            [Required]
            public string DocumentType { get; set; } 
            [Required]
            public IFormFile File { get; set; }
    }

        
        public class RescheduleRequestDto
        {
            [Required]
            public int InterviewId { get; set; }
            [Required]
            public DateTime RequestedNewStartTime { get; set; }
            [Required]
            public DateTime RequestedNewEndTime { get; set; }
            [Required]
            public string Reason { get; set; }
        }
    public class RescheduleRequestResponseDto
    {
        public int RequestId { get; set; }
        public int InterviewId { get; set; }
        public int CandidateId { get; set; }
        public DateTime RequestedNewStartTime { get; set; }
        public DateTime RequestedNewEndTime { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }


}

