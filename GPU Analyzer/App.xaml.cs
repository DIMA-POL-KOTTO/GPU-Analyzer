using GPU_Analyzer.Services;
using GPU_Analyzer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

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
            services.AddSingleton<ISystemOverviewService, SystemOverviewService>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<GPUInfoViewModel>();
            services.AddTransient<MonitoringViewModel>();
            services.AddTransient<SystemOverviewViewModel>();
            services.AddTransient<StressTestsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ReportViewModel>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            string file = Path.Combine(Path.GetTempPath(), "temp_monitoring.tmp");

            if (File.Exists(file))
                File.Delete(file);

            base.OnExit(e);
        }

    }

}
