using rmpBackend.Models.DTOs;

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
                EmailEventType.SendOTP => (
                    " Account Recovery OTP",
                    $"Your OTP is: {data["otp"]} \r\nValid for 10 minutes.\r\nDo not share this OTP."
                ),
                EmailEventType.CandidateCreated => (
                  " Your Candidate Account Has Been Created",
                  $" Hello {data["CandidateName"]},\r\n\r\nYour candidate account has been created by our recruitment team.\r\n\r\nYou can log in using your email address by selecting the “Recover Account / Forgot Password” option on the login page to set your password.\r\n\r\nIf you have any questions, feel free to contact us.\r\n\r\nBest regards,\r roima intelligence pvt ltd"
              ),

                _ => throw new Exception("Template not found")
            };
        }
    }

}
