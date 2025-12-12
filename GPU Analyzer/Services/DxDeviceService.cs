using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace GPU_Analyzer.Services
{
    public static class DxDeviceService
    {
        public static ID3D11Device? Device { get; set; }
        public static ID3D11DeviceContext? Context  { get; set; }
    }
}
