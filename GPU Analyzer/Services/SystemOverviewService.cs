using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Services
{
    public class SystemOverviewService : ISystemOverviewService
    {
        public SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo();
            try
            {
                info.ComputerName = Environment.MachineName;
                var os = Environment.OSVersion;
                info.OperatingSystem = $"{os.VersionString} (Build {os.Version.Build})";
                info.CpuName = GetWmi("Win32_Processor", "Name")?.Trim() ?? "N/A";
                info.CpuCores = GetWmi("Win32_Processor", "NumberOfCores")?.Trim() ?? "N/A";
                var fr = GetWmi("Win32_Processor", "MaxClockSpeed");
                info.CpuBaseFr = fr != null ? $"{fr} МГц" : "N/A";
                var ramBytes = GetWmi("Win32_ComputerSystem", "TotalPhysicalMemory");
                if (long.TryParse(ramBytes, out long bytes))
                {
                    double gb = bytes / (1024.0 * 1024.0 * 1024.0);
                    info.RamTotal = $"{gb:F2} ГБ";
                }
                else
                {
                    info.RamTotal = "N/A";
                }
            }
            catch (Exception ex)
            {
                info.ComputerName = "Ошибка";
                info.OperatingSystem = "N/A";
                info.CpuName = "N/A";
                info.CpuCores = "N/A";
                info.CpuBaseFr = "N/A";
                info.RamTotal = "N/A";
            }
            return info;
        }

        private string GetWmi(string className, string prop)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {prop} FROM {className}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj[prop]?.ToString();
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
