using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace GPU_Analyzer.Services.ReportGenerators
{
    public class JsonReportGenerator : IReportGenerator
    {
        public async Task<string> GenerateReportAsync(ReportData data, string outputPath)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true});
            await File.WriteAllTextAsync(outputPath, json);
            return outputPath;
        }
    }
}
