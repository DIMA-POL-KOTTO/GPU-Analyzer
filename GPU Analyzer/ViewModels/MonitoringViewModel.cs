using GPU_Analyzer.Models;
using GPU_Analyzer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace GPU_Analyzer.ViewModels
{
    public class MonitoringViewModel : INotifyPropertyChanged
    {

        private MainViewModel mainVM;
        private readonly IGPUInfoService gpuService;
        private GPUInfo lastGPU = null;
        private readonly string monitoringTempFile; //промежуточный файл
        public string MonitoringTempFile => monitoringTempFile;

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

        //CORE_CLOCK
        private float coreClock;
        public float CoreClock
        {
            get => coreClock;
            set
            {
                coreClock = value;
                OnPropertyChanged(nameof(CoreClockText));
            }
        }
        public string CoreClockText => $"{CoreClock:F0} МГц";
        public ObservableCollection<float> CoreClockHistory {  get; set; }

        //MEMORY_CLOCK
        private float memoryClock;
        public float MemoryClock
        {
            get => memoryClock;
            set
            {
                memoryClock= value;
                OnPropertyChanged(nameof(MemoryClockText)); 
            }
        }
        public string MemoryClockText => $"{MemoryClock:F0} МГц";
        public ObservableCollection<float> MemoryClockHistory { get; set; }
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

        public MonitoringViewModel(IGPUInfoService gpuService, int a = 1)
        {
            
            this.gpuService = gpuService;
            MemoryHistory = new ObservableCollection<float>();
            MemoryMaxValue = 0f;
            MemoryMidValue = 0f;
            MemoryMinValue = 0f;

            GpuLoadHistory = new ObservableCollection<float>();

            GpuTempHistory = new ObservableCollection<float>();

            CoreClockHistory = new ObservableCollection<float>();

            MemoryClockHistory = new ObservableCollection<float>();

            monitoringTempFile = Path.Combine(Path.GetTempPath(), "temp_monitoring.tmp");
            if (!File.Exists(monitoringTempFile))
                File.WriteAllText(monitoringTempFile, "");

            int interval = Math.Max(100, a * 1000);

            timer = new System.Timers.Timer(interval);
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

            
            if (lastGPU != SelectedGPU)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    MemoryHistory.Clear();
                    GpuLoadHistory.Clear();
                    GpuTempHistory.Clear();
                    CoreClockHistory.Clear();
                    MemoryClockHistory.Clear();

                    MemoryMaxValue = 0;
                    MemoryMidValue = 0;
                    MemoryMinValue = 0;

                    OnPropertyChanged(nameof(MemoryHistory));
                    OnPropertyChanged(nameof(GpuLoadHistory));
                    OnPropertyChanged(nameof(GpuTempHistory));
                    OnPropertyChanged(nameof(CoreClockHistory));
                    OnPropertyChanged(nameof(MemoryClockHistory));
                });

                lastGPU = SelectedGPU;
            }

            float used = gpuService.GetMemoryUsed(SelectedGPU);
            float load = gpuService.GetLoadGPU(SelectedGPU);
            float temp = gpuService.GetTemperatureGPU(SelectedGPU);
            float core_clock = gpuService.GetCoreClock(SelectedGPU);
            float mem_clock = gpuService.GetMemoryClock(SelectedGPU);
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

                    CoreClock = core_clock;
                    CoreClockHistory.Add(core_clock);
                    OnPropertyChanged(nameof (CoreClockHistory));
                    if (CoreClockHistory.Count > 100)
                    {
                        CoreClockHistory.RemoveAt(0);
                    }

                    MemoryClock = mem_clock;
                    MemoryClockHistory.Add(mem_clock);
                    OnPropertyChanged(nameof(MemoryClockHistory));
                    if (MemoryClockHistory.Count > 100)
                    {
                        MemoryClockHistory.RemoveAt(0);
                    }

                    var entry = new MonitoringEntry
                    {
                        Timestamp = DateTime.Now,
                        Used = used,
                        Load = load,
                        Temp = temp
                    };
                    File.AppendAllText(monitoringTempFile, JsonSerializer.Serialize(entry) + Environment.NewLine);
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
                MemoryMinValue = MemoryHistory.Min();
                MemoryMidValue = (MemoryMaxValue + MemoryMinValue)/2;
            }
            else
            {
                MemoryMaxValue = 0f;
                MemoryMidValue = 0f;
                MemoryMinValue = 0f;
            }
        }
        public void UpdateInterval(double ms)
        {
            

            timer.Stop();
            timer.Interval = ms;
            timer.Start();
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
