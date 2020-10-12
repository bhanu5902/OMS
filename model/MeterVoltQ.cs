using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterSamples.model
{
    public class MeterVoltQ
    {
        public string MeterID { get; set; }
        public string QueryTimeStamp { get; set; }
        public string TimeReported { get; set; }
        public double VoltA { get; set; }
        public double VoltB { get; set; }
        public double VoltC { get; set; }
    }
}
