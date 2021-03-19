using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class CompanyViewModel
    {
        [Required(ErrorMessage = "Company name is required")]
        public string FullName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string WebsiteUrl { get; set; }
    }
}
