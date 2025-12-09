using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GPU_Analyzer.StressTests;
using GPU_Analyzer.ViewModels.Commands;
using System.Windows.Threading;


namespace GPU_Analyzer.ViewModels
{
    public class StressTestsViewModel
    {
        
        public string Title => "Стресс-тесты";
        public ICommand StartStressTestCommand { get; }

        public StressTestsViewModel()
        {
            StartStressTestCommand = new RelayCommand(_ => StartStressTest());
        }

        private void StartStressTest()
        {
            var window = new StressTestWindow();
            window.Show(); // ← Открываем окно WPF
        }
    }
}
