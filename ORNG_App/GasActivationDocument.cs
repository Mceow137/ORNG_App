using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORNG_App
{
    public class GasActivationDocument
    {
        public int DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public int SpecialistId { get; set; }
        public int CustomerId { get; set; }
        public string Address { get; set; }
        public string MeterNumber { get; set; }
        public decimal InitialMeterReading { get; set; }
        public DateTime ActivationDate { get; set; }
        public byte[] DocumentScan { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
