using GPU_Analyzer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GPU_Analyzer.Models;

namespace GPU_Analyzer.ViewModels
{
    public class SystemOverviewViewModel : INotifyPropertyChanged
    {
        public string Title => "Обзор системы";
        public SystemInfo sysInfo { get; }
        public SystemOverviewViewModel(ISystemOverviewService systemInfoService)
        {
            sysInfo = systemInfoService.GetSystemInfo();
            
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
