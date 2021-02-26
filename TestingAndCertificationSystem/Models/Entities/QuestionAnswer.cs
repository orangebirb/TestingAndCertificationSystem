using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public class QuestionAnswer
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public int TestResultId { get; set; }
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }
        public double TotalMark { get; set; }

        public virtual Choice Choice { get; set; }
        public virtual Question Question { get; set; }
        public virtual Registration Registration { get; set; }
        public virtual TestResults TestResult { get; set; }
    }
}
