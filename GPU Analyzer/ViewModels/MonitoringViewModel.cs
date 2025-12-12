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
        private IntelGpuZ intelGpuZ;
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

        // MemoryUsed
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

        // CoreClock
        private float coreClockMax;
        public float CoreClockMax
        {
            get => coreClockMax;
            set
            {
                coreClockMax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoreClockMaxText));
            }
        }
        public string CoreClockMaxText => $"{CoreClockMax:F0}";

        private float coreClockMid;
        public float CoreClockMid
        {
            get => coreClockMid;
            set
            {
                coreClockMid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoreClockMidText));
            }
        }
        public string CoreClockMidText => $"{CoreClockMid:F0}";

        private float coreClockMin;
        public float CoreClockMin
        {
            get => coreClockMin;
            set
            {
                coreClockMin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoreClockMinText));
            }
        }
        public string CoreClockMinText => $"{CoreClockMin:F0}";

        //MemoryClock
        private float memoryClockMax;
        public float MemoryClockMax
        {
            get => memoryClockMax;
            set
            {
                memoryClockMax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryClockMaxText));
            }
        }
        public string MemoryClockMaxText => $"{MemoryClockMax:F0}";

        private float memoryClockMid;
        public float MemoryClockMid
        {
            get => memoryClockMid;
            set
            {
                memoryClockMid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryClockMidText));
            }
        }
        public string MemoryClockMidText => $"{MemoryClockMid:F0}";

        private float memoryClockMin;
        public float MemoryClockMin
        {
            get => memoryClockMin;
            set
            {
                memoryClockMin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryClockMinText));
            }
        }
        public string MemoryClockMinText => $"{MemoryClockMin:F0}";


        public MonitoringViewModel(IGPUInfoService gpuService, int a = 1)
        {
            intelGpuZ = new IntelGpuZ();
            
            
            this.gpuService = gpuService;
            MemoryHistory = new ObservableCollection<float>();
            MemoryMaxValue = 0f;
            MemoryMidValue = 0f;
            MemoryMinValue = 0f;

            GpuLoadHistory = new ObservableCollection<float>();

            GpuTempHistory = new ObservableCollection<float>();

            CoreClockHistory = new ObservableCollection<float>();
            CoreClockMax = 0f;
            CoreClockMid = 0f;
            CoreClockMin = 0f;

            MemoryClockHistory = new ObservableCollection<float>();
            MemoryClockMax = 0f;
            MemoryClockMid = 0f;
            MemoryClockMin = 0f;

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
                    CoreClockMax = 0;
                    CoreClockMid = 0;
                    CoreClockMin = 0;
                    MemoryClockMax = 0;
                    MemoryClockMid = 0;
                    MemoryClockMin = 0;

                    OnPropertyChanged(nameof(MemoryHistory));
                    OnPropertyChanged(nameof(GpuLoadHistory));
                    OnPropertyChanged(nameof(GpuTempHistory));
                    OnPropertyChanged(nameof(CoreClockHistory));
                    OnPropertyChanged(nameof(MemoryClockHistory));
                });

                lastGPU = SelectedGPU;
            }

            float used, load, temp, core_clock, mem_clock;
            if (SelectedGPU.Name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
            {
                used = gpuService.GetMemoryUsed(SelectedGPU);
                load = gpuService.GetLoadGPU(SelectedGPU);
                if (intelGpuZ != null && intelGpuZ.IsAvailable && intelGpuZ.TryUpdate())
                {
                    
                    temp = intelGpuZ.Temperature;
                    
                    core_clock = intelGpuZ.CoreClock;
                    mem_clock = intelGpuZ.MemoryClock;
                }
                else
                {
                    temp = core_clock = mem_clock = 0;
                }
            }
            else
            {
                used = gpuService.GetMemoryUsed(SelectedGPU);
                load = gpuService.GetLoadGPU(SelectedGPU);
                temp = gpuService.GetTemperatureGPU(SelectedGPU);
                core_clock = gpuService.GetCoreClock(SelectedGPU);
                mem_clock = gpuService.GetMemoryClock(SelectedGPU);
            }
                
            //gpuService.DebugGPUInfo_Sensors(); //потом убрать
            //gpuService.DebugGPUInfo_WMI();
            // Обновляем UI через Dispatcher
            try
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    
                    MemoryUsed = used;
                    MemoryHistory.Add(used);
                    UpdateValues(MemoryHistory, val => MemoryMaxValue = val, val => MemoryMidValue = val, val => MemoryMinValue = val);
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
                    UpdateValues(CoreClockHistory, val => CoreClockMax = val, val => CoreClockMid = val, val => CoreClockMin = val);
                    OnPropertyChanged(nameof (CoreClockHistory));
                    if (CoreClockHistory.Count > 100)
                    {
                        CoreClockHistory.RemoveAt(0);
                    }

                    MemoryClock = mem_clock;
                    MemoryClockHistory.Add(mem_clock);
                    UpdateValues(MemoryClockHistory, val => MemoryClockMax = val, val => MemoryClockMid = val, val => MemoryClockMin = val);
                    OnPropertyChanged(nameof(MemoryClockHistory));
                    if (MemoryClockHistory.Count > 100)
                    {
                        MemoryClockHistory.RemoveAt(0);
                    }

                    var entry = new MonitoringEntry
                    {
                        Timestamp = DateTime.Now,
                        Name = SelectedGPU.Name,
                        Used = used,
                        Load = load,
                        Temp = temp,
                        CoreClock = core_clock,
                        MemoryClock = mem_clock
                    };
                    File.AppendAllText(monitoringTempFile, JsonSerializer.Serialize(entry) + Environment.NewLine);
                });
            }
            catch (System.NullReferenceException)
            {
                System.Diagnostics.Debug.WriteLine("Всё ок");
            }
        }

        private void UpdateValues(ObservableCollection<float> history, Action<float> setMax, Action<float> setMid, Action<float> setMin)
        {
            if (history.Count > 0)
            {
                float max = history.Max();            
                float min = history.Min();
                float mid = (max + min)/2;

                setMax(max); setMid(mid); setMin(min);
            }
            else
            {
                setMax(0); setMid(0); setMin(0);
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
