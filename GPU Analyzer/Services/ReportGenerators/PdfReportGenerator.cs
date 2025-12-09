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

            Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));

            doc.Open();

            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf");
            BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            Font titleFont = new Font(bf, 20, Font.BOLD);
            Font sectionFont = new Font(bf, 14, Font.BOLD);
            Font textFont = new Font(bf, 12);

            // Заголовок
            doc.Add(new Paragraph("ОТЧЁТ GPU", titleFont));
            doc.Add(new Paragraph("\n"));

            // Функция удобного добавления
            void AddLine(string label, string value)
            {
                doc.Add(new Paragraph($"{label}: {value}", textFont));
                doc.Add(new Paragraph("\n", textFont));
            }

            // Раздел GPU
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
