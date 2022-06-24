using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem
{
    public class VerifiedGroup
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public int GroupId { get; set; }

        public virtual Test Test { get; set; }
        public virtual Group Group { get; set; }
    }
}
