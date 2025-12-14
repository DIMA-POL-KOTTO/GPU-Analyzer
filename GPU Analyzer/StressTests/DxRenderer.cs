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
using System.Diagnostics;


namespace GPU_Analyzer.StressTests
{
    public class DxSimpleRenderer : IDisposable
    {


        private readonly IntPtr _hwnd;
        private readonly uint _width;
        private readonly uint _height;
        private readonly int _cubeCount = 300000;

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
        private ID3D11Buffer? _instanceBuffer;
        private ID3D11Buffer? _constantBuffer;

        private ID3D11VertexShader? _vs;
        private ID3D11PixelShader? _ps;
        private ID3D11InputLayout? _layout;

        private Matrix4x4 _proj, _view;


        private InstanceData[] _instanceData;
        private Random _random;


        public DxSimpleRenderer(IntPtr hwnd, uint width = 1920, uint height = 1080)
        {
            _hwnd = hwnd;
            _width = width;
            _height = height;
            _random = new Random();
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
                new Rational(0, 1),
                Format.R8G8B8A8_UNorm
            );

            var swapDesc = new SwapChainDescription
            {
                BufferDescription = modeDesc,
                SampleDescription = new SampleDescription(8, 0),
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
                SampleDescription = new SampleDescription(8, 0),
                BindFlags = BindFlags.DepthStencil
            };

            var rasterDesc = new RasterizerDescription
            {
                CullMode = CullMode.None,  // отключаем отсечение
                FillMode = FillMode.Solid,
                FrontCounterClockwise = false,
                DepthClipEnable = true //???
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
            CreateInstanceData();
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
                // Вершинный поток
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0,0, InputClassification.PerVertexData, 0),
                // Поток инстансов (1-й инстанс-буфер)
                new InputElementDescription("INSTANCE_POS", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("INSTANCE_COLOR", 0, Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("INSTANCE_ROT", 0, Format.R32_Float, 32, 1, InputClassification.PerInstanceData, 1)
            };

            _layout = _device!.CreateInputLayout(elements, vsBytecode.ToArray());
            _constantBuffer = _device!.CreateBuffer(
                new BufferDescription(
                    (uint)(Unsafe.SizeOf<Matrix4x4>() + sizeof(float) + 12),
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

        private void CreateInstanceData()
        {
            _instanceData = new InstanceData[_cubeCount];

            for (int i = 0; i < _cubeCount; i++)
            {
                _instanceData[i] = new InstanceData
                {

                    Position = new Vector4(
                        (float)(_random.NextDouble() * 60 - 30),  // X: -20..20
                        (float)(_random.NextDouble() * 60 - 30),  // Y: -20..20  
                        (float)(_random.NextDouble() * 60 - 30),  // Z: -20..20
                        1.0f
                    ),
                    Color = new Color4(
                        (float)_random.NextDouble(),
                        (float)_random.NextDouble(),
                        (float)_random.NextDouble(),
                        1.0f
                    ),
                    Rotation = (float)(_random.NextDouble() * Math.PI * 2)

                };
            }
            var desc = new BufferDescription(
                (uint)(Unsafe.SizeOf<InstanceData>() * _cubeCount),
                BindFlags.VertexBuffer,
                ResourceUsage.Dynamic,
                CpuAccessFlags.Write
                );
            _instanceBuffer = _device!.CreateBuffer(desc);

            UpdateInstanceBuffer();
        }

        private void UpdateInstanceBuffer()
        {
            MappedSubresource mapped;
            _context!.Map(_instanceBuffer!, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None, out mapped);

            unsafe
            {
                Unsafe.CopyBlock(
                    mapped.DataPointer.ToPointer(),
                    Unsafe.AsPointer(ref _instanceData[0]),
                    (uint)(Unsafe.SizeOf<InstanceData>() * _cubeCount)
                );
            }

            _context.Unmap(_instanceBuffer!, 0);

        }

        private void CreateMatrices()
        {
            float aspect = (float)_width / _height;
            _proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, aspect, 1.0f, 1000f);
            _view = Matrix4x4.CreateLookAt(new Vector3(0, 0, -50), Vector3.Zero, Vector3.UnitY);
        }

        private void RenderLoop()
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                while (_running)
                {
                    float t = (float)sw.Elapsed.TotalSeconds;
                    // Обновляем вращение инстансов
                    UpdateInstances(t);
                    // Очистка
                    var bgColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
                    _context.ClearDepthStencilView(_depthView!, DepthStencilClearFlags.Depth, 1.0f, 0);
                    _context!.ClearRenderTargetView(_rtv!, bgColor);



                    RenderCubes(t);

                    _swapChain!.Present(0, PresentFlags.None);


                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("RenderLoop error: " + e);
            }
        }
        private void UpdateInstances(float time)
        {
            // Можно добавить анимацию позиций/цветов если нужно
            for (int i = 0; i < _cubeCount; i++)
            {
                // Пример: плавное изменение цвета
                _instanceData[i].Color = new Color4(
                    (float)(0.5 + 0.5 * Math.Sin(time + i * 0.01)),
                    (float)(0.5 + 0.5 * Math.Sin(time * 1.3 + i * 0.01)),
                    (float)(0.5 + 0.5 * Math.Sin(time * 1.7 + i * 0.01)),
                    1.0f
                );
            }

            UpdateInstanceBuffer();
        }


        private void RenderCubes(double t)
        {

            // Проверяем буферы
            if (_vb == null || _ib == null || _instanceBuffer == null || _constantBuffer == null)
                return;
            // Матрица вида-проекции
            var viewProj = Matrix4x4.Transpose(_view * _proj);
            float time = (float)t;
            // Обновляем константный буфер
            MappedSubresource mapped;
            _context!.Map(_constantBuffer!, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None, out mapped);

            unsafe
            {
                byte* dataPtr = (byte*)mapped.DataPointer.ToPointer();
                // Копируем матрицу
                Unsafe.Copy(dataPtr, ref viewProj);
                // Копируем время (после матрицы)
                float* timePtr = (float*)(dataPtr + Unsafe.SizeOf<Matrix4x4>());
                *timePtr = time;
            }

            _context.Unmap(_constantBuffer!, 0);

            // Настраиваем пайплайн
            var vertexStride = (uint)(sizeof(float) * 3); // Vector3
            var instanceStride = (uint)Unsafe.SizeOf<InstanceData>();

            var vertexBuffers = new[] { _vb!, _instanceBuffer! };
            var strides = new[] { vertexStride, instanceStride };
            var offsets = new[] { 0u, 0u };

            _context.IASetVertexBuffers(0, vertexBuffers, strides, offsets);
            _context.IASetIndexBuffer(_ib!, Format.R16_UInt, 0);
            _context.IASetInputLayout(_layout!);
            _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            // Шейдеры
            _context.VSSetShader(_vs);
            _context.PSSetShader(_ps);
            _context.VSSetConstantBuffer(0, _constantBuffer);

            // Рисуем с инстансингом
            _context.DrawIndexedInstanced(36, (uint)_cubeCount, 0, 0, 0);
        }

        private void Cleanup()
        {
            _rtv?.Dispose();
            _depthView?.Dispose();
            _swapChain?.Dispose();
            _context?.Dispose();
            _device?.Dispose();

            // Буферы
            _vb?.Dispose();
            _ib?.Dispose();
            _instanceBuffer?.Dispose();
            _constantBuffer?.Dispose();

            // Шейдеры
            _vs?.Dispose();
            _ps?.Dispose();
            _layout?.Dispose();

            _rtv = null;
            _swapChain = null;
            _context = null;
            _device = null;
        }

        public void Dispose()
        {
            Stop();
        }

        // Структура данных для инстансов
        private struct InstanceData
        {
            public Vector4 Position;
            public Color4 Color;
            public float Rotation;
        }
    }
}
