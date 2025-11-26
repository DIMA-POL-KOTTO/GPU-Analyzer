using GPU_Analyzer.Models;
using GPU_Analyzer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.ViewModels
{
    public class GPUInfoViewModel : INotifyPropertyChanged
    {
        private MainViewModel mainVM;
       
        public GPUInfo SelectedGPU => mainVM.SelectedGPU;
        
        public string Title => "Детальная информация о GPU";
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void Bind(MainViewModel mainVM)
        {
            this.mainVM = mainVM;
            mainVM.PropertyChanged += (s,e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedGPU))
                {
                    OnPropertyChanged(nameof(SelectedGPU));
                }
            };
        }
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        
    }
}
