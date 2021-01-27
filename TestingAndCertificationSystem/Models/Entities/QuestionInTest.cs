using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class QuestionInTest
    {
        public QuestionInTest()
        {
            QuestionAnswer = new HashSet<QuestionAnswer>();
        }

        public int Id { get; set; }
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public int QuestionNumber { get; set; }

        public virtual Question Question { get; set; }
        public virtual Test Test { get; set; }
        public virtual ICollection<QuestionAnswer> QuestionAnswer { get; set; }
    }
}
