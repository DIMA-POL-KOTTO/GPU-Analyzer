using GPU_Analyzer.Services;
using GPU_Analyzer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace GPU_Analyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServeces(services);
            Services = services.BuildServiceProvider();
            base.OnStartup(e);
        }
        private void ConfigureServeces(IServiceCollection services)
        {
            services.AddSingleton<IGPUInfoService, GPUInfoService>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<GPUInfoViewModel>();
            services.AddTransient<MonitoringViewModel>();
            services.AddTransient<SystemOverviewViewModel>();
            services.AddTransient<StressTestsViewModel>();
            services.AddTransient<SettingsViewModel>();
        }

    }

}
