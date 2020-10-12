using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterSamples.model
{
    public class MeterPingQ
    {
        public DateTime LastCommunication { get; set; }
        public string MeterID { get; set; }
        public int Status { get; set; }
    }
}
