// ============================================================================
// V4L2 异步视频捕获封装层
// 支持：MMAP（CPU 内存）和 DMA-BUF（零拷贝硬件导出）两种模式
// 支持：Pull 拉取模式 和 Push 回调模式
// 依赖：AkiraScan.Camera.Platform.Linux.V4L2PInvoke
// ============================================================================

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static AkiraScan.Camera.Platform.Linux.V4L2PInvoke;

namespace AkiraScan.Camera.Platform.Linux;

#region 事件参数

/// <summary>
/// 帧到达事件参数
/// </summary>
internal sealed class V4L2FrameEventArgs : EventArgs
{
    /// <summary>捕获到的帧。事件处理完成后必须调用 Dispose()。</summary>
    public V4L2Frame Frame { get; }

    public V4L2FrameEventArgs(V4L2Frame frame) => Frame = frame;
}

/// <summary>
/// 捕获错误事件参数
/// </summary>
internal sealed class V4L2ErrorEventArgs : EventArgs
{
    /// <summary>异常信息</summary>
    public Exception Exception { get; }

    /// <summary>是否致命错误（若为 true，捕获循环已自动停止）</summary>
    public bool IsFatal { get; }

    public V4L2ErrorEventArgs(Exception exception, bool isFatal)
    {
        Exception = exception;
        IsFatal = isFatal;
    }
}

#endregion

#region 枚举与配置

/// <summary>
/// V4L2 缓冲区模式
/// </summary>
internal enum V4L2BufferMode
{
    /// <summary>
    /// 内存映射模式（CPU 可直接访问）
    /// </summary>
    Mmap = (int)v4l2_memory.V4L2_MEMORY_MMAP,

    /// <summary>
    /// DMA-BUF 导出模式（用于零拷贝 GPU/硬件处理）
    /// </summary>
    DmaBuf = (int)v4l2_memory.V4L2_MEMORY_DMABUF,
}

/// <summary>
/// V4L2 视频格式配置
/// </summary>
internal sealed class V4L2VideoFormat
{
    /// <summary>宽度（像素）</summary>
    public uint Width { get; set; }

    /// <summary>高度（像素）</summary>
    public uint Height { get; set; }

    /// <summary>像素格式（FourCC，如 V4L2_PIX_FMT_MJPEG）</summary>
    public uint PixelFormat { get; set; } = V4L2_PIX_FMT_MJPEG;

    /// <summary>场序</summary>
    public v4l2_field Field { get; set; } = v4l2_field.V4L2_FIELD_ANY;

    /// <summary>颜色空间</summary>
    public uint Colorspace { get; set; } = 0;

    /// <summary>转换为 V4L2 原生格式结构</summary>
    internal v4l2_format ToNative(v4l2_buf_type type)
    {
        return new v4l2_format
        {
            type = (uint)type,
            fmt = new v4l2_format.union1
            {
                pix = new v4l2_pix_format
                {
                    width = Width,
                    height = Height,
                    pixelformat = PixelFormat,
                    field = (uint)Field,
                    colorspace = Colorspace,
                    bytesperline = 0, // 驱动自动计算
                    sizeimage = 0     // 驱动自动计算
                }
            }
        };
    }

    /// <summary>从 V4L2 原生格式结构解析</summary>
    internal static V4L2VideoFormat FromNative(in v4l2_format fmt)
    {
        ref readonly var pix = ref fmt.fmt.pix;
        return new V4L2VideoFormat
        {
            Width = pix.width,
            Height = pix.height,
            PixelFormat = pix.pixelformat,
            Field = (v4l2_field)pix.field,
            Colorspace = pix.colorspace
        };
    }

    /// <summary>获取 FourCC 字符串表示</summary>
    public string PixelFormatString => FourCCToString(PixelFormat);

    private static unsafe string FourCCToString(uint fourcc)
    {
        byte* bytes = (byte*)&fourcc;
        return new string((sbyte*)bytes, 0, 4, System.Text.Encoding.ASCII);
    }
}

#endregion

#region 帧数据封装

/// <summary>
/// 捕获帧数据抽象。使用后<strong>必须</strong>调用 Dispose() 释放资源。
/// <para>
/// <b>MMAP 模式</b>：通过 <see cref="GetSpan"/> 获取 CPU 可读的 Span&lt;byte&gt;
/// </para>
/// <para>
/// <b>DMA-BUF 模式</b>：通过 <see cref="GetDmaBufHandle"/> 获取 SafeFileHandle，
/// 用于导入到 EGL/DRM/VAAPI 进行零拷贝 GPU 处理
/// </para>
/// </summary>
internal sealed class V4L2Frame : IDisposable
{
    private readonly V4L2BufferMode _mode;
    private readonly uint _bytesUsed;
    private readonly uint _sequence;
    private readonly long _timestampUs;
    private readonly uint _width;
    private readonly uint _height;
    private readonly uint _pixelFormat;

    // MMAP 模式数据
    private readonly IntPtr _mmapPtr;
    private readonly uint _mmapLength;

    // DMA-BUF 模式数据
    private readonly SafeFileHandle? _dmaBufHandle;

    // 释放时重新入队的回调（由 V4L2Capture 注入）
    private readonly Action<uint>? _onDispose;
    private readonly uint _bufferIndex;
    private bool _disposed;

    internal V4L2Frame(
        in v4l2_buffer buffer,
        V4L2BufferMode mode,
        IntPtr mmapPtr,
        uint mmapLength,
        uint width, uint height, uint pixelFormat,
        Action<uint>? onDispose = null)
    {
        _mode = mode;
        _bytesUsed = buffer.bytesused;
        _sequence = buffer.sequence;
        _timestampUs = buffer.timestamp.tv_sec.ToInt64() * 1_000_000L + buffer.timestamp.tv_usec.ToInt64();
        _width = width;
        _height = height;
        _pixelFormat = pixelFormat;
        _mmapPtr = mmapPtr;
        _mmapLength = mmapLength;
        _dmaBufHandle = null;
        _bufferIndex = buffer.index;
        _onDispose = onDispose;
    }

    internal V4L2Frame(
        in v4l2_buffer buffer,
        SafeFileHandle dmaBufHandle,
        uint width, uint height, uint pixelFormat,
        Action<uint>? onDispose = null)
    {
        _mode = V4L2BufferMode.DmaBuf;
        _bytesUsed = buffer.bytesused;
        _sequence = buffer.sequence;
        _timestampUs = buffer.timestamp.tv_sec.ToInt64() * 1_000_000L + buffer.timestamp.tv_usec.ToInt64();
        _width = width;
        _height = height;
        _pixelFormat = pixelFormat;
        _mmapPtr = IntPtr.Zero;
        _mmapLength = 0;
        _dmaBufHandle = dmaBufHandle;
        _bufferIndex = buffer.index;
        _onDispose = onDispose;
    }

    /// <summary>缓冲区模式</summary>
    public V4L2BufferMode Mode => _mode;

    /// <summary>实际使用的字节数</summary>
    public uint BytesUsed => _bytesUsed;

    /// <summary>帧序列号（单调递增）</summary>
    public uint Sequence => _sequence;

    /// <summary>时间戳（微秒，从设备启动开始计数）</summary>
    public long TimestampUs => _timestampUs;

    /// <summary>图像宽度</summary>
    public uint Width => _width;

    /// <summary>图像高度</summary>
    public uint Height => _height;

    /// <summary>像素格式 FourCC</summary>
    public uint PixelFormat => _pixelFormat;

    /// <summary>内部缓冲区索引</summary>
    internal uint BufferIndex => _bufferIndex;

    /// <summary>
    /// 获取 CPU 可访问的帧数据（仅 MMAP 模式有效）
    /// </summary>
    /// <exception cref="InvalidOperationException">非 MMAP 模式或映射无效</exception>
    public unsafe Span<byte> GetSpan()
    {
        if (_mode != V4L2BufferMode.Mmap)
            throw new InvalidOperationException("GetSpan() 仅在 MMAP 模式下可用。当前模式: " + _mode);
        if (_mmapPtr == IntPtr.Zero || _mmapPtr == MAP_FAILED)
            throw new InvalidOperationException("内存映射无效");
        return new Span<byte>((void*)_mmapPtr, (int)_bytesUsed);
    }

    /// <summary>
    /// 获取 DMA-BUF 文件句柄（仅 DMABUF 模式有效）
    /// </summary>
    /// <returns>可用于 EGL/DRM/VAAPI 导入的跨进程文件句柄</returns>
    /// <exception cref="InvalidOperationException">非 DMABUF 模式或句柄无效</exception>
    public SafeFileHandle GetDmaBufHandle()
    {
        if (_mode != V4L2BufferMode.DmaBuf)
            throw new InvalidOperationException("GetDmaBufHandle() 仅在 DMABUF 模式下可用。当前模式: " + _mode);
        if (_dmaBufHandle == null || _dmaBufHandle.IsInvalid)
            throw new InvalidOperationException("DMA-BUF 句柄无效");
        return _dmaBufHandle;
    }

    /// <summary>
    /// 将帧数据复制到目标 Span（仅 MMAP 模式有效）
    /// </summary>
    public void CopyTo(Span<byte> destination)
    {
        if (_mode != V4L2BufferMode.Mmap)
            throw new InvalidOperationException("CopyTo() 仅在 MMAP 模式下可用");
        GetSpan().Slice(0, (int)Math.Min(_bytesUsed, (uint)destination.Length)).CopyTo(destination);
    }

    /// <summary>
    /// 将帧数据复制到流（仅 MMAP 模式有效）
    /// </summary>
    public void CopyTo(Stream stream)
    {
        if (_mode != V4L2BufferMode.Mmap)
            throw new InvalidOperationException("CopyTo(Stream) 仅在 MMAP 模式下可用");
        var span = GetSpan();
        stream.Write(span);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_mode == V4L2BufferMode.Mmap && _mmapPtr != IntPtr.Zero && _mmapPtr != MAP_FAILED)
        {
            munmap(_mmapPtr, new UIntPtr(_mmapLength));
        }
        _dmaBufHandle?.Dispose();

        // 通知捕获引擎重新入队（回调模式）
        _onDispose?.Invoke(_bufferIndex);
    }
}

#endregion

#region 安全句柄

/// <summary>
/// V4L2 设备文件安全句柄，自动管理 close()
/// </summary>
internal sealed class V4L2SafeHandle : SafeHandle
{
    public V4L2SafeHandle(int fd) : base(new IntPtr(-1), true)
    {
        if (fd >= 0)
            SetHandle(new IntPtr(fd));
    }

    public override bool IsInvalid => handle == new IntPtr(-1) || handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            close(handle.ToInt32());
        }
        return true;
    }

    public int FileDescriptor => handle.ToInt32();
}

#endregion

#region 主捕获类

/// <summary>
/// V4L2 异步视频捕获封装。支持 MMAP 和 DMA-BUF 两种内存模式，
/// 同时支持 <b>Pull 拉取</b>和 <b>Push 回调</b>两种数据消费模式。
/// </summary>
/// <example>
/// <b>Pull 模式示例（手动拉取）：</b>
/// <code>
/// using var capture = new V4L2Capture("/dev/video0", V4L2BufferMode.Mmap, 4);
/// capture.SetFormat(new V4L2VideoFormat { Width = 1920, Height = 1080, PixelFormat = V4L2_PIX_FMT_MJPEG });
/// capture.InitBuffers();
/// capture.StartStream();
/// 
/// using var frame = await capture.CaptureFrameAsync();
/// File.WriteAllBytes("frame.jpg", frame.GetSpan().ToArray());
/// </code>
/// 
/// <b>Push 模式示例（回调接收）：</b>
/// <code>
/// using var capture = new V4L2Capture("/dev/video0", V4L2BufferMode.DmaBuf, 4);
/// capture.SetFormat(new V4L2VideoFormat { Width = 1920, Height = 1080, PixelFormat = V4L2_PIX_FMT_YUV420 });
/// capture.InitBuffers();
/// 
/// capture.FrameArrived += (s, e) =>
/// {
///     using var frame = e.Frame;
///     var dmaBuf = frame.GetDmaBufHandle();
///     // 导入到 GPU / DRM...
/// };
/// capture.ErrorOccurred += (s, e) => Console.WriteLine($"Error: {e.Exception.Message}");
/// 
/// capture.StartCaptureLoop();  // 后台线程开始推送
/// // ...
/// capture.StopCaptureLoop();
/// </code>
/// </example>
internal sealed class V4L2Capture : IDisposable
{
    private readonly V4L2SafeHandle _handle;
    private readonly V4L2BufferMode _mode;
    private readonly uint _bufferCount;
    private readonly v4l2_buf_type _bufType;

    // MMAP 模式下内存映射信息 [index] -> ptr/length
    private readonly IntPtr[] _mmapPtrs;
    private readonly uint[] _mmapLengths;

    // DMA-BUF 模式下导出的句柄 [index] -> SafeFileHandle
    private readonly SafeFileHandle?[] _dmaBufHandles;

    // 缓冲区状态 [index] -> 是否已入队（可被驱动写入）
    private readonly bool[] _bufferQueued;
    private bool _streaming;
    private V4L2VideoFormat? _currentFormat;
    private readonly object _syncLock = new();

    // 回调模式相关
    private Thread? _captureThread;
    private CancellationTokenSource? _captureCts;
    private bool _isCaptureLoopRunning;

    /// <summary>
    /// 当新一帧到达时触发。事件参数中的 <see cref="V4L2Frame"/> 必须由处理程序调用 <c>Dispose()</c>，
    /// 否则该缓冲区不会被重新入队，最终所有缓冲区耗尽导致捕获暂停。
    /// </summary>
    public event EventHandler<V4L2FrameEventArgs>? FrameArrived;

    /// <summary>
    /// 当捕获循环中发生错误时触发。
    /// </summary>
    public event EventHandler<V4L2ErrorEventArgs>? ErrorOccurred;

    /// <summary>设备路径，如 /dev/video0</summary>
    public string DevicePath { get; }

    /// <summary>缓冲区模式</summary>
    public V4L2BufferMode BufferMode => _mode;

    /// <summary>当前视频格式（SetFormat 或 GetFormat 后有效）</summary>
    public V4L2VideoFormat? CurrentFormat => _currentFormat;

    /// <summary>是否正在流式传输</summary>
    public bool IsStreaming => _streaming;

    /// <summary>回调模式的捕获循环是否正在运行</summary>
    public bool IsCaptureLoopRunning => _isCaptureLoopRunning;

    /// <summary>内部设备句柄（高级用途）</summary>
    internal V4L2SafeHandle SafeHandle => _handle;

    /// <summary>
    /// 创建 V4L2 捕获实例
    /// </summary>
    /// <param name="devicePath">设备路径，如 /dev/video0</param>
    /// <param name="mode">缓冲区模式：Mmap（CPU 内存）或 DmaBuf（硬件导出）</param>
    /// <param name="bufferCount">缓冲区数量（建议 3-4）</param>
    /// <param name="bufType">缓冲区类型，默认 VIDEO_CAPTURE</param>
    public unsafe V4L2Capture(
        string devicePath,
        V4L2BufferMode mode = V4L2BufferMode.Mmap,
        uint bufferCount = 4,
        v4l2_buf_type bufType = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE)
    {
        if (bufferCount == 0 || bufferCount > VIDEO_MAX_FRAME)
            throw new ArgumentOutOfRangeException(nameof(bufferCount), $"缓冲区数量必须在 1-{VIDEO_MAX_FRAME} 之间");

        DevicePath = devicePath ?? throw new ArgumentNullException(nameof(devicePath));
        _mode = mode;
        _bufferCount = bufferCount;
        _bufType = bufType;
        _mmapPtrs = new IntPtr[bufferCount];
        _mmapLengths = new uint[bufferCount];
        _dmaBufHandles = mode == V4L2BufferMode.DmaBuf ? new SafeFileHandle[bufferCount] : null!;
        _bufferQueued = new bool[bufferCount];
        _streaming = false;

        // 打开设备（非阻塞 + 执行时关闭）
        int fd = open(devicePath, O_RDWR | O_NONBLOCK | O_CLOEXEC);
        if (fd < 0)
        {
            int errno = Marshal.GetLastSystemError();
            throw new IOException($"无法打开 V4L2 设备 {devicePath}: {GetErrorMessage(errno)} (errno={errno})");
        }

        _handle = new V4L2SafeHandle(fd);

        // 验证设备能力
        VerifyCapabilities();
    }

    #region 设备配置

    /// <summary>
    /// 验证设备支持所需功能
    /// </summary>
    private unsafe void VerifyCapabilities()
    {
        var cap = new v4l2_capability();
        int result = ioctl(_handle.FileDescriptor, VIDIOC_QUERYCAP, ref cap);
        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            throw new IOException($"VIDIOC_QUERYCAP 失败: {GetErrorMessage(errno)} (errno={errno})");
        }

        bool supportsCapture = (cap.capabilities & V4L2_CAP_VIDEO_CAPTURE) != 0 ||
                               (cap.capabilities & V4L2_CAP_VIDEO_CAPTURE_MPLANE) != 0;
        if (!supportsCapture)
            throw new NotSupportedException("设备不支持视频捕获（VIDEO_CAPTURE）");

        if (_mode == V4L2BufferMode.Mmap && (cap.capabilities & V4L2_CAP_STREAMING) == 0)
            throw new NotSupportedException("设备不支持流式传输（STREAMING），MMAP 模式不可用");
    }

    /// <summary>
    /// 设置视频格式。必须在 InitBuffers 之前调用。
    /// </summary>
    /// <param name="format">目标格式</param>
    /// <returns>驱动实际应用的格式（可能与请求不同）</returns>
    public unsafe V4L2VideoFormat SetFormat(V4L2VideoFormat format)
    {
        if (format == null) throw new ArgumentNullException(nameof(format));
        if (_streaming) throw new InvalidOperationException("流式传输进行中，无法更改格式");
        if (_isCaptureLoopRunning) throw new InvalidOperationException("捕获循环运行中，无法更改格式");

        var v4l2Fmt = format.ToNative(_bufType);

        int result = ioctl(_handle.FileDescriptor, VIDIOC_S_FMT, ref v4l2Fmt);
        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            throw new IOException($"VIDIOC_S_FMT 失败: {GetErrorMessage(errno)} (errno={errno})");
        }

        _currentFormat = V4L2VideoFormat.FromNative(v4l2Fmt);
        return _currentFormat;
    }

    /// <summary>
    /// 获取当前视频格式
    /// </summary>
    public unsafe V4L2VideoFormat GetFormat()
    {
        var v4l2Fmt = new v4l2_format { type = (uint)_bufType };
        int result = ioctl(_handle.FileDescriptor, VIDIOC_G_FMT, ref v4l2Fmt);
        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            throw new IOException($"VIDIOC_G_FMT 失败: {GetErrorMessage(errno)} (errno={errno})");
        }

        _currentFormat = V4L2VideoFormat.FromNative(v4l2Fmt);
        return _currentFormat;
    }

    /// <summary>
    /// 枚举设备支持的所有像素格式
    /// </summary>
    public unsafe uint[] EnumeratePixelFormats()
    {
        var formats = new List<uint>();
        uint index = 0;

        while (true)
        {
            var fmtdesc = new v4l2_fmtdesc
            {
                index = index,
                type = (uint)_bufType
            };

            int result = ioctl(_handle.FileDescriptor, VIDIOC_ENUM_FMT, ref fmtdesc);
            if (result < 0)
                break;

            formats.Add(fmtdesc.pixelformat);
            index++;
        }

        return formats.ToArray();
    }

    #endregion

    #region 缓冲区管理

    /// <summary>
    /// 初始化缓冲区：请求、查询、映射/导出，并入队所有缓冲区。
    /// 必须在 SetFormat 之后、StartStream/StartCaptureLoop 之前调用。
    /// </summary>
    public unsafe void InitBuffers()
    {
        if (_streaming) throw new InvalidOperationException("流式传输进行中，无法重新初始化");
        if (_isCaptureLoopRunning) throw new InvalidOperationException("捕获循环运行中，无法重新初始化");
        if (_currentFormat == null) throw new InvalidOperationException("请先调用 SetFormat() 设置视频格式");

        lock (_syncLock)
        {
            ReleaseBuffersInternal();

            var req = new v4l2_requestbuffers
            {
                count = _bufferCount,
                type = (uint)_bufType,
                memory = (uint)_mode
            };

            int result = ioctl(_handle.FileDescriptor, VIDIOC_REQBUFS, ref req);
            if (result < 0)
            {
                int errno = Marshal.GetLastSystemError();
                throw new IOException($"VIDIOC_REQBUFS 失败: {GetErrorMessage(errno)} (errno={errno})");
            }

            if (req.count < _bufferCount)
            {
                throw new IOException($"设备分配的缓冲区数量不足：请求 {_bufferCount}，实际 {req.count}");
            }

            for (uint i = 0; i < _bufferCount; i++)
            {
                var buf = new v4l2_buffer
                {
                    index = i,
                    type = (uint)_bufType,
                    memory = (uint)_mode
                };

                result = ioctl(_handle.FileDescriptor, VIDIOC_QUERYBUF, ref buf);
                if (result < 0)
                {
                    int errno = Marshal.GetLastSystemError();
                    throw new IOException($"VIDIOC_QUERYBUF 失败 (index={i}): {GetErrorMessage(errno)} (errno={errno})");
                }

                if (_mode == V4L2BufferMode.Mmap)
                {
                    uint length = buf.length;
                    uint offset = buf.m.offset;

                    IntPtr ptr = mmap(
                        IntPtr.Zero,
                        new UIntPtr(length),
                        PROT_READ | PROT_WRITE,
                        MAP_SHARED,
                        _handle.FileDescriptor,
                        new IntPtr(offset));

                    if (ptr == MAP_FAILED)
                    {
                        int errno = Marshal.GetLastSystemError();
                        throw new IOException($"mmap 失败 (index={i}): {GetErrorMessage(errno)} (errno={errno})");
                    }

                    _mmapPtrs[i] = ptr;
                    _mmapLengths[i] = length;
                }
                else if (_mode == V4L2BufferMode.DmaBuf)
                {
                    var expbuf = new v4l2_exportbuffer
                    {
                        type = (uint)_bufType,
                        index = i,
                        flags = O_CLOEXEC | O_RDWR
                    };

                    result = ioctl(_handle.FileDescriptor, VIDIOC_EXPBUF, ref expbuf);
                    if (result < 0)
                    {
                        int errno = Marshal.GetLastSystemError();
                        throw new IOException($"VIDIOC_EXPBUF 失败 (index={i}): {GetErrorMessage(errno)} (errno={errno})");
                    }

                    _dmaBufHandles[i] = new SafeFileHandle(new IntPtr(expbuf.fd), ownsHandle: true);
                }

                EnqueueBufferInternal(i);
            }
        }
    }

    /// <summary>
    /// 将缓冲区入队（内部方法）
    /// </summary>
    private unsafe void EnqueueBufferInternal(uint index)
    {
        var buf = new v4l2_buffer
        {
            index = index,
            type = (uint)_bufType,
            memory = (uint)_mode
        };

        if (_mode == V4L2BufferMode.DmaBuf)
        {
            buf.m.fd = _dmaBufHandles![index]!.DangerousGetHandle().ToInt32();
        }

        int result = ioctl(_handle.FileDescriptor, VIDIOC_QBUF, ref buf);
        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            throw new IOException($"VIDIOC_QBUF 失败 (index={index}): {GetErrorMessage(errno)} (errno={errno})");
        }

        _bufferQueued[index] = true;
    }

    /// <summary>
    /// 帧 Dispose 时的重新入队回调（用于回调模式）
    /// </summary>
    private void OnFrameDisposed(uint index)
    {
        lock (_syncLock)
        {
            if (_disposed) return;
            if (!_streaming) return;

            try
            {
                EnqueueBufferInternal(index);
            }
            catch (Exception ex)
            {
                // 重新入队失败，通知上层
                ErrorOccurred?.Invoke(this, new V4L2ErrorEventArgs(ex, isFatal: false));
            }
        }
    }

    /// <summary>
    /// 释放所有缓冲区资源
    /// </summary>
    private void ReleaseBuffersInternal()
    {
        if (_mode == V4L2BufferMode.Mmap)
        {
            for (int i = 0; i < _mmapPtrs.Length; i++)
            {
                if (_mmapPtrs[i] != IntPtr.Zero && _mmapPtrs[i] != MAP_FAILED)
                {
                    munmap(_mmapPtrs[i], new UIntPtr(_mmapLengths[i]));
                    _mmapPtrs[i] = IntPtr.Zero;
                    _mmapLengths[i] = 0;
                }
            }
        }
        else if (_mode == V4L2BufferMode.DmaBuf && _dmaBufHandles != null)
        {
            for (int i = 0; i < _dmaBufHandles.Length; i++)
            {
                _dmaBufHandles[i]?.Dispose();
                _dmaBufHandles[i] = null;
            }
        }

        Array.Fill(_bufferQueued, false);
    }

    #endregion

    #region 流控制

    /// <summary>
    /// 开始视频流式传输（驱动层开始填充缓冲区）
    /// </summary>
    public unsafe void StartStream()
    {
        lock (_syncLock)
        {
            if (_streaming) return;

            int type = (int)_bufType;
            int result = ioctl(_handle.FileDescriptor, VIDIOC_STREAMON, ref type);
            if (result < 0)
            {
                int errno = Marshal.GetLastSystemError();
                throw new IOException($"VIDIOC_STREAMON 失败: {GetErrorMessage(errno)} (errno={errno})");
            }

            _streaming = true;
        }
    }

    /// <summary>
    /// 停止视频流式传输
    /// </summary>
    public unsafe void StopStream()
    {
        lock (_syncLock)
        {
            if (!_streaming) return;

            int type = (int)_bufType;
            int result = ioctl(_handle.FileDescriptor, VIDIOC_STREAMOFF, ref type);
            if (result < 0)
            {
                int errno = Marshal.GetLastSystemError();
                throw new IOException($"VIDIOC_STREAMOFF 失败: {GetErrorMessage(errno)} (errno={errno})");
            }

            _streaming = false;
            Array.Fill(_bufferQueued, false);
        }
    }

    #endregion

    #region Push 回调模式

    /// <summary>
    /// 启动后台捕获循环，通过 <see cref="FrameArrived"/> 事件推送每一帧。
    /// 调用前必须先执行 <see cref="InitBuffers"/>。
    /// </summary>
    /// <param name="cancellationToken">可选的外部取消令牌</param>
    public void StartCaptureLoop(CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            if (_isCaptureLoopRunning) return;
            if (!_streaming) StartStream();

            _captureCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _isCaptureLoopRunning = true;

            _captureThread = new Thread(CaptureLoopWorker)
            {
                Name = $"V4L2Capture:{DevicePath}",
                IsBackground = true
            };
            _captureThread.Start();
        }
    }

    /// <summary>
    /// 停止后台捕获循环。会等待当前帧处理完成后退出。
    /// </summary>
    public void StopCaptureLoop()
    {
        lock (_syncLock)
        {
            if (!_isCaptureLoopRunning) return;

            _captureCts?.Cancel();
            _isCaptureLoopRunning = false;
        }

        _captureThread?.Join(5000);
        _captureThread = null;
        _captureCts?.Dispose();
        _captureCts = null;
    }

    /// <summary>
    /// 捕获循环工作线程
    /// </summary>
    private void CaptureLoopWorker()
    {
        var cts = _captureCts!;

        try
        {
            while (!cts.IsCancellationRequested)
            {
                // 等待帧就绪
                if (!PollForFrame(100))
                    continue;

                if (cts.IsCancellationRequested)
                    break;

                // 出队帧（不立即重新入队，等消费者 Dispose）
                V4L2Frame frame;
                lock (_syncLock)
                {
                    if (!_streaming || _disposed) break;
                    frame = DequeueFrameForCallback();
                }

                // 推送到事件（在锁外执行，避免阻塞捕获线程）
                try
                {
                    FrameArrived?.Invoke(this, new V4L2FrameEventArgs(frame));
                }
                catch (Exception ex)
                {
                    // 消费者事件处理异常，丢弃该帧并重新入队
                    ErrorOccurred?.Invoke(this, new V4L2ErrorEventArgs(ex, isFatal: false));
                }
            }
        }
        catch (Exception ex)
        {
            _isCaptureLoopRunning = false;
            ErrorOccurred?.Invoke(this, new V4L2ErrorEventArgs(ex, isFatal: true));
        }
    }

    /// <summary>
    /// 出队一帧用于回调模式（不立即重新入队，而是绑定 Dispose 回调）
    /// </summary>
    private unsafe V4L2Frame DequeueFrameForCallback()
    {
        var buf = new v4l2_buffer
        {
            type = (uint)_bufType,
            memory = (uint)_mode
        };

        int result = ioctl(_handle.FileDescriptor, VIDIOC_DQBUF, ref buf);
        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            if (errno == EAGAIN)
                throw new TimeoutException("没有可用的帧（EAGAIN）");
            throw new IOException($"VIDIOC_DQBUF 失败: {GetErrorMessage(errno)} (errno={errno})");
        }

        uint index = buf.index;
        _bufferQueued[index] = false;

        if (_currentFormat == null)
            throw new InvalidOperationException("格式信息丢失");

        // 绑定 Dispose 回调：消费者处理完后自动重新入队
        Action<uint> onDispose = OnFrameDisposed;

        if (_mode == V4L2BufferMode.Mmap)
        {
            return new V4L2Frame(
                buf,
                _mode,
                _mmapPtrs[index],
                _mmapLengths[index],
                _currentFormat.Width,
                _currentFormat.Height,
                _currentFormat.PixelFormat,
                onDispose);
        }
        else
        {
            return new V4L2Frame(
                buf,
                _dmaBufHandles![index]!,
                _currentFormat.Width,
                _currentFormat.Height,
                _currentFormat.PixelFormat,
                onDispose);
        }
    }

    #endregion

    #region Pull 拉取模式

    /// <summary>
    /// 异步捕获一帧。基于 poll() 实现真正的异步等待，不阻塞调用线程。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>帧数据，使用后必须 Dispose</returns>
    public Task<V4L2Frame> CaptureFrameAsync(CancellationToken cancellationToken = default)
    {
        if (!_streaming) throw new InvalidOperationException("流式传输未启动，请先调用 StartStream()");
        if (_isCaptureLoopRunning) throw new InvalidOperationException("捕获循环正在运行，请使用 FrameArrived 事件或先 StopCaptureLoop()");

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            while (!PollForFrame(100))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            lock (_syncLock)
            {
                return DequeueFrameForPull();
            }
        }, cancellationToken);
    }

    /// <summary>
    /// 同步尝试捕获一帧（非阻塞）。
    /// </summary>
    /// <param name="frame">输出帧，null 表示无可用数据</param>
    /// <returns>是否成功捕获</returns>
    public bool TryCaptureFrame(out V4L2Frame? frame)
    {
        frame = null;
        if (!_streaming) return false;
        if (_isCaptureLoopRunning) return false;

        if (!PollForFrame(0))
            return false;

        lock (_syncLock)
        {
            frame = DequeueFrameForPull();
        }
        return true;
    }

    /// <summary>
    /// 同步捕获一帧（阻塞直到有数据或超时）。
    /// </summary>
    /// <param name="timeoutMs">超时毫秒数，-1 表示无限等待</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>帧数据，使用后必须 Dispose</returns>
    public V4L2Frame CaptureFrame(int timeoutMs = -1, CancellationToken cancellationToken = default)
    {
        if (!_streaming) throw new InvalidOperationException("流式传输未启动");
        if (_isCaptureLoopRunning) throw new InvalidOperationException("捕获循环正在运行，请使用 FrameArrived 事件或先 StopCaptureLoop()");

        if (!PollForFrame(timeoutMs))
            throw new TimeoutException("捕获帧超时");

        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncLock)
        {
            return DequeueFrameForPull();
        }
    }

    /// <summary>
    /// 出队一帧用于 Pull 模式（立即重新入队）
    /// </summary>
    private unsafe V4L2Frame DequeueFrameForPull()
    {
        var buf = new v4l2_buffer
        {
            type = (uint)_bufType,
            memory = (uint)_mode
        };

        int result = ioctl(_handle.FileDescriptor, VIDIOC_DQBUF, ref buf);
        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            if (errno == EAGAIN)
                throw new TimeoutException("没有可用的帧（EAGAIN）");
            throw new IOException($"VIDIOC_DQBUF 失败: {GetErrorMessage(errno)} (errno={errno})");
        }

        uint index = buf.index;
        _bufferQueued[index] = false;

        if (_currentFormat == null)
            throw new InvalidOperationException("格式信息丢失");

        V4L2Frame frame;
        if (_mode == V4L2BufferMode.Mmap)
        {
            frame = new V4L2Frame(
                buf,
                _mode,
                _mmapPtrs[index],
                _mmapLengths[index],
                _currentFormat.Width,
                _currentFormat.Height,
                _currentFormat.PixelFormat);
        }
        else
        {
            frame = new V4L2Frame(
                buf,
                _dmaBufHandles![index]!,
                _currentFormat.Width,
                _currentFormat.Height,
                _currentFormat.PixelFormat);
        }

        // Pull 模式：立即重新入队（调用者用完即 Dispose，不持有缓冲区）
        EnqueueBufferInternal(index);

        return frame;
    }

    #endregion

    #region 私有工具

    /// <summary>
    /// 使用 poll 检查是否有帧可读
    /// </summary>
    /// <param name="timeoutMs">超时毫秒数，-1 表示无限等待，0 表示立即返回</param>
    private unsafe bool PollForFrame(int timeoutMs)
    {
        var pfd = new c_pollfd
        {
            fd = _handle.FileDescriptor,
            events = POLLIN,
            revents = 0
        };

        int result;
        result = poll(&pfd, new CULong(1), timeoutMs);

        if (result < 0)
        {
            int errno = Marshal.GetLastSystemError();
            if (errno == EINTR) return false;
            throw new IOException($"poll 失败: {GetErrorMessage(errno)} (errno={errno})");
        }

        return result > 0 && (pfd.revents & (POLLIN | POLLERR)) != 0;
    }

    private static string GetErrorMessage(int errno)
    {
        return errno switch
        {
            EPERM => "Operation not permitted",
            ENOENT => "No such file or directory",
            ESRCH => "No such process",
            EINTR => "Interrupted system call",
            EIO => "I/O error",
            ENXIO => "No such device or address",
            E2BIG => "Argument list too long",
            ENOEXEC => "Exec format error",
            EBADF => "Bad file number",
            ECHILD => "No child processes",
            EAGAIN => "Try again / Resource temporarily unavailable",
            ENOMEM => "Out of memory",
            EACCES => "Permission denied",
            EFAULT => "Bad address",
            EBUSY => "Device or resource busy",
            EEXIST => "File exists",
            ENODEV => "No such device",
            ENOTDIR => "Not a directory",
            EISDIR => "Is a directory",
            EINVAL => "Invalid argument",
            ENFILE => "File table overflow",
            EMFILE => "Too many open files",
            ENOSPC => "No space left on device",
            EPIPE => "Broken pipe",
            EDOM => "Math argument out of domain",
            ERANGE => "Math result not representable",
            _ => $"Unknown error (errno={errno})"
        };
    }

    #endregion

    private bool _disposed;

    /// <summary>
    /// 释放所有资源。会自动停止捕获循环和流式传输。
    /// </summary>
    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_disposed) return;
            _disposed = true;

            StopCaptureLoop();
            StopStream();
            ReleaseBuffersInternal();
            _handle?.Dispose();
        }
    }
}

#endregion
