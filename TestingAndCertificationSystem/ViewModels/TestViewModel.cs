using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class CreateTestViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Duration in minutes")]
        public string DurationInMinutes { get; set; }

        [Required]
        [Display(Name = "Certificate")]
        public bool Certificate { get; set; }

        [Required]
        [Display(Name = "Instruction")]
        public string Instruction { get; set; }

        [Display(Name = "Additional task")]
        public int AdditionalTaskId { get; set; }

        [Required]
        [Display(Name = "Access only for selected users")]
        public bool IsPrivate { get; set; }

        [Required]
        [Display(Name = "Required mark to pass")]
        [Range(0, 100, ErrorMessage = "Incorrect mark value")]
        public float PassingMarkInPercents { get; set; }
    }
}
