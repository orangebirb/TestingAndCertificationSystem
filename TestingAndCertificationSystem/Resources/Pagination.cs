using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.Resources
{
    public class Pagination
    {
        public int PageNum { get; private set; }
        public int TotalPages { get; private set; }

        public Pagination(int count, int pageNum, int pageSize)
        {
            PageNum = pageNum;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageNum > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageNum < TotalPages);
            }
        }
    }
}
