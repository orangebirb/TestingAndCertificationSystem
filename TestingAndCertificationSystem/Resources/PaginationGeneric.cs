using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.Resources
{
    public class PaginationGeneric<T>
    {
        public IEnumerable<T> source { get; set; }
        public Pagination pagination { get; set; }
    }
}
