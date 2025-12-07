using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GPU_Analyzer.Models;
using GPU_Analyzer.Commands;
using GPU_Analyzer.Services;
using SharpDX.Direct3D11;
using GPU_Analyzer.Services.ReportGenerators;
using System.Windows.Input;

namespace GPU_Analyzer.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private string savePath;
        public string SavePath 
        { 
            get { return savePath; } 
            set { savePath = value; OnPropertyChanged(nameof(SavePath)); } 
        }

        public GPUInfo gpu { get; set; }
        public ICommand BrowseCommand { get; }
        public ICommand GenerateJsonCommand { get; }
        public ICommand GenerateXmlCommand { get; }
        public ICommand GenerateTxtCommand { get; }
        public ICommand GeneratePdfCommand { get; }

        public ReportViewModel()
        {
            
            BrowseCommand = new RelayCommand(_ => Browse());
            GenerateJsonCommand = new RelayCommand(async _ => await GenerateReportAsync(new JsonReportGenerator(), ".json"));
            GenerateXmlCommand = new RelayCommand(async _ => await GenerateReportAsync(new XmlReportGenerator(), ".xml"));
            GenerateTxtCommand = new RelayCommand(async _ => await GenerateReportAsync(new TxtReportGenerator(), ".txt"));
            GeneratePdfCommand = new RelayCommand(async _ => await GenerateReportAsync(new PdfReportGenerator(), ".pdf"));
        }

        private void Browse()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "JSON file (*.json)|*.json" + "XML (*.xml)|*.xml|" + "Text (*.txt)|*.txt|" + "PDF (*.pdf)|*.pdf";
            dialog.FileName = "gpu_report";
             
            if (dialog.ShowDialog() == true)
            {
                SavePath = dialog.FileName;
            }
        }

        private async Task GenerateReportAsync(IReportGenerator generator, string extension)
        {
            if (gpu == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SavePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = $"{extension.ToUpper()} file (*{extension})|*{extension}",
                    FileName = $"gpu_report{extension}",
                    AddExtension = true
                };

                if (dialog.ShowDialog() != true)
                    return;

                SavePath = dialog.FileName;
            }

            // гарантируем правильное расширение
            if (!SavePath.EndsWith(extension))
                SavePath += extension;

            var data = new ReportData { GpuInfo = gpu };

            await generator.GenerateReportAsync(data, SavePath);

            // закрываем окно
            CloseRequested?.Invoke();

        }

        public Action CloseRequested { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
