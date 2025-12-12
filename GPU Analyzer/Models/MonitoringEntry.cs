using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Models
{
    public class MonitoringEntry
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public float? Used { get; set; }
        public float? Load { get; set; }
        public float? Temp { get; set; }
        public float? CoreClock { get; set; }
        public float? MemoryClock { get; set; }
    }
}
