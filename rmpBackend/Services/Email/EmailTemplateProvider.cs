using rmpBackend.Models;

namespace rmpBackend.Services.Email
{
    public class EmailTemplateProvider : IEmailTemplateProvider
    {
        public (string Subject, string Body) GetTemplate(
            EmailEventType eventType,
            Dictionary<string, string> data)
        {
            return eventType switch
            {
                EmailEventType.CandidateMovedToNextRound => (
                    "Interview information ",
                    $" Candidate : {data["CandidateName"]} with mail : {data["CandidateMail"]} is finished {data["ScheduleSeq"]} Round." +
                    $" Their Application ID is {data["ApplicationId"]} " 
                    
                ),
                EmailEventType.InterviewReminder => (
                   "Interview Reminder ",
                   $" Round :{ data["InterviewRound"]} of interview Id :{data["InterviewId"]}  at start {data["StartTime"]} to  {data["EndTime"]}." +
                   $"interview Link = {data["MeetingLink"]}"
               ),
                EmailEventType.OnBoarding => (
                  "On Boarding " ,
                  $"Congratulations { data["CandidateName"] }, welcome to roima intelligence pvt ltd please complete your employee signup and sign-in. Use PUWNAUE*( as security password"
              ),

                _ => throw new Exception("Template not found")
            };
        }
    }

}
