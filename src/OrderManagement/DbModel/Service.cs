using System;
using System.Collections.Generic;

namespace OrderManagement.DbModel
{
    public class Service
    {
        public static readonly Service Validation = new Service
        {
            Id = Guid.Parse("{21C75A7E-2426-4DFD-AE09-69912D96290F}"),
            Name = "Validate"
        };

        public static readonly Service Normalize = new Service
        {
            Id = Guid.Parse("{D21B38A1-2B00-497C-BFB8-D577A05B37E0}"),
            Name = "Normalize"
        };

        public static readonly Service Capitalize = new Service
        {
            Id = Guid.Parse("{5CD76522-E37E-49C2-9D79-20D8C76A4AF8}"),
            Name = "Capitalize"
        };

        public static IList<Service> AllServices = new List<Service> {Validation, Normalize, Capitalize};
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}