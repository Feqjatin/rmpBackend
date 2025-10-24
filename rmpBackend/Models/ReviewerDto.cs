namespace rmpBackend.Models
{
       
        public class AssignReviewerDto
        {
            public int JobId { get; set; }
            public string UserName { get; set; }
       }
    public class ReviewerApplicationActionDto
    {
        public int ApplicationId { get; set; }
        public int? ReviewerUserId { get; set; }
        public string Status { get; set; }
        public bool IsPublished { get; set; }
        public string PersonalNote { get; set; }
        public DateTime? ActionDate { get; set; }
    }

    public class ReviewerDashboardDto
        {
            public int JobId { get; set; }
            public string JobTitle { get; set; } = string.Empty;
            public int Accepted { get; set; }
            public int Rejected { get; set; }
            public int OnHold { get; set; }
            public int New { get; set; }
        public int Published { get; set; }
        public int Total { get; set; }
    }
    
        public class BulkReviewerActionDto
        {
            
            public List<int> Ids { get; set; }
            public string Status { get; set; }
            public string Username { get; set; }
        }
    public class UpdateNoteDto
    {
        public int Id { get; set; }  
        public string PersonalNote { get; set; }
        public string Username { get; set; }
    }

}
