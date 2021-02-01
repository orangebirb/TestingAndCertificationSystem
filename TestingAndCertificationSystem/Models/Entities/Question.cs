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
            QuestionInTest = new HashSet<QuestionInTest>();
        }

        public int Id { get; set; }
        public string QuestionType { get; set; }
        public double Points { get; set; }

        public virtual ICollection<Choice> Choice { get; set; }
        public virtual ICollection<QuestionInTest> QuestionInTest { get; set; }
    }
}
