using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GPU_Analyzer.Services.ReportGenerators
{
    public class PdfReportGenerator : IReportGenerator
    {

        public async Task<string> GenerateReportAsync(ReportData data, string outputPath)
        {
            Document doc = new Document();
            PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));

            doc.Open();
            doc.Add(new Paragraph("GPU REPORT"));
            doc.Add(new Paragraph("===================="));
            doc.Add(new Paragraph($"Name: {data.GpuInfo.Name}"));
            doc.Add(new Paragraph($"Vendor: {data.GpuInfo.Vendor}"));
            doc.Add(new Paragraph($"VRAM: {data.GpuInfo.DedicatedMemory} MB"));
            doc.Add(new Paragraph($"Driver: {data.GpuInfo.DriverVersion}"));
            doc.Close();

            return await Task.FromResult(outputPath);
        }
    }
}
