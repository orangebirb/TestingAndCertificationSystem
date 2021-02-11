using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public class Question
    {
        public Question()
        {
            Choice = new HashSet<Choice>();
            QuestionAnswer = new HashSet<QuestionAnswer>();
        }

        public int Id { get; set; }
        public int TestId { get; set; }
        public string Text { get; set; }
        public string QuestionType { get; set; }

        public virtual Test Test { get; set; }
        public virtual ICollection<Choice> Choice { get; set; }
        public virtual ICollection<QuestionAnswer> QuestionAnswer { get; set; }
    }
}
