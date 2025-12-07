using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Services
{
    public interface IReportGenerator
    {
        Task<string> GenerateReportAsync(ReportData data, string outputPath);
    }
}
