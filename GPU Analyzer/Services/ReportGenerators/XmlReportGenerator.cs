using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GPU_Analyzer.Services.ReportGenerators
{
    public class XmlReportGenerator : IReportGenerator
    {
        public async Task<string> GenerateReportAsync(ReportData data, string outputPath)
        {
            var serializer = new XmlSerializer(typeof(ReportData));

            using (var writer = new StreamWriter(outputPath))
            {
                serializer.Serialize(writer, data);
            }

            return await Task.FromResult(outputPath);
        }
    }
}
