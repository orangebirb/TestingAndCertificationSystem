using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class AdditionalTaskViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Text")]
        public string Text { get; set; }

        [Required]
        [Display(Name = "Expiration date")]
        public DateTime ExpirationDate { get; set; }
    }
}
