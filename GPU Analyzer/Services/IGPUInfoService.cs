using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Services
{
    public interface IGPUInfoService
    {
        List<GPUInfo> EnumerateAdapters();
        float GetMemoryUsed(GPUInfo gpu);
        float GetLoadGPU(GPUInfo gpu);
        float GetTemperatureGPU(GPUInfo gpu);
        float GetCoreClock(GPUInfo gpu);
        float GetMemoryClock(GPUInfo gpu);
        void DebugGPUInfo_Sensors();
        void DebugGPUInfo_WMI();
    }
}
