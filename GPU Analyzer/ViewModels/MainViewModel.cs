using GPU_Analyzer.Models;
using GPU_Analyzer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GPU_Analyzer.Services;
using GPU_Analyzer.Commands;
using System.Windows.Input;

namespace GPU_Analyzer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        
        private IGPUInfoService gpuService;
        public ICommand OpenReportWindowCommand { get; }
        
        public ObservableCollection<GPUInfo> GPUs { get; }
        private GPUInfo selectedGPU;
        public GPUInfo SelectedGPU
        {
            get { return selectedGPU; }
            set
            {
                selectedGPU = value;
                OnPropertyChanged("SelectedGPU");
            }
        }
        
        public TabItemModel SelectedTab {  get; set; }
        public List<TabItemModel> Tabs { get; }
        public MainViewModel(
            IGPUInfoService gpuService,
            GPUInfoViewModel gpuInfoVM,
            MonitoringViewModel monitoringVM,
            SystemOverviewViewModel systemOverviewVM,
            StressTestsViewModel stressTestsVM,
            SettingsViewModel settingsVM)
        {
            this.gpuService = gpuService;
            GPUs = new ObservableCollection<GPUInfo>(gpuService.EnumerateAdapters());
            SelectedGPU = GPUs.FirstOrDefault();
            gpuInfoVM.Bind(this);
            monitoringVM.Bind(this);
            
            //systemOverviewVM.BindSelectedGPU(this);
            Tabs = new List<TabItemModel>
            {
                new TabItemModel { Title = systemOverviewVM.Title, View = new SystemOverviewView { DataContext = systemOverviewVM } },
                new TabItemModel { Title = gpuInfoVM.Title, View = new GPUInfoView { DataContext = gpuInfoVM } },
                new TabItemModel { Title = monitoringVM.Title, View = new MonitoringView { DataContext = monitoringVM } },
                new TabItemModel { Title = stressTestsVM.Title, View = new StressTestsView { DataContext = stressTestsVM } },
                /*
               
                
                new TabItemModel { Title = settingsVM.Title, View = new SettingsView { DataContext = settingsVM } },*/

            };
            SelectedTab = Tabs[0];
            OpenReportWindowCommand = new RelayCommand(_ => OpenReportWindow());
        }

        private void OpenReportWindow()
        {
            
            var vm = new ReportViewModel
            {
                gpu = this.SelectedGPU
            };

            var window = new ReportWindow();
            window.DataContext = vm;
            vm.CloseRequested += () => window.Close();

            window.ShowDialog();
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
