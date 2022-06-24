using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingAndCertificationSystem.Resources;

namespace TestingAndCertificationSystem.ViewModels
{
    public class VerifiedGroupsViewModel
    {
        public PaginationGeneric<GroupViewModel> VerifiedGroups { get; set; }
        public List<Group> Groups { get; set; }
    }
}
