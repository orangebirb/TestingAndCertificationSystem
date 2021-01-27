using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class CompanyViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Short Name")]
        public string ShortName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Establishment date")]
        public DateTime? EstablishmentDate { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [MaxLength(50)]
        [Display(Name = "Website")]
        public string WebsiteUrl { get; set; }
    }
}
