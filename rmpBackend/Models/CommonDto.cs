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
}
