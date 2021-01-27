using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class QuestionAnswer
    {
        public QuestionAnswer()
        {
            TestResults = new HashSet<TestResults>();
        }

        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public int QuestionInTestId { get; set; }
        public int ChoiceId { get; set; }
        public double TotalMark { get; set; }

        public virtual Choice Choice { get; set; }
        public virtual QuestionInTest QuestionInTest { get; set; }
        public virtual Registration Registration { get; set; }
        public virtual ICollection<TestResults> TestResults { get; set; }
    }
}
