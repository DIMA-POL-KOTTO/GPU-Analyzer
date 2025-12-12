using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Models
{
    public class ReportData
    {
        public SystemInfo SystemInfo { get; set; }
        public GPUInfo GpuInfo { get; set; }
        public List<MonitoringEntry> MonitoringEntries { get; set; } = new();

    }
}
