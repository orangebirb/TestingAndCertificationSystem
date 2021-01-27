﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class AdditionalTask
    {
        public AdditionalTask()
        {
            Test = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public int RecipientId { get; set; }
        public DateTime ExpirationDate { get; set; }

        public virtual ICollection<Test> Test { get; set; }
    }
}
