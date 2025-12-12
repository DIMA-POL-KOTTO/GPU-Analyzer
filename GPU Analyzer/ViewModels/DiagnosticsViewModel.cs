using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GPU_Analyzer.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GPU_Analyzer.ViewModels
{
    public class DiagnosticsViewModel : INotifyPropertyChanged
    {
       

        public List<TabItemModel> Tabs { get; }
        private TabItemModel selectedTab;
        public TabItemModel SelectedTab
        {
            get => selectedTab;
            set { selectedTab = value; OnPropertyChanged(); }
        }

        // выбранный GPU (назначается при открытии окна)
        public GPUInfo SelectedGPU { get; set; }
        private MonitoringViewModel monitoringVM;
        public ReferenceViewModel referenceVM { get; }
        public VramCheckViewModel vramVM {  get; }
        public DiagnosticsViewModel(ReferenceViewModel referenceVM, VramCheckViewModel vramVM, MonitoringViewModel monitoringVM)
        {
            // передаём ссылку на SelectedGPU в дочерние VM, если нужно
            this.referenceVM = referenceVM;
            this.vramVM = vramVM;
            this.monitoringVM = monitoringVM;

            referenceVM.ParentDiagnostics = this;
            referenceVM.Monitoring = monitoringVM;
            var mainVM = App.Services.GetRequiredService<MainViewModel>();
            this.monitoringVM.Bind(mainVM);
            vramVM.ParentDiagnostics = this;

            Tabs = new List<TabItemModel>
            {
                new TabItemModel { Title = referenceVM.Title, View = new ReferenceView { DataContext = referenceVM } },
                new TabItemModel { Title = vramVM.Title, View = new VramCheckView { DataContext = vramVM } }
            };

            SelectedTab = Tabs[0];
        }

       
        public GPUInfo GetSelectedGPU() => SelectedGPU;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

}
