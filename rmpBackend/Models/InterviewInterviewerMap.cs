using System;
using System.Collections.Generic;
namespace rmpBackend.Models
{
    public class InterviewInterviewerMap
    {
       
            public int InterviewId { get; set; }

            public int InterviewerUserId { get; set; }

            public virtual InterviewSchedule Interview { get; set; } = null!;

            public virtual User InterviewerUser { get; set; } = null!;
        
    }
}
