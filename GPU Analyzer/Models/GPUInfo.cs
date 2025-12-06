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
        private string videoProcessor = "";
        private string adapterCompatibility = "";
        private string pnpDeviceId = "";
        private string deviceId = "";
        private string videoModeDescription = "";
        private string currentResolution = "";
        private string refreshRate = "";
        private string videoMemoryType = "";
        private string vendor = "";

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
        public string VideoProcessor
        {
            get { return videoProcessor; }
            set { videoProcessor = value; OnPropertyChanged("VideoProcessor"); }
        }
        public string AdapterCompatibility
        {
            get { return adapterCompatibility; }
            set { adapterCompatibility = value; OnPropertyChanged("AdapterCompatibility"); }
        }
        public string PNPDeviceID
        {
            get { return pnpDeviceId; }
            set { pnpDeviceId = value; OnPropertyChanged("PNPDeviceId"); }
        }
        public string DeviceID
        {
            get { return deviceId; }
            set { deviceId = value; OnPropertyChanged("DeviceId"); }
        }
        public string VideoModeDescription
        {
            get { return videoModeDescription; }
            set { videoModeDescription = value; OnPropertyChanged("VideoModeDescription"); }
        }
        public string CurrentResolution
        {
            get { return currentResolution; }
            set { currentResolution = value; OnPropertyChanged("CurrentResolution"); }
        }
        public string RefreshRate
        {
            get { return refreshRate; }
            set { refreshRate = value; OnPropertyChanged("RefreshRate"); }
        }
        public string VideoMemoryType
        {
            get { return videoMemoryType; }
            set { videoMemoryType = value; OnPropertyChanged("VideoMemoryType"); }
        }

        public string Vendor
        {
            get { return vendor; }
            set { vendor = value; OnPropertyChanged("Vendor"); }
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
