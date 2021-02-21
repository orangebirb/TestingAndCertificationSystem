using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.ViewModels
{
    public class QuestionDataModel
    {
        public Question Question { get; set; }
        public List<ChoiceModel> Choices { get; set; }
    }

    public class ChoiceModel
    {
        public Choice Choice { get; set; }
        public bool IsChecked { get; set; }
    }
}
