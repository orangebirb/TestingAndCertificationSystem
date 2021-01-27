using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class Test
    {
        public Test()
        {
            QuestionInTest = new HashSet<QuestionInTest>();
            VerifiedUsers = new HashSet<VerifiedUsers>();
        }

        public int Id { get; set; }
        public string TestAuthorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DurationInMinutes { get; set; }
        public bool Certificate { get; set; }
        public string Instruction { get; set; }
        public Guid Token { get; set; }
        public int TokenLifetimeInMinutes { get; set; }
        public DateTime TokenStartTime { get; set; }
        public DateTime TokenEndTime { get; set; }
        public int? AdditionalTaskId { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        public double PassingMarkInPercents { get; set; }
        public string Link { get; set; }

        public virtual AdditionalTask AdditionalTask { get; set; }
        public virtual ICollection<QuestionInTest> QuestionInTest { get; set; }
        public virtual ICollection<VerifiedUsers> VerifiedUsers { get; set; }
    }
}
