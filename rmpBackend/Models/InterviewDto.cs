using System.ComponentModel.DataAnnotations;

namespace rmpBackend.Models
{
     
    public class InterviewRoundTemplateDto
    {
        [Required]
        public int JobId { get; set; }
        [Required]
        public int RoundOrder { get; set; }
        [Required]
        public string RoundType { get; set; }
        [Required]
        public string RoundName { get; set; }
        public string Description { get; set; }
    }

    
    public class InterviewScheduleDto
    {
        [Required]
        public int ApplicationId { get; set; }
        [Required]
        public int RoundTemplateId { get; set; }
        public string? Status { get; set; }  
       
        public DateTime? ScheduledStartTime { get; set; }
         
        public DateTime? ScheduledEndTime { get; set; }
        public string? MeetingLink { get; set; }
        public string? Location { get; set; }

        public int? TestId { get; set; }

        public decimal? TestScore { get; set; }

        public decimal? RoundScore { get; set; }
        public int ? RoundSequence {  get; set; } 

    }
    public class InterviewScheduleUpdateDto
    {
        public int? ApplicationId { get; set; }
        public int? RoundTemplateId { get; set; }
        public string? Status { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public string? MeetingLink { get; set; }
        public string? Location { get; set; }
        public decimal? TestScore { get; set; }
        public int? TestId { get; set; }
        public decimal? RoundScore { get; set; }
        public int? RoundSequence { get; set; }
    }


    public class InterviewInterviewerMapDto
    {
        [Required]
        public int InterviewId { get; set; }
        [Required]
        public int InterviewerUserId { get; set; }
    }

    
    public class BulkInterviewEventDto
    {
        [Required]
        public string EventName { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int? CreatedByUserId { get; set; }
    }
    public class UpdateRescheduleRequestDto
    {
        public int RequestId { get; set; }
        public string Status { get; set; } = null!;
        public string? AdminComment { get; set; }
    }

    public class BulkInterviewerRequest
    {
        public List<int> InterviewIds { get; set; } = new();
        public int UserId { get; set; }
    }

    public class BulkInterviewScheduleUpdateDto
    {
        public List<int> ScheduleIds { get; set; } = new();
        public InterviewScheduleUpdateDto Update { get; set; } = null!;
    }

}
