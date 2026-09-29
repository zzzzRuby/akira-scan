using AkiraScan.Camera.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Tizen.Internals.Errors;

namespace AkiraScan.Camera.Platform.Tizen;

internal enum tbm_surface_error_e: int
{
    TBM_SURFACE_ERROR_NONE = ErrorCode.None,
    TBM_SURFACE_ERROR_INVALID_PARAMETER = ErrorCode.InvalidParameter,
    TBM_SURFACE_ERROR_INVALID_OPERATION = ErrorCode.InvalidOperation,
}

internal unsafe partial struct _tbm_surface_plane
{
    [NativeTypeName("unsigned char *")]
    public byte* ptr;

    [NativeTypeName("uint32_t")]
    public uint size;

    [NativeTypeName("uint32_t")]
    public uint offset;

    [NativeTypeName("uint32_t")]
    public uint stride;

    public void* reserved1;

    public void* reserved2;

    public void* reserved3;
}

internal unsafe partial struct _tbm_surface_info
{
    [NativeTypeName("uint32_t")]
    public uint width;

    [NativeTypeName("uint32_t")]
    public uint height;

    [NativeTypeName("tbm_format")]
    public uint format;

    [NativeTypeName("uint32_t")]
    public uint bpp;

    [NativeTypeName("uint32_t")]
    public uint size;

    [NativeTypeName("uint32_t")]
    public uint num_planes;

    [NativeTypeName("tbm_surface_plane_s[4]")]
    public _planes_e__FixedBuffer planes;

    public void* reserved4;

    public void* reserved5;

    public void* reserved6;

    [InlineArray(Methods.TBM_SURF_PLANE_MAX)]
    public partial struct _planes_e__FixedBuffer
    {
        public _tbm_surface_plane e0;
    }
}

internal static unsafe partial class Methods
{
    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_query_formats([NativeTypeName("uint32_t **")] uint** formats, [NativeTypeName("uint32_t *")] uint* num);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: NativeTypeName("tbm_surface_h")]
    public static partial IntPtr tbm_surface_create(int width, int height, [NativeTypeName("tbm_format")] uint format);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_destroy([NativeTypeName("tbm_surface_h")] IntPtr surface);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_map([NativeTypeName("tbm_surface_h")] IntPtr surface, int opt, [NativeTypeName("tbm_surface_info_s *")] _tbm_surface_info* info);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_unmap([NativeTypeName("tbm_surface_h")] IntPtr surface);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_get_info([NativeTypeName("tbm_surface_h")] IntPtr surface, [NativeTypeName("tbm_surface_info_s *")] _tbm_surface_info* info);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_get_width([NativeTypeName("tbm_surface_h")] IntPtr surface);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int tbm_surface_get_height([NativeTypeName("tbm_surface_h")] IntPtr surface);

    [LibraryImport("libtbm.so")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: NativeTypeName("tbm_format")]
    public static partial uint tbm_surface_get_format([NativeTypeName("tbm_surface_h")] IntPtr surface);

    [LibraryImport("libc")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void free(IntPtr memory);

    [NativeTypeName("#define TBM_SURF_PLANE_MAX 4")]
    public const int TBM_SURF_PLANE_MAX = 4;

    [NativeTypeName("#define TBM_SURF_OPTION_READ (1 << 0)")]
    public const int TBM_SURF_OPTION_READ = (1 << 0);

    [NativeTypeName("#define TBM_SURF_OPTION_WRITE (1 << 1)")]
    public const int TBM_SURF_OPTION_WRITE = (1 << 1);

    [NativeTypeName("#define TBM_FORMAT_C8 __tbm_fourcc_code('C', '8', ' ', ' ')")]
    public const uint TBM_FORMAT_C8 = ((uint)('C') | ((uint)('8') << 8) | ((uint)(' ') << 16) | ((uint)(' ') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGB332 __tbm_fourcc_code('R', 'G', 'B', '8')")]
    public const uint TBM_FORMAT_RGB332 = ((uint)('R') | ((uint)('G') << 8) | ((uint)('B') << 16) | ((uint)('8') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGR233 __tbm_fourcc_code('B', 'G', 'R', '8')")]
    public const uint TBM_FORMAT_BGR233 = ((uint)('B') | ((uint)('G') << 8) | ((uint)('R') << 16) | ((uint)('8') << 24));

    [NativeTypeName("#define TBM_FORMAT_XRGB4444 __tbm_fourcc_code('X', 'R', '1', '2')")]
    public const uint TBM_FORMAT_XRGB4444 = ((uint)('X') | ((uint)('R') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_XBGR4444 __tbm_fourcc_code('X', 'B', '1', '2')")]
    public const uint TBM_FORMAT_XBGR4444 = ((uint)('X') | ((uint)('B') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBX4444 __tbm_fourcc_code('R', 'X', '1', '2')")]
    public const uint TBM_FORMAT_RGBX4444 = ((uint)('R') | ((uint)('X') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRX4444 __tbm_fourcc_code('B', 'X', '1', '2')")]
    public const uint TBM_FORMAT_BGRX4444 = ((uint)('B') | ((uint)('X') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_ARGB4444 __tbm_fourcc_code('A', 'R', '1', '2')")]
    public const uint TBM_FORMAT_ARGB4444 = ((uint)('A') | ((uint)('R') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_ABGR4444 __tbm_fourcc_code('A', 'B', '1', '2')")]
    public const uint TBM_FORMAT_ABGR4444 = ((uint)('A') | ((uint)('B') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBA4444 __tbm_fourcc_code('R', 'A', '1', '2')")]
    public const uint TBM_FORMAT_RGBA4444 = ((uint)('R') | ((uint)('A') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRA4444 __tbm_fourcc_code('B', 'A', '1', '2')")]
    public const uint TBM_FORMAT_BGRA4444 = ((uint)('B') | ((uint)('A') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_XRGB1555 __tbm_fourcc_code('X', 'R', '1', '5')")]
    public const uint TBM_FORMAT_XRGB1555 = ((uint)('X') | ((uint)('R') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_XBGR1555 __tbm_fourcc_code('X', 'B', '1', '5')")]
    public const uint TBM_FORMAT_XBGR1555 = ((uint)('X') | ((uint)('B') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBX5551 __tbm_fourcc_code('R', 'X', '1', '5')")]
    public const uint TBM_FORMAT_RGBX5551 = ((uint)('R') | ((uint)('X') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRX5551 __tbm_fourcc_code('B', 'X', '1', '5')")]
    public const uint TBM_FORMAT_BGRX5551 = ((uint)('B') | ((uint)('X') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_ARGB1555 __tbm_fourcc_code('A', 'R', '1', '5')")]
    public const uint TBM_FORMAT_ARGB1555 = ((uint)('A') | ((uint)('R') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_ABGR1555 __tbm_fourcc_code('A', 'B', '1', '5')")]
    public const uint TBM_FORMAT_ABGR1555 = ((uint)('A') | ((uint)('B') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBA5551 __tbm_fourcc_code('R', 'A', '1', '5')")]
    public const uint TBM_FORMAT_RGBA5551 = ((uint)('R') | ((uint)('A') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRA5551 __tbm_fourcc_code('B', 'A', '1', '5')")]
    public const uint TBM_FORMAT_BGRA5551 = ((uint)('B') | ((uint)('A') << 8) | ((uint)('1') << 16) | ((uint)('5') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGB565 __tbm_fourcc_code('R', 'G', '1', '6')")]
    public const uint TBM_FORMAT_RGB565 = ((uint)('R') | ((uint)('G') << 8) | ((uint)('1') << 16) | ((uint)('6') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGR565 __tbm_fourcc_code('B', 'G', '1', '6')")]
    public const uint TBM_FORMAT_BGR565 = ((uint)('B') | ((uint)('G') << 8) | ((uint)('1') << 16) | ((uint)('6') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGB888 __tbm_fourcc_code('R', 'G', '2', '4')")]
    public const uint TBM_FORMAT_RGB888 = ((uint)('R') | ((uint)('G') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGR888 __tbm_fourcc_code('B', 'G', '2', '4')")]
    public const uint TBM_FORMAT_BGR888 = ((uint)('B') | ((uint)('G') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_XRGB8888 __tbm_fourcc_code('X', 'R', '2', '4')")]
    public const uint TBM_FORMAT_XRGB8888 = ((uint)('X') | ((uint)('R') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_XBGR8888 __tbm_fourcc_code('X', 'B', '2', '4')")]
    public const uint TBM_FORMAT_XBGR8888 = ((uint)('X') | ((uint)('B') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBX8888 __tbm_fourcc_code('R', 'X', '2', '4')")]
    public const uint TBM_FORMAT_RGBX8888 = ((uint)('R') | ((uint)('X') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRX8888 __tbm_fourcc_code('B', 'X', '2', '4')")]
    public const uint TBM_FORMAT_BGRX8888 = ((uint)('B') | ((uint)('X') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_ARGB8888 __tbm_fourcc_code('A', 'R', '2', '4')")]
    public const uint TBM_FORMAT_ARGB8888 = ((uint)('A') | ((uint)('R') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_ABGR8888 __tbm_fourcc_code('A', 'B', '2', '4')")]
    public const uint TBM_FORMAT_ABGR8888 = ((uint)('A') | ((uint)('B') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBA8888 __tbm_fourcc_code('R', 'A', '2', '4')")]
    public const uint TBM_FORMAT_RGBA8888 = ((uint)('R') | ((uint)('A') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRA8888 __tbm_fourcc_code('B', 'A', '2', '4')")]
    public const uint TBM_FORMAT_BGRA8888 = ((uint)('B') | ((uint)('A') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_XRGB2101010 __tbm_fourcc_code('X', 'R', '3', '0')")]
    public const uint TBM_FORMAT_XRGB2101010 = ((uint)('X') | ((uint)('R') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_XBGR2101010 __tbm_fourcc_code('X', 'B', '3', '0')")]
    public const uint TBM_FORMAT_XBGR2101010 = ((uint)('X') | ((uint)('B') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBX1010102 __tbm_fourcc_code('R', 'X', '3', '0')")]
    public const uint TBM_FORMAT_RGBX1010102 = ((uint)('R') | ((uint)('X') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRX1010102 __tbm_fourcc_code('B', 'X', '3', '0')")]
    public const uint TBM_FORMAT_BGRX1010102 = ((uint)('B') | ((uint)('X') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_ARGB2101010 __tbm_fourcc_code('A', 'R', '3', '0')")]
    public const uint TBM_FORMAT_ARGB2101010 = ((uint)('A') | ((uint)('R') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_ABGR2101010 __tbm_fourcc_code('A', 'B', '3', '0')")]
    public const uint TBM_FORMAT_ABGR2101010 = ((uint)('A') | ((uint)('B') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_RGBA1010102 __tbm_fourcc_code('R', 'A', '3', '0')")]
    public const uint TBM_FORMAT_RGBA1010102 = ((uint)('R') | ((uint)('A') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_BGRA1010102 __tbm_fourcc_code('B', 'A', '3', '0')")]
    public const uint TBM_FORMAT_BGRA1010102 = ((uint)('B') | ((uint)('A') << 8) | ((uint)('3') << 16) | ((uint)('0') << 24));

    [NativeTypeName("#define TBM_FORMAT_YUYV __tbm_fourcc_code('Y', 'U', 'Y', 'V')")]
    public const uint TBM_FORMAT_YUYV = ((uint)('Y') | ((uint)('U') << 8) | ((uint)('Y') << 16) | ((uint)('V') << 24));

    [NativeTypeName("#define TBM_FORMAT_YVYU __tbm_fourcc_code('Y', 'V', 'Y', 'U')")]
    public const uint TBM_FORMAT_YVYU = ((uint)('Y') | ((uint)('V') << 8) | ((uint)('Y') << 16) | ((uint)('U') << 24));

    [NativeTypeName("#define TBM_FORMAT_UYVY __tbm_fourcc_code('U', 'Y', 'V', 'Y')")]
    public const uint TBM_FORMAT_UYVY = ((uint)('U') | ((uint)('Y') << 8) | ((uint)('V') << 16) | ((uint)('Y') << 24));

    [NativeTypeName("#define TBM_FORMAT_VYUY __tbm_fourcc_code('V', 'Y', 'U', 'Y')")]
    public const uint TBM_FORMAT_VYUY = ((uint)('V') | ((uint)('Y') << 8) | ((uint)('U') << 16) | ((uint)('Y') << 24));

    [NativeTypeName("#define TBM_FORMAT_AYUV __tbm_fourcc_code('A', 'Y', 'U', 'V')")]
    public const uint TBM_FORMAT_AYUV = ((uint)('A') | ((uint)('Y') << 8) | ((uint)('U') << 16) | ((uint)('V') << 24));

    [NativeTypeName("#define TBM_FORMAT_NV12 __tbm_fourcc_code('N', 'V', '1', '2')")]
    public const uint TBM_FORMAT_NV12 = ((uint)('N') | ((uint)('V') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_NV21 __tbm_fourcc_code('N', 'V', '2', '1')")]
    public const uint TBM_FORMAT_NV21 = ((uint)('N') | ((uint)('V') << 8) | ((uint)('2') << 16) | ((uint)('1') << 24));

    [NativeTypeName("#define TBM_FORMAT_NV16 __tbm_fourcc_code('N', 'V', '1', '6')")]
    public const uint TBM_FORMAT_NV16 = ((uint)('N') | ((uint)('V') << 8) | ((uint)('1') << 16) | ((uint)('6') << 24));

    [NativeTypeName("#define TBM_FORMAT_NV61 __tbm_fourcc_code('N', 'V', '6', '1')")]
    public const uint TBM_FORMAT_NV61 = ((uint)('N') | ((uint)('V') << 8) | ((uint)('6') << 16) | ((uint)('1') << 24));

    [NativeTypeName("#define TBM_FORMAT_YUV410 __tbm_fourcc_code('Y', 'U', 'V', '9')")]
    public const uint TBM_FORMAT_YUV410 = ((uint)('Y') | ((uint)('U') << 8) | ((uint)('V') << 16) | ((uint)('9') << 24));

    [NativeTypeName("#define TBM_FORMAT_YVU410 __tbm_fourcc_code('Y', 'V', 'U', '9')")]
    public const uint TBM_FORMAT_YVU410 = ((uint)('Y') | ((uint)('V') << 8) | ((uint)('U') << 16) | ((uint)('9') << 24));

    [NativeTypeName("#define TBM_FORMAT_YUV411 __tbm_fourcc_code('Y', 'U', '1', '1')")]
    public const uint TBM_FORMAT_YUV411 = ((uint)('Y') | ((uint)('U') << 8) | ((uint)('1') << 16) | ((uint)('1') << 24));

    [NativeTypeName("#define TBM_FORMAT_YVU411 __tbm_fourcc_code('Y', 'V', '1', '1')")]
    public const uint TBM_FORMAT_YVU411 = ((uint)('Y') | ((uint)('V') << 8) | ((uint)('1') << 16) | ((uint)('1') << 24));

    [NativeTypeName("#define TBM_FORMAT_YUV420 __tbm_fourcc_code('Y', 'U', '1', '2')")]
    public const uint TBM_FORMAT_YUV420 = ((uint)('Y') | ((uint)('U') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_YVU420 __tbm_fourcc_code('Y', 'V', '1', '2')")]
    public const uint TBM_FORMAT_YVU420 = ((uint)('Y') | ((uint)('V') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TBM_FORMAT_YUV422 __tbm_fourcc_code('Y', 'U', '1', '6')")]
    public const uint TBM_FORMAT_YUV422 = ((uint)('Y') | ((uint)('U') << 8) | ((uint)('1') << 16) | ((uint)('6') << 24));

    [NativeTypeName("#define TBM_FORMAT_YVU422 __tbm_fourcc_code('Y', 'V', '1', '6')")]
    public const uint TBM_FORMAT_YVU422 = ((uint)('Y') | ((uint)('V') << 8) | ((uint)('1') << 16) | ((uint)('6') << 24));

    [NativeTypeName("#define TBM_FORMAT_YUV444 __tbm_fourcc_code('Y', 'U', '2', '4')")]
    public const uint TBM_FORMAT_YUV444 = ((uint)('Y') | ((uint)('U') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_YVU444 __tbm_fourcc_code('Y', 'V', '2', '4')")]
    public const uint TBM_FORMAT_YVU444 = ((uint)('Y') | ((uint)('V') << 8) | ((uint)('2') << 16) | ((uint)('4') << 24));

    [NativeTypeName("#define TBM_FORMAT_NV12MT __tbm_fourcc_code('T', 'M', '1', '2')")]
    public const uint TBM_FORMAT_NV12MT = ((uint)('T') | ((uint)('M') << 8) | ((uint)('1') << 16) | ((uint)('2') << 24));

    [NativeTypeName("#define TIZEN_ERROR_MAX_PLATFORM_ERROR 0")]
    public const int TIZEN_ERROR_MAX_PLATFORM_ERROR = 0;

    [NativeTypeName("#define TIZEN_ERROR_MIN_PLATFORM_ERROR (-1073741824LL)")]
    public const long TIZEN_ERROR_MIN_PLATFORM_ERROR = (-1073741824L);

    [NativeTypeName("#define TIZEN_ERROR_MIN_MODULE_ERROR (-2147483648LL)")]
    public const long TIZEN_ERROR_MIN_MODULE_ERROR = (-2147483648L);

    [NativeTypeName("#define TIZEN_ERROR_MAX_MODULE_ERROR (TIZEN_ERROR_MIN_PLATFORM_ERROR - 1)")]
    public const long TIZEN_ERROR_MAX_MODULE_ERROR = ((-1073741824L) - 1);

    [NativeTypeName("#define TIZEN_ERROR_APPLICATION_CLASS -0x00000200")]
    public const int TIZEN_ERROR_APPLICATION_CLASS = -0x00000200;

    [NativeTypeName("#define TIZEN_ERROR_BASE_CLASS -0x00000400")]
    public const int TIZEN_ERROR_BASE_CLASS = -0x00000400;

    [NativeTypeName("#define TIZEN_ERROR_CONTENT_CLASS -0x00000800")]
    public const int TIZEN_ERROR_CONTENT_CLASS = -0x00000800;

    [NativeTypeName("#define TIZEN_ERROR_LOCATION_CLASS -0x00001000")]
    public const int TIZEN_ERROR_LOCATION_CLASS = -0x00001000;

    [NativeTypeName("#define TIZEN_ERROR_MULTIMEDIA_CLASS -0x00002000")]
    public const int TIZEN_ERROR_MULTIMEDIA_CLASS = -0x00002000;

    [NativeTypeName("#define TIZEN_ERROR_MESSAGING_CLASS -0x00004000")]
    public const int TIZEN_ERROR_MESSAGING_CLASS = -0x00004000;

    [NativeTypeName("#define TIZEN_ERROR_NETWORK_CLASS -0x00008000")]
    public const int TIZEN_ERROR_NETWORK_CLASS = -0x00008000;

    [NativeTypeName("#define TIZEN_ERROR_SOCIAL_CLASS -0x00010000")]
    public const int TIZEN_ERROR_SOCIAL_CLASS = -0x00010000;

    [NativeTypeName("#define TIZEN_ERROR_SYSTEM_CLASS -0x00020000")]
    public const int TIZEN_ERROR_SYSTEM_CLASS = -0x00020000;

    [NativeTypeName("#define TIZEN_ERROR_TELEPHONY_CLASS -0x00040000")]
    public const int TIZEN_ERROR_TELEPHONY_CLASS = -0x00040000;

    [NativeTypeName("#define TIZEN_ERROR_UI_CLASS -0x00080000")]
    public const int TIZEN_ERROR_UI_CLASS = -0x00080000;

    [NativeTypeName("#define TIZEN_ERROR_UIX_CLASS -0x00100000")]
    public const int TIZEN_ERROR_UIX_CLASS = -0x00100000;

    [NativeTypeName("#define TIZEN_ERROR_AD_CLASS -0x00200000")]
    public const int TIZEN_ERROR_AD_CLASS = -0x00200000;

    [NativeTypeName("#define TIZEN_ERROR_WEB_CLASS -0x00400000")]
    public const int TIZEN_ERROR_WEB_CLASS = -0x00400000;

    [NativeTypeName("#define TIZEN_ERROR_DRM_CLASS -0x00800000")]
    public const int TIZEN_ERROR_DRM_CLASS = -0x00800000;

    [NativeTypeName("#define TIZEN_ERROR_ACCOUNT_CLASS -0x01000000")]
    public const int TIZEN_ERROR_ACCOUNT_CLASS = -0x01000000;

    [NativeTypeName("#define TIZEN_ERROR_ACCOUNT -0x01000000")]
    public const int TIZEN_ERROR_ACCOUNT = -0x01000000;

    [NativeTypeName("#define TIZEN_ERROR_ACCOUNT_OAUTH -0x01010000")]
    public const int TIZEN_ERROR_ACCOUNT_OAUTH = -0x01010000;

    [NativeTypeName("#define TIZEN_ERROR_SYNC_MANAGER -0x01020000")]
    public const int TIZEN_ERROR_SYNC_MANAGER = -0x01020000;

    [NativeTypeName("#define TIZEN_ERROR_FIDO -0x01030000")]
    public const int TIZEN_ERROR_FIDO = -0x01030000;

    [NativeTypeName("#define TIZEN_ERROR_APPLICATION -0x01100000")]
    public const int TIZEN_ERROR_APPLICATION = -0x01100000;

    [NativeTypeName("#define TIZEN_ERROR_APPLICATION_MANAGER -0x01110000")]
    public const int TIZEN_ERROR_APPLICATION_MANAGER = -0x01110000;

    [NativeTypeName("#define TIZEN_ERROR_BADGE -0x01120000")]
    public const int TIZEN_ERROR_BADGE = -0x01120000;

    [NativeTypeName("#define TIZEN_ERROR_MESSAGE_PORT -0x01130000")]
    public const int TIZEN_ERROR_MESSAGE_PORT = -0x01130000;

    [NativeTypeName("#define TIZEN_ERROR_NOTIFICATION -0x01140000")]
    public const int TIZEN_ERROR_NOTIFICATION = -0x01140000;

    [NativeTypeName("#define TIZEN_ERROR_PACKAGE_MANAGER -0x01150000")]
    public const int TIZEN_ERROR_PACKAGE_MANAGER = -0x01150000;

    [NativeTypeName("#define TIZEN_ERROR_SHORTCUT -0x01160000")]
    public const int TIZEN_ERROR_SHORTCUT = -0x01160000;

    [NativeTypeName("#define TIZEN_ERROR_BUNDLE -0x01180000")]
    public const int TIZEN_ERROR_BUNDLE = -0x01180000;

    [NativeTypeName("#define TIZEN_ERROR_DATA_CONTROL -0x01190000")]
    public const int TIZEN_ERROR_DATA_CONTROL = -0x01190000;

    [NativeTypeName("#define TIZEN_ERROR_SERVICE_APPLICATION -0x01200000")]
    public const int TIZEN_ERROR_SERVICE_APPLICATION = -0x01200000;

    [NativeTypeName("#define TIZEN_ERROR_MINICONTROL -0x01210000")]
    public const int TIZEN_ERROR_MINICONTROL = -0x01210000;

    [NativeTypeName("#define TIZEN_ERROR_UTILITY_ICU -0x01500000")]
    public const int TIZEN_ERROR_UTILITY_ICU = -0x01500000;

    [NativeTypeName("#define TIZEN_ERROR_MIME_TYPE -0x01600000")]
    public const int TIZEN_ERROR_MIME_TYPE = -0x01600000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_CONTENT -0x01610000")]
    public const int TIZEN_ERROR_MEDIA_CONTENT = -0x01610000;

    [NativeTypeName("#define TIZEN_ERROR_EMAIL_SERVICE -0x01700000")]
    public const int TIZEN_ERROR_EMAIL_SERVICE = -0x01700000;

    [NativeTypeName("#define TIZEN_ERROR_MESSAGING_SERVICE -0x01710000")]
    public const int TIZEN_ERROR_MESSAGING_SERVICE = -0x01710000;

    [NativeTypeName("#define TIZEN_ERROR_PUSH -0x01720000")]
    public const int TIZEN_ERROR_PUSH = -0x01720000;

    [NativeTypeName("#define TIZEN_ERROR_AUDIO_IO -0x01900000")]
    public const int TIZEN_ERROR_AUDIO_IO = -0x01900000;

    [NativeTypeName("#define TIZEN_ERROR_CAMERA -0x01910000")]
    public const int TIZEN_ERROR_CAMERA = -0x01910000;

    [NativeTypeName("#define TIZEN_ERROR_IMAGE_UTIL -0x01920000")]
    public const int TIZEN_ERROR_IMAGE_UTIL = -0x01920000;

    [NativeTypeName("#define TIZEN_ERROR_METADATA_EXTRACTOR -0x01930000")]
    public const int TIZEN_ERROR_METADATA_EXTRACTOR = -0x01930000;

    [NativeTypeName("#define TIZEN_ERROR_PLAYER -0x01940000")]
    public const int TIZEN_ERROR_PLAYER = -0x01940000;

    [NativeTypeName("#define TIZEN_ERROR_RECORDER -0x01950000")]
    public const int TIZEN_ERROR_RECORDER = -0x01950000;

    [NativeTypeName("#define TIZEN_ERROR_SOUND_MANAGER -0x01960000")]
    public const int TIZEN_ERROR_SOUND_MANAGER = -0x01960000;

    [NativeTypeName("#define TIZEN_ERROR_TONE_PLAYER -0x01970000")]
    public const int TIZEN_ERROR_TONE_PLAYER = -0x01970000;

    [NativeTypeName("#define TIZEN_ERROR_VIDEO_UTIL -0x01980000")]
    public const int TIZEN_ERROR_VIDEO_UTIL = -0x01980000;

    [NativeTypeName("#define TIZEN_ERROR_WAV_PLAYER -0x01990000")]
    public const int TIZEN_ERROR_WAV_PLAYER = -0x01990000;

    [NativeTypeName("#define TIZEN_ERROR_RADIO -0x019A0000")]
    public const int TIZEN_ERROR_RADIO = -0x019A0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIACODEC -0x019B0000")]
    public const int TIZEN_ERROR_MEDIACODEC = -0x019B0000;

    [NativeTypeName("#define TIZEN_ERROR_METADATA_EDITOR -0x019C0000")]
    public const int TIZEN_ERROR_METADATA_EDITOR = -0x019C0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_VISION -0x019D0000")]
    public const int TIZEN_ERROR_MEDIA_VISION = -0x019D0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_DEMUXER -0x019E0000")]
    public const int TIZEN_ERROR_MEDIA_DEMUXER = -0x019E0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_MUXER -0x019F0000")]
    public const int TIZEN_ERROR_MEDIA_MUXER = -0x019F0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_STREAMER -0x01A00000")]
    public const int TIZEN_ERROR_MEDIA_STREAMER = -0x01A00000;

    [NativeTypeName("#define TIZEN_ERROR_STREAMRECORDER -0x01A10000")]
    public const int TIZEN_ERROR_STREAMRECORDER = -0x01A10000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_TOOL -0x01A20000")]
    public const int TIZEN_ERROR_MEDIA_TOOL = -0x01A20000;

    [NativeTypeName("#define TIZEN_ERROR_BLUETOOTH -0x01C00000")]
    public const int TIZEN_ERROR_BLUETOOTH = -0x01C00000;

    [NativeTypeName("#define TIZEN_ERROR_CONNECTION -0x01C10000")]
    public const int TIZEN_ERROR_CONNECTION = -0x01C10000;

    [NativeTypeName("#define TIZEN_ERROR_NFC -0x01C20000")]
    public const int TIZEN_ERROR_NFC = -0x01C20000;

    [NativeTypeName("#define TIZEN_ERROR_SERIAL -0x01C30000")]
    public const int TIZEN_ERROR_SERIAL = -0x01C30000;

    [NativeTypeName("#define TIZEN_ERROR_TETHERING -0x01C40000")]
    public const int TIZEN_ERROR_TETHERING = -0x01C40000;

    [NativeTypeName("#define TIZEN_ERROR_WIFI -0x01C50000")]
    public const int TIZEN_ERROR_WIFI = -0x01C50000;

    [NativeTypeName("#define TIZEN_ERROR_WIFI_DIRECT -0x01C60000")]
    public const int TIZEN_ERROR_WIFI_DIRECT = -0x01C60000;

    [NativeTypeName("#define TIZEN_ERROR_SMARTCARD -0x01C70000")]
    public const int TIZEN_ERROR_SMARTCARD = -0x01C70000;

    [NativeTypeName("#define TIZEN_ERROR_IOTCON -0x01C80000")]
    public const int TIZEN_ERROR_IOTCON = -0x01C80000;

    [NativeTypeName("#define TIZEN_ERROR_SSDP -0x01C90000")]
    public const int TIZEN_ERROR_SSDP = -0x01C90000;

    [NativeTypeName("#define TIZEN_ERROR_DNSSD -0x01CA0000")]
    public const int TIZEN_ERROR_DNSSD = -0x01CA0000;

    [NativeTypeName("#define TIZEN_ERROR_VPNSVC -0x01CB0000")]
    public const int TIZEN_ERROR_VPNSVC = -0x01CB0000;

    [NativeTypeName("#define TIZEN_ERROR_MTP -0x01CC0000")]
    public const int TIZEN_ERROR_MTP = -0x01CC0000;

    [NativeTypeName("#define TIZEN_ERROR_HTTP -0x01CD0000")]
    public const int TIZEN_ERROR_HTTP = -0x01CD0000;

    [NativeTypeName("#define TIZEN_ERROR_WIFI_MANAGER -0x01CE0000")]
    public const int TIZEN_ERROR_WIFI_MANAGER = -0x01CE0000;

    [NativeTypeName("#define TIZEN_ERROR_PRIVACY_MANAGER -0x01E00000")]
    public const int TIZEN_ERROR_PRIVACY_MANAGER = -0x01E00000;

    [NativeTypeName("#define TIZEN_ERROR_KEY_MANAGER -0x01E10000")]
    public const int TIZEN_ERROR_KEY_MANAGER = -0x01E10000;

    [NativeTypeName("#define TIZEN_ERROR_PRIVILEGE_INFORMATION -0x01E20000")]
    public const int TIZEN_ERROR_PRIVILEGE_INFORMATION = -0x01E20000;

    [NativeTypeName("#define TIZEN_ERROR_CSR -0x01E30000")]
    public const int TIZEN_ERROR_CSR = -0x01E30000;

    [NativeTypeName("#define TIZEN_ERROR_CALENDAR -0x02000000")]
    public const int TIZEN_ERROR_CALENDAR = -0x02000000;

    [NativeTypeName("#define TIZEN_ERROR_CONTACTS -0x02010000")]
    public const int TIZEN_ERROR_CONTACTS = -0x02010000;

    [NativeTypeName("#define TIZEN_ERROR_PHONENUMBER_UTILS -0x02020000")]
    public const int TIZEN_ERROR_PHONENUMBER_UTILS = -0x02020000;

    [NativeTypeName("#define TIZEN_ERROR_STORAGE -0x02200000")]
    public const int TIZEN_ERROR_STORAGE = -0x02200000;

    [NativeTypeName("#define TIZEN_ERROR_VCONF -0x02210000")]
    public const int TIZEN_ERROR_VCONF = -0x02210000;

    [NativeTypeName("#define TIZEN_ERROR_DEVICE -0x02400000")]
    public const int TIZEN_ERROR_DEVICE = -0x02400000;

    [NativeTypeName("#define TIZEN_ERROR_DLOG -0x02410000")]
    public const int TIZEN_ERROR_DLOG = -0x02410000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_KEY -0x02420000")]
    public const int TIZEN_ERROR_MEDIA_KEY = -0x02420000;

    [NativeTypeName("#define TIZEN_ERROR_RUNTIME_INFORMATION -0x02430000")]
    public const int TIZEN_ERROR_RUNTIME_INFORMATION = -0x02430000;

    [NativeTypeName("#define TIZEN_ERROR_SENSOR -0x02440000")]
    public const int TIZEN_ERROR_SENSOR = -0x02440000;

    [NativeTypeName("#define TIZEN_ERROR_SYSTEM_INFORMATION -0x02450000")]
    public const int TIZEN_ERROR_SYSTEM_INFORMATION = -0x02450000;

    [NativeTypeName("#define TIZEN_ERROR_SYSTEM_SETTING -0x02460000")]
    public const int TIZEN_ERROR_SYSTEM_SETTING = -0x02460000;

    [NativeTypeName("#define TIZEN_ERROR_SYSTEM_RESOURCE -0x02470000")]
    public const int TIZEN_ERROR_SYSTEM_RESOURCE = -0x02470000;

    [NativeTypeName("#define TIZEN_ERROR_CONTEXT -0x02480000")]
    public const int TIZEN_ERROR_CONTEXT = -0x02480000;

    [NativeTypeName("#define TIZEN_ERROR_HEALTH -0x02490000")]
    public const int TIZEN_ERROR_HEALTH = -0x02490000;

    [NativeTypeName("#define TIZEN_ERROR_FEEDBACK -0x024A0000")]
    public const int TIZEN_ERROR_FEEDBACK = -0x024A0000;

    [NativeTypeName("#define TIZEN_ERROR_USB_HOST -0x024B0000")]
    public const int TIZEN_ERROR_USB_HOST = -0x024B0000;

    [NativeTypeName("#define TIZEN_ERROR_TELEPHONY -0x02600000")]
    public const int TIZEN_ERROR_TELEPHONY = -0x02600000;

    [NativeTypeName("#define TIZEN_ERROR_EFL_UTIL -0x02800000")]
    public const int TIZEN_ERROR_EFL_UTIL = -0x02800000;

    [NativeTypeName("#define TIZEN_ERROR_UTILX -0x02810000")]
    public const int TIZEN_ERROR_UTILX = -0x02810000;

    [NativeTypeName("#define TIZEN_ERROR_NATIVE_BUFFER -0x02820000")]
    public const int TIZEN_ERROR_NATIVE_BUFFER = -0x02820000;

    [NativeTypeName("#define TIZEN_ERROR_TBM -0x02830000")]
    public const int TIZEN_ERROR_TBM = -0x02830000;

    [NativeTypeName("#define TIZEN_ERROR_EOM -0x02840000")]
    public const int TIZEN_ERROR_EOM = -0x02840000;

    [NativeTypeName("#define TIZEN_ERROR_PANEL -0x02850000")]
    public const int TIZEN_ERROR_PANEL = -0x02850000;

    [NativeTypeName("#define TIZEN_ERROR_TZSH -0x02860000")]
    public const int TIZEN_ERROR_TZSH = -0x02860000;

    [NativeTypeName("#define TIZEN_ERROR_DOWNLOAD -0x02A00000")]
    public const int TIZEN_ERROR_DOWNLOAD = -0x02A00000;

    [NativeTypeName("#define TIZEN_ERROR_WEBVIEW -0x02A10000")]
    public const int TIZEN_ERROR_WEBVIEW = -0x02A10000;

    [NativeTypeName("#define TIZEN_ERROR_LOCATION_MANAGER -0x02C00000")]
    public const int TIZEN_ERROR_LOCATION_MANAGER = -0x02C00000;

    [NativeTypeName("#define TIZEN_ERROR_GEOFENCE_MANAGER -0x02C10000")]
    public const int TIZEN_ERROR_GEOFENCE_MANAGER = -0x02C10000;

    [NativeTypeName("#define TIZEN_ERROR_MAPS_SERVICE -0x02C20000")]
    public const int TIZEN_ERROR_MAPS_SERVICE = -0x02C20000;

    [NativeTypeName("#define TIZEN_ERROR_STT -0x02F00000")]
    public const int TIZEN_ERROR_STT = -0x02F00000;

    [NativeTypeName("#define TIZEN_ERROR_TTS -0x02F10000")]
    public const int TIZEN_ERROR_TTS = -0x02F10000;

    [NativeTypeName("#define TIZEN_ERROR_IME -0x02F20000")]
    public const int TIZEN_ERROR_IME = -0x02F20000;

    [NativeTypeName("#define TIZEN_ERROR_SERVICE_ADAPTOR -0x02F30000")]
    public const int TIZEN_ERROR_SERVICE_ADAPTOR = -0x02F30000;

    [NativeTypeName("#define TIZEN_ERROR_WIDGET -0x02F40000")]
    public const int TIZEN_ERROR_WIDGET = -0x02F40000;

    [NativeTypeName("#define TIZEN_ERROR_VOICE_CONTROL -0x02F50000")]
    public const int TIZEN_ERROR_VOICE_CONTROL = -0x02F50000;

    [NativeTypeName("#define TIZEN_ERROR_ASP -0x02F60000")]
    public const int TIZEN_ERROR_ASP = -0x02F60000;

    [NativeTypeName("#define TIZEN_ERROR_ZIGBEE -0x02F70000")]
    public const int TIZEN_ERROR_ZIGBEE = -0x02F70000;

    [NativeTypeName("#define TIZEN_ERROR_STC -0x02F80000")]
    public const int TIZEN_ERROR_STC = -0x02F80000;

    [NativeTypeName("#define TIZEN_ERROR_THUMBNAIL_UTIL -0x02F90000")]
    public const int TIZEN_ERROR_THUMBNAIL_UTIL = -0x02F90000;

    [NativeTypeName("#define TIZEN_ERROR_JOB_SCHEDULER -0x02FA0000")]
    public const int TIZEN_ERROR_JOB_SCHEDULER = -0x02FA0000;

    [NativeTypeName("#define TIZEN_ERROR_SOFTAP -0x02FB0000")]
    public const int TIZEN_ERROR_SOFTAP = -0x02FB0000;

    [NativeTypeName("#define TIZEN_ERROR_COMPLICATION -0x02FC0000")]
    public const int TIZEN_ERROR_COMPLICATION = -0x02FC0000;

    [NativeTypeName("#define TIZEN_ERROR_DEVICE_CERTIFICATE_MANAGER -0x02FD0000")]
    public const int TIZEN_ERROR_DEVICE_CERTIFICATE_MANAGER = -0x02FD0000;

    [NativeTypeName("#define TIZEN_ERROR_INM -0x02FE0000")]
    public const int TIZEN_ERROR_INM = -0x02FE0000;

    [NativeTypeName("#define TIZEN_ERROR_UPDATE_CONTROL -0x02FF0000")]
    public const int TIZEN_ERROR_UPDATE_CONTROL = -0x02FF0000;

    [NativeTypeName("#define TIZEN_ERROR_MULTI_ASSISTANT -0x03000000")]
    public const int TIZEN_ERROR_MULTI_ASSISTANT = -0x03000000;

    [NativeTypeName("#define TIZEN_ERROR_AUTOFILL -0x03010000")]
    public const int TIZEN_ERROR_AUTOFILL = -0x03010000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_CONTROLLER -0x03020000")]
    public const int TIZEN_ERROR_MEDIA_CONTROLLER = -0x03020000;

    [NativeTypeName("#define TIZEN_ERROR_COMPONENT -0x03030000")]
    public const int TIZEN_ERROR_COMPONENT = -0x03030000;

    [NativeTypeName("#define TIZEN_ERROR_COMPONENT_MANAGER -0x03040000")]
    public const int TIZEN_ERROR_COMPONENT_MANAGER = -0x03040000;

    [NativeTypeName("#define TIZEN_ERROR_STICKER -0x03050000")]
    public const int TIZEN_ERROR_STICKER = -0x03050000;

    [NativeTypeName("#define TIZEN_ERROR_BATTERY_MONITOR -0x03060000")]
    public const int TIZEN_ERROR_BATTERY_MONITOR = -0x03060000;

    [NativeTypeName("#define TIZEN_ERROR_GENERIC_TEXT_CLASSIFIER -0x03070000")]
    public const int TIZEN_ERROR_GENERIC_TEXT_CLASSIFIER = -0x03070000;

    [NativeTypeName("#define TIZEN_ERROR_DIAGNOSTICS -0x03080000")]
    public const int TIZEN_ERROR_DIAGNOSTICS = -0x03080000;

    [NativeTypeName("#define TIZEN_ERROR_GESTURE -0x03090000")]
    public const int TIZEN_ERROR_GESTURE = -0x03090000;

    [NativeTypeName("#define TIZEN_ERROR_USER_AWARENESS -0x030A0000")]
    public const int TIZEN_ERROR_USER_AWARENESS = -0x030A0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_WEBRTC -0x030B0000")]
    public const int TIZEN_ERROR_MEDIA_WEBRTC = -0x030B0000;

    [NativeTypeName("#define TIZEN_ERROR_CION -0x030C0000")]
    public const int TIZEN_ERROR_CION = -0x030C0000;

    [NativeTypeName("#define TIZEN_ERROR_MEDIA_EDITOR -0x030D0000")]
    public const int TIZEN_ERROR_MEDIA_EDITOR = -0x030D0000;

    [NativeTypeName("#define TIZEN_ERROR_RESOURCE_MONITOR -0x030E0000")]
    public const int TIZEN_ERROR_RESOURCE_MONITOR = -0x030E0000;

    [NativeTypeName("#define TIZEN_ERROR_TVS -0x030F0000")]
    public const int TIZEN_ERROR_TVS = -0x030F0000;

    [NativeTypeName("#define TIZEN_ERROR_WAUTHN -0x03100000")]
    public const int TIZEN_ERROR_WAUTHN = -0x03100000;

    [NativeTypeName("#define TIZEN_ERROR_WIFI_AWARE -0x03110000")]
    public const int TIZEN_ERROR_WIFI_AWARE = -0x03110000;

    [NativeTypeName("#define TIZEN_ERROR_MMI -0x03120000")]
    public const int TIZEN_ERROR_MMI = -0x03120000;
}
