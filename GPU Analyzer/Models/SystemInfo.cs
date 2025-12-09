using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Models
{
    public class SystemInfo : INotifyPropertyChanged
    {
        private string _computerName;
        private string _operatingSystem;
        private string _cpuName;
        private string _cpuCores;
        private string _cpuBaseFr;
        private string _ramTotal;

        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; OnPropertyChanged("ComputerName"); }
        }
        public string OperatingSystem
        {
            get { return _operatingSystem; }
            set { _operatingSystem = value; OnPropertyChanged("OperatingSystem"); }
        }
        public string CpuName
        {
            get { return _cpuName; }
            set { _cpuName = value; OnPropertyChanged("CpuName"); }
        }
        public string CpuCores
        {
            get { return _cpuCores; }
            set { _cpuCores = value; OnPropertyChanged("CpuCores"); }
        }
        public string CpuBaseFr
        {
            get { return _cpuBaseFr; }
            set { _cpuBaseFr = value; OnPropertyChanged("CpuBaseFr"); }
        }
        public string RamTotal
        {
            get { return _ramTotal; }
            set { _ramTotal = value; OnPropertyChanged("RamTotal"); }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
