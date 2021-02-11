using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class QuestionDataViewModel
    {
        public Question Question { get; set; }
        public List<ChoiceViewModel> Choices { get; set; }
    }

    public class ChoiceViewModel
    {
        public Choice Choice { get; set; }
        public bool IsChecked { get; set; }
    }
}
