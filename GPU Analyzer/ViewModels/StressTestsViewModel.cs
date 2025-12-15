using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GPU_Analyzer.StressTests;

using GPU_Analyzer.ViewModels.Commands;
using System.Windows.Threading;
using GPU_Analyzer.Views;
using Microsoft.Extensions.DependencyInjection;


namespace GPU_Analyzer.ViewModels
{
    public class StressTestsViewModel
    {
        
        public string Title => "Стресс-тесты";
        public ICommand StartStressTestCommand { get; }
        public ICommand OpenDiagnosticsCommand { get; }
        public StressTestsViewModel()
        {
            StartStressTestCommand = new RelayCommand(_ => StartStressTest());
            OpenDiagnosticsCommand = new RelayCommand(_ => OpenDiagnostics());
        }

        private void StartStressTest()
        {
            var window = new StressTests.StressTestWindow();
            window.Show(); // открываем окно WPF
        }
        private void OpenDiagnostics()
        {
            var vm = App.Services.GetRequiredService<DiagnosticsViewModel>();
            vm.SelectedGPU = App.Services.GetRequiredService<MainViewModel>().SelectedGPU;
            var window = new DiagnosticsWindow() { DataContext=vm};
            
            window.ShowDialog();
        }
    }
}
