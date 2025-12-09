using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPU_Analyzer.Models;

namespace GPU_Analyzer.Services
{
    public interface ISystemOverviewService
    {
        SystemInfo GetSystemInfo();
    }
}
