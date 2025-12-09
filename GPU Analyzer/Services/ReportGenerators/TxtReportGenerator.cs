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
            var gpu = data.GpuInfo;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ОТЧЁТ GPU");
            sb.AppendLine("==================================");
            sb.AppendLine($"Имя: {gpu.Name}");
            sb.AppendLine($"Тип GPU: {gpu.Vendor}");
            sb.AppendLine($"Выделенная память: {gpu.DedicatedMemory} MB");
            sb.AppendLine($"Версия драйвера: {gpu.DriverVersion}");
            sb.AppendLine($"Название GPU: {gpu.VideoProcessor}");
            sb.AppendLine($"Производитель: {gpu.AdapterCompatibility}");
            sb.AppendLine($"Текущий видеорежим: {gpu.VideoModeDescription}");
            sb.AppendLine($"Частота обновления монитора (Гц): {gpu.RefreshRate}");
            sb.AppendLine($"Тип GPU: {gpu.VideoMemoryType}");
            sb.AppendLine();

            await File.WriteAllTextAsync(outputPath, sb.ToString());
            return outputPath;
        }
    }
}
