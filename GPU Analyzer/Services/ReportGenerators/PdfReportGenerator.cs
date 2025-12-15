using GPU_Analyzer.Models;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GPU_Analyzer.Services.ReportGenerators
{
    public class PdfReportGenerator : IReportGenerator
    {
        public async Task<string> GenerateReportAsync(ReportData data, string outputPath)
        {
            var gpu = data.GpuInfo;
            var sys = data.SystemInfo;

            Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));

            doc.Open();

            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf");
            BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            Font titleFont = new Font(bf, 20, Font.BOLD);
            Font sectionFont = new Font(bf, 14, Font.BOLD);
            Font textFont = new Font(bf, 12);

            doc.Add(new Paragraph("ОТЧЁТ", titleFont));
            doc.Add(new Paragraph("\n"));

            void AddLine(string label, string value)
            {
                doc.Add(new Paragraph($"{label}: {value}", textFont));
                doc.Add(new Paragraph("\n", textFont));
            }

           
            doc.Add(new Paragraph("Информация о системе", sectionFont));
            doc.Add(new Paragraph("----------------------------------------------------------------------------------------------------------------\n", textFont));

            AddLine("Имя ПК", sys.ComputerName);
            AddLine("ОС", sys.OperatingSystem);
            AddLine("CPU", sys.CpuName);
            AddLine("Ядер CPU", sys.CpuCores);
            AddLine("Базовая частота CPU", sys.CpuBaseFr);
            AddLine("ОЗУ", sys.RamTotal);

            doc.Add(new Paragraph("Информация о GPU", sectionFont));
            doc.Add(new Paragraph("----------------------------------------------------------------------------------------------------------------\n", textFont));

            AddLine("Имя", gpu.Name);
            AddLine("Тип GPU", gpu.Vendor);
            AddLine("Выделенная память", $"{gpu.DedicatedMemory} MB");
            AddLine("Версия драйвера", gpu.DriverVersion);
            AddLine("Название графического процессора", gpu.VideoProcessor);
            AddLine("Производитель", gpu.AdapterCompatibility);
            AddLine("Текущий видеорежим", gpu.VideoModeDescription);
            AddLine("Частота обновления монитора", $"{gpu.RefreshRate} Гц");
            AddLine("Тип видео памяти", gpu.VideoMemoryType);

            doc.Close();

            return await Task.FromResult(outputPath);
        }
    }
}
