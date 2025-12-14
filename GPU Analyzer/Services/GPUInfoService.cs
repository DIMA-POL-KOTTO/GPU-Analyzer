using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using LibreHardwareMonitor;
using LibreHardwareMonitor.Hardware;


namespace GPU_Analyzer.Services
{
    public class GPUInfoService : IGPUInfoService
    {
        private Computer computer;
        public GPUInfoService()
        {
            computer = new Computer()
            {
                IsGpuEnabled = true
            };
            computer.Open();
        }

        public List<GPUInfo> EnumerateAdapters()
        {
           
            var adapters = new List<GPUInfo>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                var results = searcher.Get();
                foreach (ManagementObject obj in results)
                {
                    var adapter = new GPUInfo
                    {
                        Name = obj["Name"]?.ToString() ?? "N/A",
                        DriverVersion = obj["DriverVersion"]?.ToString() ?? "N/A",
                        DedicatedMemory = FormatMemory(obj["AdapterRAM"]),
                        VideoProcessor = obj["VideoProcessor"]?.ToString() ?? "N/A",
                        AdapterCompatibility = obj["AdapterCompatibility"]?.ToString() ?? "N/A",
                        PNPDeviceID = obj["PNPDeviceID"]?.ToString() ?? "N/A",
                        DeviceID = obj["DeviceID"]?.ToString() ?? "N/A",
                        VideoModeDescription = obj["VideoModeDescription"]?.ToString() ?? "N/A",
                        CurrentResolution = $"{obj["CurrentHorizontalResolution"]}x{obj["CurrentVerticalResolution"]}",
                        RefreshRate = obj["CurrentRefreshRate"]?.ToString() ?? "N/A",
                        VideoMemoryType = obj["VideoMemoryType"]?.ToString() ?? "N/A",
                        Vendor = GetVendorFromPNP(obj["PNPDeviceID"]?.ToString())
                    };
                    adapters.Add(adapter);
                    adapters.Add(new GPUInfo { Name = "Checking" });//delete
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении GPU: ", ex.Message);
            }
            return adapters;
        }

        private static string GetVendorFromPNP(string pnpId)
        {
            if (string.IsNullOrEmpty(pnpId)) return "Unknown";

            if (pnpId.Contains("VEN_10DE")) return "NVIDIA";
            if (pnpId.Contains("VEN_1002")) return "AMD";
            if (pnpId.Contains("VEN_8086") || pnpId.Contains("VEN_8087")) return "Intel";
            return "Unknown";
        }
        //преобразование в мб
        private static string FormatMemory(object obj)
        {
            if (obj == null) return "N/A";
            long bytes = Convert.ToInt64(obj);
            return $"{bytes / 1024 / 1024} МБ";
        }

        public float GetMemoryUsed(GPUInfo gpu)
        {
            
            float memoryUsed = 0;
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    if (hardware.Name != gpu.Name)
                        continue;

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Name.Contains("Memory Used"))
                        {
                            memoryUsed = sensor.Value ?? 0;
                            
                        }
                    }
                    
                }
            }
            return memoryUsed;
        }

        public float GetLoadGPU(GPUInfo gpu)
        {
            if (gpu.Name.Contains("Intel"))
            {
                return IntelGpuLoadWin10.GetLoad();
            }
            float loadGPU = 0;
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    if (hardware.Name != gpu.Name)
                        continue;

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        {
                            loadGPU = sensor.Value ?? 0;

                        }
                    }

                }
            }
            return loadGPU;
        }

        public float GetTemperatureGPU(GPUInfo gpu)
        {
            float temperatureGPU = 0;
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    if (hardware.Name != gpu.Name)
                        continue;
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            temperatureGPU = sensor.Value ?? 0;
                        }
                    }
                }
            }
            return temperatureGPU;
        }

        public float GetCoreClock(GPUInfo gpu)
        {
            float coreClock = 0;
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    if (hardware.Name != gpu.Name)
                        continue;

                    foreach (var sensor in hardware.Sensors)
                    {
                        
                        if (sensor.SensorType == SensorType.Clock && sensor.Name == "GPU Core") //.Contains("Core")
                        {
                            coreClock = sensor.Value ?? 0;
                        }
                    }
                }
            }
            return coreClock;
        }

        public float GetMemoryClock(GPUInfo gpu)
        {
            float memoryClock = 0;
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();

                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    if (hardware.Name != gpu.Name)
                        continue;

                    foreach (var sensor in hardware.Sensors)
                    {                      
                        if (sensor.SensorType == SensorType.Clock && sensor.Name == "GPU Memory") //.Contains("Memory")
                        {
                            memoryClock = sensor.Value ?? 0;
                        }
                    }
                }
            }
            return memoryClock;
        }

        public (float, float) GetFan(GPUInfo gpu)
        {
            float rpm = 0;
            float percent = 0;
            foreach(var hardware in computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    if (hardware.Name != gpu.Name)
                        continue;

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Fan) //.Contains("Memory")
                        {
                            rpm = sensor.Value ?? 0;
                        }
                        if (sensor.SensorType == SensorType.Control)
                        {
                            percent = sensor.Value ?? 0;
                        }
                    }
                }
            }
            return (rpm, percent);
        }

        //какие вообще сенсоры я могу прочитать с GPU
        public void DebugGPUInfo_Sensors()
        {
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia ||
                    hardware.HardwareType == HardwareType.GpuAmd ||
                    hardware.HardwareType == HardwareType.GpuIntel)
                {
                    System.Diagnostics.Debug.WriteLine($"GPU: {hardware.Name} ({hardware.HardwareType})");

                    foreach (var sensor in hardware.Sensors)
                    {
                        string sensorName = sensor.Name;
                        string sensorType = sensor.SensorType.ToString();
                        string value = sensor.Value.HasValue ? sensor.Value.Value.ToString() : "null";

                        System.Diagnostics.Debug.WriteLine($"  Sensor: {sensorName}, Type: {sensorType}, Value: {value}");
                    }
                    System.Diagnostics.Debug.WriteLine("---------------------------------------------------");
                }
            }
        }

        public void DebugGPUInfo_WMI()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    System.Diagnostics.Debug.WriteLine("=========================================");
                    System.Diagnostics.Debug.WriteLine($"GPU: {obj["Name"]}");

                    foreach (var property in obj.Properties)
                    {
                        string name = property.Name;
                        object value = property.Value ?? "null";

                        System.Diagnostics.Debug.WriteLine($"{name}: {value}");
                    }

                    System.Diagnostics.Debug.WriteLine("=========================================\n");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка WMI: {ex.Message}");
            }
        }
    }
        
}
