using GPU_Analyzer.Services;
using GPU_Analyzer.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPU_Analyzer.ViewModels
{
    public class ReferenceViewModel : INotifyPropertyChanged
    {
        public string Title => "Референс";
        public DiagnosticsViewModel ParentDiagnostics { get; set; }
        private MonitoringViewModel monitoring;
        public MonitoringViewModel Monitoring 
        { 
            get => monitoring;
            set
            {
                if (monitoring  != null)
                {
                    monitoring.PropertyChanged -= MonitoringChanged;
                }
                monitoring = value;
                if(monitoring != null)
                {
                    monitoring.PropertyChanged += MonitoringChanged;
                }
                OnPropertyChanged();
            } 
        }

        private string compareResult;
        public string CompareResult
        {
            get => compareResult;
            set { compareResult = value; OnPropertyChanged(); }
        }

        public ICommand RunCompareCommand { get; }

        public ReferenceViewModel()
        {
            RunCompareCommand = new RelayCommand(_ => RunCompare());
        }
        private void MonitoringChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Monitoring.GpuTemp) ||
                e.PropertyName == nameof(Monitoring.CoreClock) ||
                e.PropertyName == nameof(Monitoring.MemoryClock))
            {
                // RunCompare();
            }
        }

        private void RunCompare()
        {
            var gpu = ParentDiagnostics?.GetSelectedGPU();
            if (gpu == null)
            {
                CompareResult = "GPU не выбран.";
                return;
            }

            var refs = ReferenceDatabase.Load("reference.json").GetValues(gpu.Name);
            if (refs == null)
            {
                CompareResult = "Для этой модели нет референсных значений, извините:(";
                return;
            }
            float temp = Monitoring.GpuTemp;
            float core = Monitoring.CoreClock;
            float mem = Monitoring.MemoryClock;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Сравнение для {gpu.Name}:\n");

            sb.AppendLine($"Температура: {temp}°C (норма до {refs.MaxTemp}°C)");
            sb.AppendLine(temp <= refs.MaxTemp ? "OK — температура в норме\n" : "!Перегрев!\n");

            sb.AppendLine($"Частота ядра: {core} МГц (ожидаемый {refs.CoreClock} МГц)");
            sb.AppendLine(core >= refs.CoreClock ? "OK — частота нормальная\n" : "!Частота ниже нормы!\n");

            sb.AppendLine($"Частота памяти: {mem} МГц (ожидаемо ~{refs.MemoryClock} МГц)");
            sb.AppendLine(Math.Abs(mem - refs.MemoryClock) < 300 ? "OK — память работает нормально\n" : "!Возможная проблема с памятью!\n");

            CompareResult = sb.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
