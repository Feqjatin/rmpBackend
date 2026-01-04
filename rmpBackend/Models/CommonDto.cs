namespace rmpBackend.Models
{
    public class CreateFeedbackDto
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public int UserRoleId { get; set; }
        public string FeedbackStage { get; set; }
        public string CommentText { get; set; }
    }

    
    public class UpdateFeedbackDto
    {
        public string CommentText { get; set; }
    }
 
    public class FeedbackViewDto
    {
        public int FeedbackId { get; set; }
        public int ApplicationId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string FeedbackStage { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class JobMatchingRequestDto
    {
        public int Id { get; set; }
    }
    public class ApplicationCommentDto
    {
        public int ApplicationId { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public string Role { get; set; }
    }
    public class SaveSkillAssessmentsWithRoleStageDto
    {
        public int ApplicationId { get; set; }
        public string Username { get; set; }
        public List<SkillAssessmentItemDto> Assessments { get; set; }
        public string Role { get; set; }  
        public string Stage { get; set; }  
    }
    public class SkillAssessmentItemDto
    {
        public int SkillId { get; set; }
        public decimal? Years { get; set; }
        public string Comment { get; set; }
    }
    public class SkillAssessmentViewDto
    {
        public int AssessmentId { get; set; }
        public int? ApplicationId { get; set; }
        public string SkillName { get; set; }
        public decimal? YearsOfExperience { get; set; }
        public string Comment { get; set; }
        public string AssessedByUserName { get; set; }
        public string AssessedInRoleName { get; set; }
        public string Stage { get; set; }
        public DateTime AssessmentDate { get; set; }
    }
    public enum EmailEventType
    {
        CandidateMovedToNextRound,
        InterviewScheduled,
        InterviewReminder,
        FeedbackPending,
        CustomRoundEnabled,
        OnBoarding
    }
    public class EmailRequest
    {
        public EmailEventType EventType { get; set; }
        public List<string> ToEmails { get; set; }

        public Dictionary<string, string> Data { get; set; } = new();
    }


}
