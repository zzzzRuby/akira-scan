using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using static AkiraScan.Camera.Platform.Tizen.Methods;

namespace AkiraScan.Camera.Platform.Tizen;

/// <summary>
/// TBM 错误码异常。
/// </summary>
internal sealed class TbmException : Exception
{
    public int ErrorCode { get; }

    public TbmException(int errorCode)
        : base($"TBM operation failed with error code: {errorCode}")
    {
        ErrorCode = errorCode;
    }

    public TbmException(int errorCode, string message)
        : base($"[TBM Error {errorCode}] {message}")
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// TBM Surface 原生句柄的安全包装。
/// 保证即使在异常或并发场景下，tbm_surface_destroy 也只会被调用一次。
/// </summary>
internal sealed class TbmSurfaceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public TbmSurfaceHandle() : base(true) { }

    /// <summary>
    /// 创建并持有一个新的 TBM Surface 句柄。
    /// </summary>
    public static TbmSurfaceHandle Create(int width, int height, uint format)
    {
        var ptr = tbm_surface_create(width, height, format);
        if (ptr == IntPtr.Zero)
            throw new TbmException(-1, $"tbm_surface_create failed ({width}x{height}, format=0x{format:X8})");

        return new TbmSurfaceHandle { handle = ptr };
    }

    /// <summary>
    /// 包装一个已有的有效句柄（不调用 create）。
    /// 调用方必须确保句柄有效且所有权转移给本实例。
    /// </summary>
    public static TbmSurfaceHandle TakeOwnership(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            throw new ArgumentException("Cannot wrap a null TBM surface handle.", nameof(ptr));

        return new TbmSurfaceHandle { handle = ptr };
    }

    protected override bool ReleaseHandle()
    {
        // SafeHandle 已保证本方法只执行一次，且不会被并发调用
        if (handle != IntPtr.Zero && handle != new IntPtr(-1))
        {
            tbm_surface_destroy(handle);
        }
        return true;
    }
}

/// <summary>
/// TBM 像素格式。
/// </summary>
internal enum TbmFormat : uint
{
    C8 = TBM_FORMAT_C8,
    Rgb332 = TBM_FORMAT_RGB332,
    Bgr233 = TBM_FORMAT_BGR233,
    Xrgb4444 = TBM_FORMAT_XRGB4444,
    Xbgr4444 = TBM_FORMAT_XBGR4444,
    Rgbx4444 = TBM_FORMAT_RGBX4444,
    Bgrx4444 = TBM_FORMAT_BGRX4444,
    Argb4444 = TBM_FORMAT_ARGB4444,
    Abgr4444 = TBM_FORMAT_ABGR4444,
    Rgba4444 = TBM_FORMAT_RGBA4444,
    Bgra4444 = TBM_FORMAT_BGRA4444,
    Xrgb1555 = TBM_FORMAT_XRGB1555,
    Xbgr1555 = TBM_FORMAT_XBGR1555,
    Rgbx5551 = TBM_FORMAT_RGBX5551,
    Bgrx5551 = TBM_FORMAT_BGRX5551,
    Argb1555 = TBM_FORMAT_ARGB1555,
    Abgr1555 = TBM_FORMAT_ABGR1555,
    Rgba5551 = TBM_FORMAT_RGBA5551,
    Bgra5551 = TBM_FORMAT_BGRA5551,
    Rgb565 = TBM_FORMAT_RGB565,
    Bgr565 = TBM_FORMAT_BGR565,
    Rgb888 = TBM_FORMAT_RGB888,
    Bgr888 = TBM_FORMAT_BGR888,
    Xrgb8888 = TBM_FORMAT_XRGB8888,
    Xbgr8888 = TBM_FORMAT_XBGR8888,
    Rgbx8888 = TBM_FORMAT_RGBX8888,
    Bgrx8888 = TBM_FORMAT_BGRX8888,
    Argb8888 = TBM_FORMAT_ARGB8888,
    Abgr8888 = TBM_FORMAT_ABGR8888,
    Rgba8888 = TBM_FORMAT_RGBA8888,
    Bgra8888 = TBM_FORMAT_BGRA8888,
    Xrgb2101010 = TBM_FORMAT_XRGB2101010,
    Xbgr2101010 = TBM_FORMAT_XBGR2101010,
    Rgbx1010102 = TBM_FORMAT_RGBX1010102,
    Bgrx1010102 = TBM_FORMAT_BGRX1010102,
    Argb2101010 = TBM_FORMAT_ARGB2101010,
    Abgr2101010 = TBM_FORMAT_ABGR2101010,
    Rgba1010102 = TBM_FORMAT_RGBA1010102,
    Bgra1010102 = TBM_FORMAT_BGRA1010102,
    Yuyv = TBM_FORMAT_YUYV,
    Yvyu = TBM_FORMAT_YVYU,
    Uyvy = TBM_FORMAT_UYVY,
    Vyuy = TBM_FORMAT_VYUY,
    Ayuv = TBM_FORMAT_AYUV,
    Nv12 = TBM_FORMAT_NV12,
    Nv21 = TBM_FORMAT_NV21,
    Nv16 = TBM_FORMAT_NV16,
    Nv61 = TBM_FORMAT_NV61,
    Yuv410 = TBM_FORMAT_YUV410,
    Yvu410 = TBM_FORMAT_YVU410,
    Yuv411 = TBM_FORMAT_YUV411,
    Yvu411 = TBM_FORMAT_YVU411,
    Yuv420 = TBM_FORMAT_YUV420,
    Yvu420 = TBM_FORMAT_YVU420,
    Yuv422 = TBM_FORMAT_YUV422,
    Yvu422 = TBM_FORMAT_YVU422,
    Yuv444 = TBM_FORMAT_YUV444,
    Yvu444 = TBM_FORMAT_YVU444,
    Nv12mt = TBM_FORMAT_NV12MT,
}

/// <summary>
/// Surface 映射访问模式。
/// </summary>
[Flags]
internal enum TbmSurfaceOptions
{
    None = 0,
    Read = TBM_SURF_OPTION_READ,
    Write = TBM_SURF_OPTION_WRITE,
    ReadWrite = Read | Write,
}

/// <summary>
/// 单个平面信息。
/// </summary>
internal readonly struct TbmSurfacePlane
{
    private readonly _tbm_surface_plane _native;

    internal TbmSurfacePlane(in _tbm_surface_plane native) => _native = native;

    /// <summary>平面数据指针。仅在 Map 期间有效。</summary>
    public nint Pointer
    {
        get
        {
            unsafe
            {
                return (nint)_native.ptr;
            }
        }
    }

    /// <summary>平面数据总大小（字节）。</summary>
    public uint Size => _native.size;

    /// <summary>数据在缓冲区中的偏移量。</summary>
    public uint Offset => _native.offset;

    /// <summary>每行字节数（stride）。</summary>
    public uint Stride => _native.stride;

    /// <summary>
    /// 获取该平面的 Span。仅在 Map 期间且持有有效指针时可用。
    /// </summary>
    public unsafe Span<byte> AsSpan()
    {
        if (_native.ptr == null) return Span<byte>.Empty;
        return new Span<byte>(_native.ptr, (int)_native.size);
    }

    /// <summary>
    /// 获取该平面的只读 Span。仅在 Map 期间且持有有效指针时可用。
    /// </summary>
    public unsafe ReadOnlySpan<byte> AsReadOnlySpan()
    {
        if (_native.ptr == null) return ReadOnlySpan<byte>.Empty;
        return new ReadOnlySpan<byte>(_native.ptr, (int)_native.size);
    }
}

/// <summary>
/// Surface 信息快照。
/// </summary>
internal readonly struct TbmSurfaceInfo
{
    private readonly _tbm_surface_info _native;

    internal TbmSurfaceInfo(in _tbm_surface_info native) => _native = native;

    public uint Width => _native.width;
    public uint Height => _native.height;
    public TbmFormat Format => (TbmFormat)_native.format;
    public uint Bpp => _native.bpp;
    public uint Size => _native.size;
    public uint NumPlanes => _native.num_planes;

    /// <summary>
    /// 获取指定索引的平面信息。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">索引越界。</exception>
    public unsafe TbmSurfacePlane GetPlane(int index)
    {
        if (index < 0 || index >= TBM_SURF_PLANE_MAX)
            throw new ArgumentOutOfRangeException(nameof(index));

        fixed (_tbm_surface_plane* p = &_native.planes.e0)
            return new TbmSurfacePlane(p[index]);
    }

    /// <summary>
    /// 获取所有有效平面信息（长度为 <see cref="NumPlanes"/>）。
    /// </summary>
    public unsafe TbmSurfacePlane[] GetPlanes()
    {
        int count = (int)_native.num_planes;
        var result = new TbmSurfacePlane[count];

        fixed (_tbm_surface_plane* p = &_native.planes.e0)
        {
            for (int i = 0; i < count; i++)
                result[i] = new TbmSurfacePlane(p[i]);
        }
        return result;
    }
}

/// <summary>
/// TBM Surface 托管封装。基于 <see cref="SafeHandle"/> 实现句柄安全，
/// 保证即使在异常、线程终止或 GC 压力场景下，底层资源也只会被释放一次。
/// </summary>
internal sealed class TbmSurface : IDisposable
{
    private readonly TbmSurfaceHandle _handle;

    /// <summary>
    /// 底层 SafeHandle。可用于与其他需要原生句柄的 API 交互。
    /// </summary>
    public SafeHandle SafeHandle => _handle;

    /// <summary>
    /// 原生句柄值。谨慎使用，不要单独释放。
    /// </summary>
    public IntPtr DangerousHandle => _handle.DangerousGetHandle();

    /// <summary>
    /// 句柄是否已无效或释放。
    /// </summary>
    public bool IsClosed => _handle.IsClosed;

    private TbmSurface(TbmSurfaceHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// 创建指定格式的 TBM Surface。
    /// </summary>
    /// <param name="width">宽度（像素）。</param>
    /// <param name="height">高度（像素）。</param>
    /// <param name="format">像素格式。</param>
    /// <exception cref="TbmException">创建失败。</exception>
    public static TbmSurface Create(int width, int height, TbmFormat format)
    {
        var h = TbmSurfaceHandle.Create(width, height, (uint)format);
        return new TbmSurface(h);
    }

    /// <summary>
    /// 查询系统支持的所有像素格式。
    /// 内部会自动释放 TBM 分配的查询结果内存。
    /// </summary>
    public static unsafe TbmFormat[] QueryFormats()
    {
        uint* formats = null;
        uint count = 0;

        int ret = tbm_surface_query_formats(&formats, &count);
        if (ret < 0)
            throw new TbmException(ret, "tbm_surface_query_formats failed");

        try
        {
            var result = new TbmFormat[count];
            for (int i = 0; i < count; i++)
                result[i] = (TbmFormat)formats[i];
            return result;
        }
        finally
        {
            if (formats != null)
                free((IntPtr)formats);
        }
    }

    /// <summary>
    /// 获取 Surface 宽度。
    /// </summary>
    /// <exception cref="TbmException">底层调用返回错误。</exception>
    public int Width
    {
        get
        {
            int w = tbm_surface_get_width(_handle.DangerousGetHandle());
            if (w < 0) ThrowError(w);
            return w;
        }
    }

    /// <summary>
    /// 获取 Surface 高度。
    /// </summary>
    /// <exception cref="TbmException">底层调用返回错误。</exception>
    public int Height
    {
        get
        {
            int h = tbm_surface_get_height(_handle.DangerousGetHandle());
            if (h < 0) ThrowError(h);
            return h;
        }
    }

    /// <summary>
    /// 获取 Surface 像素格式。
    /// </summary>
    public TbmFormat Format => (TbmFormat)tbm_surface_get_format(_handle.DangerousGetHandle());

    /// <summary>
    /// 获取 Surface 信息（无需 Map，只读元数据）。
    /// </summary>
    /// <exception cref="TbmException">底层调用返回错误。</exception>
    public unsafe TbmSurfaceInfo GetInfo()
    {
        _tbm_surface_info info;
        int ret = tbm_surface_get_info(_handle.DangerousGetHandle(), &info);
        if (ret < 0) ThrowError(ret);
        return new TbmSurfaceInfo(info);
    }

    /// <summary>
    /// 映射 Surface 以进行读写。
    /// 返回的 <see cref="TbmSurfaceMapScope"/> 必须被 dispose，否则内存保持锁定。
    /// </summary>
    /// <param name="options">访问模式。</param>
    /// <exception cref="TbmException">底层调用返回错误。</exception>
    public TbmSurfaceMapScope Map(TbmSurfaceOptions options)
    {
        return new TbmSurfaceMapScope(_handle, (int)options);
    }

    /// <summary>
    /// 关闭并释放底层句柄。后续访问将抛出 <see cref="ObjectDisposedException"/>。
    /// </summary>
    public void Dispose()
    {
        _handle.Dispose();
    }

    internal static void ThrowError(int code)
    {
        throw new TbmException(code);
    }
}

/// <summary>
/// Map 作用域。使用 <c>using</c> 语句确保 <c>tbm_surface_unmap</c> 被严格配对调用。
/// 这是一个 <c>ref struct</c>，只能在栈上分配，防止逃逸到堆上导致生命周期不可控。
/// </summary>
internal ref struct TbmSurfaceMapScope
{
    private readonly TbmSurfaceHandle _handle;
    private _tbm_surface_info _info;
    private bool _mapped;

    internal TbmSurfaceMapScope(TbmSurfaceHandle handle, int options)
    {
        _handle = handle;
        _info = default;
        _mapped = false;

        unsafe
        {
            fixed (_tbm_surface_info* info = &_info) {
                int ret = tbm_surface_map(handle.DangerousGetHandle(), options, info);
                if (ret < 0)
                    TbmSurface.ThrowError(ret);
            }
        }

        _mapped = true;
    }

    /// <summary>映射后的 Surface 信息，包含各平面指针。仅在 Map 期间有效。</summary>
    public readonly TbmSurfaceInfo Info => new(_info);

    /// <summary>
    /// 解除映射。可安全调用多次（后续调用无效果）。
    /// </summary>
    public void Dispose()
    {
        if (_mapped)
        {
            tbm_surface_unmap(_handle.DangerousGetHandle());
            _mapped = false;
        }
    }
}