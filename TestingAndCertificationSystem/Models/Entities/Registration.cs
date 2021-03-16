using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public class Registration
    {
        public Registration()
        {
            QuestionAnswer = new HashSet<QuestionAnswer>();
            TestResults = new HashSet<TestResults>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public int? TestId { get; set; }
        public Guid Token { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime EndingTime { get; set; }

        public virtual Test Test { get; set; }
        public virtual ICollection<QuestionAnswer> QuestionAnswer { get; set; }
        public virtual ICollection<TestResults> TestResults { get; set; }
    }
}
