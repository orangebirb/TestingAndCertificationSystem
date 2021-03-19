using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public class Company
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string WebsiteUrl { get; set; }
    }
}
