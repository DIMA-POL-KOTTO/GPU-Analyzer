using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GPU_Analyzer.Services.ReportGenerators
{
    public class TxtReportGenerator : IReportGenerator
    {
        public async Task<string> GenerateReportAsync(ReportData data, string outputPath)
        {
            string text =
                $"GPU REPORT\n" +
                $"====================\n" +
                $"Name: {data.GpuInfo.Name}\n" +
                $"Vendor: {data.GpuInfo.Vendor}\n" +
                $"VRAM: {data.GpuInfo.VideoProcessor}\n" +
                $"Driver: {data.GpuInfo.DriverVersion}\n";

            await File.WriteAllTextAsync(outputPath, text);
            return outputPath;
        }
    }
}
