 
 
namespace rmpBackend.Models
{ 
    public class NewJobDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public int? MinExperience { get; set; }
        public string Username { get; set; } = null!;  
        public List<JobSkillDto> Skills { get; set; } = new();
    }

    
    public class UpdateJobDto
    {
        public int JobId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public int? MinExperience { get; set; }
        public List<JobSkillDto> Skills { get; set; } = new();
    }

    
    public class JobSkillDto
    {
        public int Id { get; set; }  
        public string? Type { get; set; }  
    }
 
    public class DeleteJobDto
    {
        public int JobId { get; set; }
    }
 
    public class NewSkillDto
    {
        public string SkillName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateSkillDto
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class DeleteSkillDto
    {
        public int SkillId { get; set; }
    }

    
}
