using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.Resources
{
    public enum SortingOrders
    {
        NameAsc,
        NameDesc,
        MarkAsc,
        MarkDesc,
        DateAsc,
        DateDesc,
        PassedOnly,
        FailedOnly,
        DurationAsc,
        DurationDesc,
        CityAsc,
        CityDesc,
        IsActiveOnly,
        IsNotActiveOnly,
        EmailAsc,
        EmailDesc
    }
}
