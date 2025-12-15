using GPU_Analyzer.Services;
using GPU_Analyzer.ViewModels.Commands;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using static Vortice.Direct3D11.D3D11;

namespace GPU_Analyzer.ViewModels
{
    public class VramCheckViewModel : INotifyPropertyChanged
    {
        public string Title => "Проверка VRAM";
        public DiagnosticsViewModel ParentDiagnostics { get; set; }

        private string vramResult;
        public string VramResult
        {
            get => vramResult;
            set { vramResult = value; OnPropertyChanged(); }
        }

        public ICommand StartVramTestCommand { get; }

        public VramCheckViewModel()
        {
            StartVramTestCommand = new RelayCommand(async _ => await StartVramTest());
        }

        private async Task StartVramTest()
        {
            

            VramResult = "Запуск VRAM теста...";
            string result = await Task.Run(RunVramTest);
            VramResult = result;
        }

        private string RunVramTest()
        {

            ID3D11Device device;
            ID3D11DeviceContext context;
            //временное устройство DirectX
            if (D3D11CreateDevice(
                    null,
                    DriverType.Hardware,
                    DeviceCreationFlags.BgraSupport,
                    new[] { FeatureLevel.Level_11_0 },
                    out device,
                    out context).Failure)

                
            {
                return "Ошибка: не удалось создать DirectX устройство.";
            }

            try
            {
                return RunVramTestCore(device, context, 256); // 256 МБ
            }
            finally
            {
                
                context?.Dispose();
                device?.Dispose();
            }
        }

        private string RunVramTestCore(ID3D11Device device, ID3D11DeviceContext context, int testSizeMB)
        {
            int totalBytes = testSizeMB * 1024 * 1024;
            if (totalBytes % 4 != 0) totalBytes = (totalBytes / 4) * 4;

            // буфер в VRAM
            var gpuBufferDesc = new BufferDescription(
                (uint)totalBytes,
                BindFlags.None,
                ResourceUsage.Default,
                CpuAccessFlags.None
            );

            using var gpuBuffer = device.CreateBuffer(gpuBufferDesc);
            var stagingDesc = new BufferDescription(
                (uint)totalBytes,
                BindFlags.None,
                ResourceUsage.Staging,
                CpuAccessFlags.Read
            );

            using var stagingBuffer = device.CreateBuffer(stagingDesc);

            uint[] patterns = { 0xAAAAAAAA, 0x55555555, 0xFFFFFFFF };
            var errors = new StringBuilder();

            foreach (uint pattern in patterns)
            {
                var cpuData = new uint[totalBytes / 4];
                for (int i = 0; i < cpuData.Length; i++)
                {
                    cpuData[i] = pattern;
                }
                    
                // загрузка в GPU
                var handle = GCHandle.Alloc(cpuData, GCHandleType.Pinned);
                try
                {
                    context.UpdateSubresource(gpuBuffer, 0, null, handle.AddrOfPinnedObject(), 0, 0);
                }
                finally
                {
                    handle.Free();
                }
                context.CopyResource(stagingBuffer, gpuBuffer);

                // проверка
                if (context.Map(stagingBuffer, 0, MapMode.Read, MapFlags.None, out var mapped).Success)
                {
                    unsafe
                    {
                        uint* ptr = (uint*)mapped.DataPointer.ToPointer();
                        for (int i = 0; i < cpuData.Length; i++)
                        {
                            if (ptr[i] != pattern)
                            {
                                errors.AppendLine($"Ошибка @ {i * 4} байт: ожидалось {pattern:X8}, получено {ptr[i]:X8}");
                                if (errors.Length > 1000) break;
                            }
                        }
                    }
                    context.Unmap(stagingBuffer, 0);
                }
                else
                {
                    return "Ошибка: не удалось прочитать staging-буфер.";
                }
            }

            return errors.Length == 0 ? $"√ VRAM ({testSizeMB} МБ): ошибок не найдено." : $"x Найдено ошибок:\n{errors}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
