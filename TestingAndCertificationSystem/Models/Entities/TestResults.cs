﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class TestResults
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public int QuestionAnswerId { get; set; }
        public double FinalMarkInPercents { get; set; }
        public bool IsPassed { get; set; }

        public virtual QuestionAnswer QuestionAnswer { get; set; }
        public virtual Registration Registration { get; set; }
    }
}
