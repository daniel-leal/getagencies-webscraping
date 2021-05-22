using System;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace GetAgency
{
    public class AgencyDB
    {
        [Key]
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string ServiceStartTime { get; set; }

        public string ServiceEndTime { get; set; }

        public string SelfServiceStartTime { get; set; }

        public string SelfServiceEndTime { get; set; }

        public Point Location { get; set; }

        public string Phone { get; set; }

        public string Phone2 { get; set; }

        public string Phone3 { get; set; }

        public string Address { get; set; }

        public string Cep { get; set; }

        public string District { get; set; }

        public string City { get; set; }

        public bool IsStation { get; set; } // Posto de Atendimento?

        public bool IsCapital { get; set; } // Agencia ou Posto da Capital?
    }
}