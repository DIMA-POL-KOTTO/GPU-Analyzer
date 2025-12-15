using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GPU_Analyzer.StressTests
{
    /// <summary>
    /// Логика взаимодействия для StressTestWindow.xaml
    /// </summary>
    public partial class StressTestWindow : Window
    {
        public DxSimpleRenderer _renderer;

        public StressTestWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // получение HWND окна WPF
            var hwnd = new WindowInteropHelper(this).Handle;

            uint w = Math.Max(1, (uint)ActualWidth);
            uint h = Math.Max(1, (uint)ActualHeight);

            _renderer = new DxSimpleRenderer(hwnd, w, h);
            _renderer.Start();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _renderer?.Stop();
            _renderer = null;
        }
    }
}
