using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace rmpBackend.Models
{
    public class InterviewInterviewerMap
    {
       
            public int InterviewId { get; set; }
            [Column("Interviewer_User_Id")]
            public int InterviewerUserId { get; set; }

            public virtual InterviewSchedule Interview { get; set; } = null!;

            public virtual User InterviewerUser { get; set; } = null!;
        
    }
}
