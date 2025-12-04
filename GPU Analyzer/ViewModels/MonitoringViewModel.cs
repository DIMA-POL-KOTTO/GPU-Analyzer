using GPU_Analyzer.Models;
using GPU_Analyzer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GPU_Analyzer.ViewModels
{
    public class MonitoringViewModel : INotifyPropertyChanged
    {

        private MainViewModel mainVM;
        private readonly IGPUInfoService gpuService;
        private GPUInfo lastGPU = null;

        public string Title => "Мониторинг";
        
        private System.Timers.Timer timer;
        
        // MEMORY_USED
        private float memoryUsed;
        public float MemoryUsed
        {
            get => memoryUsed;
            set
            {
                memoryUsed = value;
                OnPropertyChanged(nameof(MemoryUsedText));

            } 
        }
        public string MemoryUsedText => $"{MemoryUsed:F0} МБ";
        public ObservableCollection<float> MemoryHistory { get; set; }

        // GPU_LOAD
        private float gpuLoad;
        public float GpuLoad
        {
            get => gpuLoad;
            set
            {
                gpuLoad = value;
                OnPropertyChanged(nameof(GpuLoadText));
            }
        }

        public string GpuLoadText => $"{GpuLoad:F0}%";
        public ObservableCollection<float> GpuLoadHistory { get; set; }

        // TEMPERATURE
        private float gpuTemp;
        public float GpuTemp
        {
            get => gpuTemp;
            set
            {
                gpuTemp = value;
                OnPropertyChanged(nameof(GpuTempText));
            }
        }
        public string GpuTempText => $"{GpuTemp:F0} °C";
        public ObservableCollection<float> GpuTempHistory { get; set; }

        // other
        public GPUInfo SelectedGPU => mainVM.SelectedGPU;
        private float memoryMaxValue;
        public float MemoryMaxValue
        {
            get => memoryMaxValue;
            set
            {
                memoryMaxValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryMaxValueText));
            }
        }
        public string MemoryMaxValueText => $"{MemoryMaxValue:F0}";

        private float memoryMidValue;
        public float MemoryMidValue
        {
            get => memoryMidValue;
            set
            {
                memoryMidValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryMidValueText));
            }
        }
        public string MemoryMidValueText => $"{MemoryMidValue:F0}";

        private float memoryMinValue;
        public float MemoryMinValue
        {
            get => memoryMinValue;
            set
            {
                memoryMinValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryMinValueText));
            }
        }
        public string MemoryMinValueText => $"{MemoryMinValue:F0}";

        public MonitoringViewModel(IGPUInfoService gpuService)
        {
            
            this.gpuService = gpuService;
            MemoryHistory = new ObservableCollection<float>();
            MemoryMaxValue = 0f;
            MemoryMidValue = 0f;
            MemoryMinValue = 0f;

            GpuLoadHistory = new ObservableCollection<float>();

            GpuTempHistory = new ObservableCollection<float>();

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, e) => UpdateMonitoring();
            
        }

        public void Bind(MainViewModel mainVM)
        {
            this.mainVM = mainVM;
            mainVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(mainVM.SelectedGPU))
                    OnPropertyChanged(nameof(SelectedGPU));
            };
            timer.Start();
        }
        private void UpdateMonitoring()
        {
            if (SelectedGPU == null)
                return;

            // GPU сменился → очистить данные
            if (lastGPU != SelectedGPU)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    MemoryHistory.Clear();
                    GpuLoadHistory.Clear();
                    GpuTempHistory.Clear();

                    MemoryMaxValue = 0;
                    MemoryMidValue = 0;
                    MemoryMinValue = 0;

                    OnPropertyChanged(nameof(MemoryHistory));
                    OnPropertyChanged(nameof(GpuLoadHistory));
                    OnPropertyChanged(nameof(GpuTempHistory));
                });

                lastGPU = SelectedGPU;
            }

            float used = gpuService.GetMemoryUsed(SelectedGPU);
            float load = gpuService.GetLoadGPU(SelectedGPU);
            float temp = gpuService.GetTemperatureGPU(SelectedGPU);
            //gpuService.DebugGPUInfo_Sensors(); //потом убрать
            //gpuService.DebugGPUInfo_WMI();
            // Обновляем UI через Dispatcher
            try
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    
                    MemoryUsed = used;
                    MemoryHistory.Add(used);
                    UpdateMemoryGraphValues();
                    OnPropertyChanged(nameof(MemoryHistory));
                    if (MemoryHistory.Count > 100)
                    {
                        MemoryHistory.RemoveAt(0);
                    }

                    GpuLoad = load;
                    GpuLoadHistory.Add(load);
                    OnPropertyChanged(nameof(GpuLoadHistory));
                    if (GpuLoadHistory.Count > 100)
                    {
                        GpuLoadHistory.RemoveAt(0);
                    }

                    GpuTemp = temp;
                    GpuTempHistory.Add(temp);
                    OnPropertyChanged(nameof(GpuTempHistory));
                    if (GpuTempHistory.Count > 100)
                    {
                        GpuTempHistory.RemoveAt(0);
                    }
                });
            }
            catch (System.NullReferenceException)
            {
                System.Diagnostics.Debug.WriteLine("Всё ок");
            }
        }

        private void UpdateMemoryGraphValues()
        {
            if (MemoryHistory.Count > 0)
            {
                MemoryMaxValue = MemoryHistory.Max();
                MemoryMidValue = MemoryHistory.Average();
                MemoryMinValue = MemoryHistory.Min();
            }
            else
            {
                MemoryMaxValue = 0f;
                MemoryMidValue = 0f;
                MemoryMinValue = 0f;
            }
        }


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
