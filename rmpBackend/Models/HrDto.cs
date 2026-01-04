namespace rmpBackend.Models
{
    public class JobCandidateSelectedUpdateDto
    {
        public DateOnly? JoiningDate { get; set; }
        public bool? IsMovedToEmpTable { get; set; }
        public bool? IsDocumentVerified { get; set; }
        public string? Comment { get; set; }
    }
    public class CandidateDocumentUpdateDto
    {
        public string? Status { get; set; }
        public string? Comment { get; set; }
    }

}
