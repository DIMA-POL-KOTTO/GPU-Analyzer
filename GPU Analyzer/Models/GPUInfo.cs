using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GPU_Analyzer.Models
{
    public class GPUInfo : INotifyPropertyChanged
    {
        private string name = "";
        private string dedicatedMemory = "";
        private string driverVersion = "";
        private float memoryUsed = 0;

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }
        public string DedicatedMemory
        {
            get { return dedicatedMemory; }
            set { dedicatedMemory = value; OnPropertyChanged("DedicatedMemory"); }
        }
        public string DriverVersion
        {
            get { return driverVersion; }
            set { driverVersion = value; OnPropertyChanged("DriverVersion"); }
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
