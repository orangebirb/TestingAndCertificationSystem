using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class TestViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed for duration")]
        public int DurationInMinutes { get; set; }

        [Required]
        public bool Certificate { get; set; }

        [Required]
        public string Instruction { get; set; }

        [Required]
        public bool IsPrivate { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Required grade should be in range 0 - 100 percents")]
        public float PassingMarkInPercents { get; set; }
    }
}
