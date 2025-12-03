using System;
using System.Numerics;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.D3DCompiler;
using static Vortice.Direct3D11.D3D11;
using System.Runtime.CompilerServices;
using System.IO;
using Vortice;


namespace GPU_Analyzer.StressTests
{
    public class DxSimpleRenderer : IDisposable
    {


        private readonly IntPtr _hwnd;
        private readonly uint _width;
        private readonly uint _height;

        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private IDXGISwapChain? _swapChain;
        private ID3D11RenderTargetView? _rtv;
        private ID3D11DepthStencilView? _depthView;

        private Thread? _renderThread;
        private volatile bool _running;
        
        // кубы
        private ID3D11Buffer? _vb;
        private ID3D11Buffer? _ib;
        private ID3D11Buffer? _constantBuffer;

        private ID3D11VertexShader? _vs;
        private ID3D11PixelShader? _ps;
        private ID3D11InputLayout? _layout;

        private Matrix4x4 _proj, _view;




        public DxSimpleRenderer(IntPtr hwnd, uint width = 800, uint height = 600)
        {
            _hwnd = hwnd;
            _width = width;
            _height = height;
        }

        public void Start()
        {
            if (_running) return;

            InitializeDirectX();

            _running = true;
            _renderThread = new Thread(RenderLoop)
            {
                IsBackground = true,
                Name = "DxSimpleRendererThread"
            };
            _renderThread.Start();
        }

        public void Stop()
        {
            _running = false;
            _renderThread?.Join(1000);
            Cleanup();
        }

        private void InitializeDirectX()
        {
            // Полное описание BufferDescription
            var modeDesc = new ModeDescription(
                _width,
                _height,
                new Rational(60, 1),
                Format.R8G8B8A8_UNorm
            );
            
            var swapDesc = new SwapChainDescription
            {
                BufferDescription = modeDesc,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputWindow = _hwnd,
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                Flags = SwapChainFlags.None
            };

            // Device
            FeatureLevel? returnedLevel;

            var result = D3D11CreateDeviceAndSwapChain(
                null,                                // adapter
                DriverType.Hardware,                 // GPU
                DeviceCreationFlags.BgraSupport,     // нужен для WPF совместимости
                new[] { FeatureLevel.Level_11_0 },   // уровень устройства
                swapDesc,
                out _swapChain!,
                out _device!,
                out returnedLevel,
                out _context!
            );

            var depthDesc = new Texture2DDescription
            {
                Width = _width,
                Height = _height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.DepthStencil
            };

            var rasterDesc = new RasterizerDescription
            {
                CullMode = CullMode.None,  // отключаем отсечение
                FillMode = FillMode.Solid,
                FrontCounterClockwise = false
            };

            var rasterState = _device.CreateRasterizerState(rasterDesc);
            _context.RSSetState(rasterState);

            using var depthTex = _device.CreateTexture2D(depthDesc);
            _depthView = _device.CreateDepthStencilView(depthTex);

            

            if (result.Failure)
            {
                throw new Exception("Failed to create D3D11 device: " + result.Code);
            }

            // RT view
            using var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            _rtv = _device.CreateRenderTargetView(backBuffer);
            // назначаем RTV + Depth
            _context.OMSetRenderTargets(_rtv!, _depthView!);

            _context!.RSSetViewports(new[] { new Viewport(0, 0, _width, _height) });
            CreateShaders();
            CreateCubeGeometry();
            CreateMatrices();
        }

        private void CreateShaders()
        {
            System.Diagnostics.Debug.WriteLine(Path.GetFullPath("StressTests/Shaders/CubeVS.hlsl"));
            System.Diagnostics.Debug.WriteLine(File.Exists("StressTests/Shaders/CubeVS.hlsl"));
            // Компиляция .hlsl в рантайме через Vortice
            var vsBytecode = Compiler.CompileFromFile("StressTests/Shaders/CubeVS.hlsl", "main", "vs_5_0");
            var psBytecode = Compiler.CompileFromFile("StressTests/Shaders/CubePS.hlsl", "main", "ps_5_0");

            _vs = _device!.CreateVertexShader(vsBytecode.Span);
            _ps = _device!.CreatePixelShader(psBytecode.Span);

            var elements = new[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0,0, InputClassification.PerVertexData, 0)
            };

            _layout = _device!.CreateInputLayout(elements, vsBytecode.ToArray());
            _constantBuffer = _device!.CreateBuffer(
                new BufferDescription(
                    (uint)Unsafe.SizeOf<Matrix4x4>(),
                    BindFlags.ConstantBuffer,
                    ResourceUsage.Dynamic,
                    CpuAccessFlags.Write
                    )
                );
        }

        private void CreateCubeGeometry()
        {
            //8 вершин куба
            var vertices = new[]
            {
                new Vector3(-1,-1,-1),
                new Vector3(-1,+1,-1),
                new Vector3(+1,+1,-1),
                new Vector3(+1,-1,-1),

                new Vector3(-1,-1,+1),
                new Vector3(-1,+1,+1),
                new Vector3(+1,+1,+1),
                new Vector3(+1,-1,+1)
            };
            ushort[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                0,1,5, 0,5,4,
                2,3,7, 2,7,6,
                1,2,6, 1,6,5,
                0,3,7, 0,7,4
            };

            _vb = _device!.CreateBuffer(vertices, BindFlags.VertexBuffer);
            _ib = _device!.CreateBuffer(indices, BindFlags.IndexBuffer);
        }

        private void CreateMatrices()
        {
            float aspect = (float)_width / _height;
            _proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, aspect, 0.1f, 100f);
            _view = Matrix4x4.CreateLookAt(new Vector3(0,0,-5), Vector3.Zero, Vector3.UnitY);
        }

        private void RenderLoop()
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                while (_running)
                {
                    float t = (float)sw.Elapsed.TotalSeconds;

                    // плавный градиент
                    var r = 0.5f + 0.5f * (float)Math.Sin(t);
                    var g = 0.5f + 0.5f * (float)Math.Sin(t * 1.3);
                    var b = 0.5f + 0.5f * (float)Math.Sin(t * 1.7 + 1.0f);

                    var color = new Color4(r, g, b, 1.0f);
                    //_context.OMSetRenderTargets(_rtv!, _depthView!);
                    _context.ClearDepthStencilView(_depthView!, DepthStencilClearFlags.Depth, 1.0f, 0);
                    _context!.ClearRenderTargetView(_rtv!, color);

                    RenderCube(t);
                    
                    _swapChain!.Present(1, PresentFlags.None);

                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("RenderLoop error: " + e);
            }
        }

        private void RenderCube(double t)
        {
            // модель (вращаем куб)
            var model = Matrix4x4.CreateRotationY((float)t) * Matrix4x4.CreateRotationX((float)(t * 0.5));

            
            var mvp = model * _view * _proj;
            var mvpTransposed = Matrix4x4.Transpose(mvp);

            // Map constant buffer (Vortice -> MappedSubresource)
            MappedSubresource mapped;
            _context!.Map(_constantBuffer!, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None, out mapped);

            // Копируем матрицу (unsafe и Unsafe.Copy — быстрый вариант)
            unsafe
            {
                Unsafe.Copy((void*)mapped.DataPointer.ToPointer(), ref mvpTransposed);
            }

            _context.Unmap(_constantBuffer!, 0);

            // Настроим pipeline
            // Input assembler
            var stride = (uint)(sizeof(float) * 3); // Vector3 позиция (3 floats)
            var offset = 0u;
            _context.IASetVertexBuffers(0, new[] { _vb! }, new[] { stride }, new[] { offset });
            _context.IASetIndexBuffer(_ib!, Format.R16_UInt, 0);
            _context.IASetInputLayout(_layout!);
            _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            // Шейдеры и константные буферы
            _context.VSSetShader(_vs);
            _context.PSSetShader(_ps);
            _context.VSSetConstantBuffer(0, _constantBuffer);
            // если нужно, можно и PSSetConstantBuffer(0, _constantBuffer);

            // Рисуем
            _context.DrawIndexed(36, 0, 0); // 12 треугольников * 3 = 36 индексов
        }

        private void Cleanup()
        {
            _rtv?.Dispose();
            _swapChain?.Dispose();
            _context?.Dispose();
            _device?.Dispose();

            _rtv = null;
            _swapChain = null;
            _context = null;
            _device = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
