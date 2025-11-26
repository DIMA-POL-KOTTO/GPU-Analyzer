using GPU_Analyzer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer
{
    class ViewModelLocator
    {
        public MainViewModel MainVM => App.Services.GetRequiredService<MainViewModel>();
        public GPUInfoViewModel GPUInfoVM => App.Services.GetRequiredService<GPUInfoViewModel>();
        public MonitoringViewModel MonitoringVM => App.Services.GetRequiredService<MonitoringViewModel>();
        public SystemOverviewViewModel SystemOverviewVM => App.Services.GetRequiredService<SystemOverviewViewModel>();
        public StressTestsViewModel StressVM => App.Services.GetRequiredService<StressTestsViewModel>();
        public SettingsViewModel SettingsVM => App.Services.GetRequiredService<SettingsViewModel>();
    }
}
