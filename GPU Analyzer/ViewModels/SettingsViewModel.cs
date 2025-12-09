using GPU_Analyzer.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPU_Analyzer.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public string Title => "Настройки";
        private MonitoringViewModel _monitoringVM;
        private int _monitoringUpdateInterval = 1000;
        public int MonitoringUpdateInterval
        {
            get => _monitoringUpdateInterval;
            set
            {  
                _monitoringUpdateInterval = value;
                OnPropertyChanged("MonitoringUpdateInterval");
                
            }
        }

        public ICommand ApplyCommand { get; }

        public SettingsViewModel()
        {
            ApplyCommand = new RelayCommand(_ => ApplySettings());
        }
        public void ApplySettings()
        {
            _monitoringVM?.UpdateInterval(MonitoringUpdateInterval);
        }
        public void Bind(MonitoringViewModel monitoringVM)
        {
            _monitoringVM = monitoringVM;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
