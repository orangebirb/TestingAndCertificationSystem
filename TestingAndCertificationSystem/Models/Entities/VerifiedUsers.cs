using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class VerifiedUsers
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string UserEmail { get; set; }

        public virtual Test Test { get; set; }
    }
}
