using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingAndCertificationSystem.Models;

namespace TestingAndCertificationSystem
{
    public class GroupUser
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserEmail { get; set; }

        public virtual Group Group { get; set; }
    }
}
