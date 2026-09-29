using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AkiraScan.Camera.Platform.Linux;

internal static unsafe partial class V4L2PInvoke
{
    private const string Libc = "libc";

    public const int EPERM = 1;	/* Operation not permitted */
    public const int ENOENT = 2;	/* No such file or directory */
    public const int ESRCH = 3;	/* No such process */
    public const int EINTR = 4;	/* Interrupted system call */
    public const int EIO = 5;	/* I/O error */
    public const int ENXIO = 6;	/* No such device or address */
    public const int E2BIG = 7;	/* Argument list too long */
    public const int ENOEXEC = 8;	/* Exec format error */
    public const int EBADF = 9;	/* Bad file number */
    public const int ECHILD = 10;	/* No child processes */
    public const int EAGAIN = 11;	/* Try again */
    public const int ENOMEM = 12;	/* Out of memory */
    public const int EACCES = 13;	/* Permission denied */
    public const int EFAULT = 14;	/* Bad address */
    public const int ENOTBLK = 15;	/* Block device required */
    public const int EBUSY = 16;	/* Device or resource busy */
    public const int EEXIST = 17;	/* File exists */
    public const int EXDEV = 18;	/* Cross-device link */
    public const int ENODEV = 19;	/* No such device */
    public const int ENOTDIR = 20;	/* Not a directory */
    public const int EISDIR = 21;	/* Is a directory */
    public const int EINVAL = 22;	/* Invalid argument */
    public const int ENFILE = 23;	/* File table overflow */
    public const int EMFILE = 24;	/* Too many open files */
    public const int ENOTTY = 25;	/* Not a typewriter */
    public const int ETXTBSY = 26;	/* Text file busy */
    public const int EFBIG = 27;	/* File too large */
    public const int ENOSPC = 28;	/* No space left on device */
    public const int ESPIPE = 29;	/* Illegal seek */
    public const int EROFS = 30;	/* Read-only file system */
    public const int EMLINK = 31;	/* Too many links */
    public const int EPIPE = 32;	/* Broken pipe */
    public const int EDOM = 33;	/* Math argument out of domain of func */
    public const int ERANGE = 34;   /* Math result not representable */

    [StructLayout(LayoutKind.Sequential)]
    public struct c_pollfd
    {
        public int fd;         /* File descriptor to poll.  */
        public short events;       /* Types of events poller cares about.  */
        public short revents;		/* Types of events that actually occurred.  */
    }

    [LibraryImport(Libc, SetLastError = true)]
    public static partial int poll(c_pollfd* fds, CULong nfds, int timeout);

    // Common Poll Events Constants
    public const short POLLIN = 0x0001;
    public const short POLLOUT = 0x0004;
    public const short POLLERR = 0x0008;
    public const short POLLHUP = 0x0010;

    [LibraryImport(Libc, SetLastError = true)]
    public static partial IntPtr mmap(IntPtr addr, UIntPtr length, int prot, int flags, int fd, IntPtr offset);

    [LibraryImport(Libc, SetLastError = true)]
    public static partial int munmap(IntPtr addr, UIntPtr length);

    [LibraryImport(Libc, SetLastError = true)]
    public static partial int ioctl(int fd, CULong request, void* rb);

    [LibraryImport(Libc, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int open(string pathname, int flags, uint mode = 0);

    [LibraryImport(Libc, SetLastError = true)]
    public static partial int close(int fd);

    public const int PROT_READ = 0x1;
    public const int PROT_WRITE = 0x2;
    public const int PROT_EXEC = 0x4;
    public const int PROT_NONE = 0x0;

    public const int O_ACCMODE = 0x00000003;
    public const int O_RDONLY = 0x00000000;
    public const int O_WRONLY = 0x00000001;
    public const int O_RDWR = 0x00000002;
    public const int O_CREAT = 0x00000040;	/* not fcntl */
    public const int O_EXCL = 0x00000080;	/* not fcntl */
    public const int O_NOCTTY = 0x00000100;	/* not fcntl */
    public const int O_TRUNC = 0x00000200;	/* not fcntl */
    public const int O_APPEND = 0x00000400;
    public const int O_NONBLOCK = 0x00000800;
    public const int O_DSYNC = 0x00001000;	/* used to be O_SYNC, see below */
    public const int FASYNC = 0x00002000;	/* fcntl, for BSD compatibility */
    public const int O_DIRECT = 0x00004000;	/* direct disk access hint */
    public const int O_LARGEFILE = 0x00008000;
    public const int O_DIRECTORY = 0x00010000;	/* must be a directory */
    public const int O_NOFOLLOW = 0x00020000;	/* don't follow links */
    public const int O_NOATIME = 0x00040000;
    public const int O_CLOEXEC = 0x00080000;	/* set close_on_exec */

    public const int MAP_SHARED = 0x01;		/* Share changes */
    public const int MAP_PRIVATE = 0x02;		/* Changes are private */
    public const int MAP_SHARED_VALIDATE = 0x03;	/* share + validate extension flags */
    public const int MAP_DROPPABLE = 0x08;      /* Zero memory under memory pressure. */
    public const IntPtr MAP_FAILED = -1;

    /*  Four-character-code (FOURCC) */
    private static uint v4l2_fourcc(uint a, uint b, uint c, uint d) => a | (b << 8) | (c << 16) | (d << 24);
    private static uint v4l2_fourcc_be(uint a, uint b, uint c, uint d) => (v4l2_fourcc(a, b, c, d) | (1U << 31));
    private static uint _IOC(uint dir, uint type, uint nr, uint size) => (dir << 30) | (size << 16) | (type << 8) | nr;
    private static uint _IO(uint type, uint nr) => _IOC(0, type, nr, 0);
    private static uint _IOR<T>(uint type, uint nr) where T : unmanaged => _IOC(2, type, nr, (uint)Marshal.SizeOf<T>());
    private static uint _IOW<T>(uint type, uint nr) where T : unmanaged => _IOC(1, type, nr, (uint)Marshal.SizeOf<T>());
    private static uint _IOWR<T>(uint type, uint nr) where T : unmanaged => _IOC(3, type, nr, (uint)Marshal.SizeOf<T>());

    public const int VIDEO_MAX_FRAME = 32;
    public const int VIDEO_MAX_PLANES = 8;

    public static readonly uint VIDIOC_QUERYCAP = _IOR<v4l2_capability>('V', 0);
    public static readonly uint VIDIOC_ENUM_FMT = _IOWR<v4l2_fmtdesc>('V', 2);
    public static readonly uint VIDIOC_G_FMT = _IOWR<v4l2_format>('V', 4);
    public static readonly uint VIDIOC_S_FMT = _IOWR<v4l2_format>('V', 5);
    public static readonly uint VIDIOC_REQBUFS = _IOWR<v4l2_requestbuffers>('V', 8);
    public static readonly uint VIDIOC_QUERYBUF = _IOWR<v4l2_buffer>('V', 9);
    //public static readonly uint VIDIOC_G_FBUF = _IOR<v4l2_framebuffer>('V', 10);
    //public static readonly uint VIDIOC_S_FBUF = _IOW<v4l2_framebuffer>('V', 11);
    public static readonly uint VIDIOC_OVERLAY = _IOW<int>('V', 14);
    public static readonly uint VIDIOC_QBUF = _IOWR<v4l2_buffer>('V', 15);
    public static readonly uint VIDIOC_EXPBUF = _IOWR<v4l2_exportbuffer>('V', 16);
    public static readonly uint VIDIOC_DQBUF = _IOWR<v4l2_buffer>('V', 17);
    public static readonly uint VIDIOC_STREAMON = _IOW<int>('V', 18);
    public static readonly uint VIDIOC_STREAMOFF = _IOW<int>('V', 19);
    public static readonly uint VIDIOC_G_PARM = _IOWR<v4l2_streamparm>('V', 21);
    public static readonly uint VIDIOC_S_PARM = _IOWR<v4l2_streamparm>('V', 22);
    //public static readonly uint VIDIOC_G_STD = _IOR<v4l2_std_id>('V', 23);
    //public static readonly uint VIDIOC_S_STD = _IOW<v4l2_std_id>('V', 24);
    //public static readonly uint VIDIOC_ENUMSTD = _IOWR<v4l2_standard>('V', 25);
    //public static readonly uint VIDIOC_ENUMINPUT = _IOWR<v4l2_input>('V', 26);
    //public static readonly uint VIDIOC_G_CTRL = _IOWR<v4l2_control>('V', 27);
    //public static readonly uint VIDIOC_S_CTRL = _IOWR<v4l2_control>('V', 28);
    //public static readonly uint VIDIOC_G_TUNER = _IOWR<v4l2_tuner>('V', 29);
    //public static readonly uint VIDIOC_S_TUNER = _IOW<v4l2_tuner>('V', 30);
    //public static readonly uint VIDIOC_G_AUDIO = _IOR<v4l2_audio>('V', 33);
    //public static readonly uint VIDIOC_S_AUDIO = _IOW<v4l2_audio>('V', 34);
    //public static readonly uint VIDIOC_QUERYCTRL = _IOWR<v4l2_queryctrl>('V', 36);
    //public static readonly uint VIDIOC_QUERYMENU = _IOWR<v4l2_querymenu>('V', 37);
    public static readonly uint VIDIOC_G_INPUT = _IOR<int>('V', 38);
    public static readonly uint VIDIOC_S_INPUT = _IOWR<int>('V', 39);
    //public static readonly uint VIDIOC_G_EDID = _IOWR<v4l2_edid>('V', 40);
    //public static readonly uint VIDIOC_S_EDID = _IOWR<v4l2_edid>('V', 41);
    public static readonly uint VIDIOC_G_OUTPUT = _IOR<int>('V', 46);
    public static readonly uint VIDIOC_S_OUTPUT = _IOWR<int>('V', 47);
    //public static readonly uint VIDIOC_ENUMOUTPUT = _IOWR<v4l2_output>('V', 48);
    //public static readonly uint VIDIOC_G_AUDOUT = _IOR<v4l2_audioout>('V', 49);
    //public static readonly uint VIDIOC_S_AUDOUT = _IOW<v4l2_audioout>('V', 50);
    //public static readonly uint VIDIOC_G_MODULATOR = _IOWR<v4l2_modulator>('V', 54);
    //public static readonly uint VIDIOC_S_MODULATOR = _IOW<v4l2_modulator>('V', 55);
    //public static readonly uint VIDIOC_G_FREQUENCY = _IOWR<v4l2_frequency>('V', 56);
    //public static readonly uint VIDIOC_S_FREQUENCY = _IOW<v4l2_frequency>('V', 57);
    //public static readonly uint VIDIOC_CROPCAP = _IOWR<v4l2_cropcap>('V', 58);
    //public static readonly uint VIDIOC_G_CROP = _IOWR<v4l2_crop>('V', 59);
    //public static readonly uint VIDIOC_S_CROP = _IOW<v4l2_crop>('V', 60);
    //public static readonly uint VIDIOC_G_JPEGCOMP = _IOR<v4l2_jpegcompression>('V', 61);
    //public static readonly uint VIDIOC_S_JPEGCOMP = _IOW<v4l2_jpegcompression>('V', 62);
    //public static readonly uint VIDIOC_QUERYSTD = _IOR<v4l2_std_id>('V', 63);
    //public static readonly uint VIDIOC_TRY_FMT = _IOWR<v4l2_format>('V', 64);
    //public static readonly uint VIDIOC_ENUMAUDIO = _IOWR<v4l2_audio>('V', 65);
    //public static readonly uint VIDIOC_ENUMAUDOUT = _IOWR<v4l2_audioout>('V', 66);
    //public static readonly uint VIDIOC_G_PRIORITY = _IOR<uint>('V', 67); /* enum v4l2_priority */
    //public static readonly uint VIDIOC_S_PRIORITY = _IOW<uint>('V', 68); /* enum v4l2_priority */
    //public static readonly uint VIDIOC_G_SLICED_VBI_CAP = _IOWR<v4l2_sliced_vbi_cap>('V', 69);
    public static readonly uint VIDIOC_LOG_STATUS = _IO('V', 70);
    //public static readonly uint VIDIOC_G_EXT_CTRLS = _IOWR<v4l2_ext_controls>('V', 71);
    //public static readonly uint VIDIOC_S_EXT_CTRLS = _IOWR<v4l2_ext_controls>('V', 72);
    //public static readonly uint VIDIOC_TRY_EXT_CTRLS = _IOWR<v4l2_ext_controls>('V', 73);
    //public static readonly uint VIDIOC_ENUM_FRAMESIZES = _IOWR<v4l2_frmsizeenum>('V', 74);
    //public static readonly uint VIDIOC_ENUM_FRAMEINTERVALS = _IOWR<v4l2_frmivalenum>('V', 75);
    //public static readonly uint VIDIOC_G_ENC_INDEX = _IOR<v4l2_enc_idx>('V', 76);
    //public static readonly uint VIDIOC_ENCODER_CMD = _IOWR<v4l2_encoder_cmd>('V', 77);
    //public static readonly uint VIDIOC_TRY_ENCODER_CMD = _IOWR<v4l2_encoder_cmd>('V', 78);

    public static int ioctl<T>(int fd, uint request, ref T rb) where T : unmanaged
    {
        fixed (T* ptr = &rb)
        {
            return ioctl(fd, new CULong(request), ptr);
        }
    }

    public const uint V4L2_CAP_VIDEO_CAPTURE = 0x00000001;
    public const uint V4L2_CAP_VIDEO_OUTPUT = 0x00000002;
    public const uint V4L2_CAP_VIDEO_OVERLAY = 0x00000004;
    public const uint V4L2_CAP_VBI_CAPTURE = 0x00000010;
    public const uint V4L2_CAP_VBI_OUTPUT = 0x00000020;
    public const uint V4L2_CAP_SLICED_VBI_CAPTURE = 0x00000040;
    public const uint V4L2_CAP_SLICED_VBI_OUTPUT = 0x00000080;
    public const uint V4L2_CAP_RDS_CAPTURE = 0x00000100;
    public const uint V4L2_CAP_VIDEO_OUTPUT_OVERLAY = 0x00000200;
    public const uint V4L2_CAP_HW_FREQ_SEEK = 0x00000400;
    public const uint V4L2_CAP_RDS_OUTPUT = 0x00000800;

    public const uint V4L2_CAP_VIDEO_CAPTURE_MPLANE = 0x00001000;
    public const uint V4L2_CAP_VIDEO_OUTPUT_MPLANE = 0x00002000;
    public const uint V4L2_CAP_VIDEO_M2M_MPLANE = 0x00004000;
    public const uint V4L2_CAP_VIDEO_M2M = 0x00008000;

    public const uint V4L2_CAP_TUNER = 0x00010000;
    public const uint V4L2_CAP_AUDIO = 0x00020000;
    public const uint V4L2_CAP_RADIO = 0x00040000;
    public const uint V4L2_CAP_MODULATOR = 0x00080000;

    public const uint V4L2_CAP_SDR_CAPTURE = 0x00100000;
    public const uint V4L2_CAP_EXT_PIX_FORMAT = 0x00200000;
    public const uint V4L2_CAP_SDR_OUTPUT = 0x00400000;
    public const uint V4L2_CAP_META_CAPTURE = 0x00800000;

    public const uint V4L2_CAP_READWRITE = 0x01000000;
    public const uint V4L2_CAP_EDID = 0x02000000;
    public const uint V4L2_CAP_STREAMING = 0x04000000;
    public const uint V4L2_CAP_META_OUTPUT = 0x08000000;

    public const uint V4L2_CAP_TOUCH = 0x10000000;

    public const uint V4L2_CAP_IO_MC = 0x20000000;

    public const uint V4L2_CAP_DEVICE_CAPS = 0x80000000;

    public enum v4l2_memory : uint
    {
        V4L2_MEMORY_MMAP = 1,
        V4L2_MEMORY_USERPTR = 2,
        V4L2_MEMORY_OVERLAY = 3,
        V4L2_MEMORY_DMABUF = 4,
    }

    public enum v4l2_buf_type : uint
    {
        V4L2_BUF_TYPE_VIDEO_CAPTURE = 1,
        V4L2_BUF_TYPE_VIDEO_OUTPUT = 2,
        V4L2_BUF_TYPE_VIDEO_OVERLAY = 3,
        V4L2_BUF_TYPE_VBI_CAPTURE = 4,
        V4L2_BUF_TYPE_VBI_OUTPUT = 5,
        V4L2_BUF_TYPE_SLICED_VBI_CAPTURE = 6,
        V4L2_BUF_TYPE_SLICED_VBI_OUTPUT = 7,
        V4L2_BUF_TYPE_VIDEO_OUTPUT_OVERLAY = 8,
        V4L2_BUF_TYPE_VIDEO_CAPTURE_MPLANE = 9,
        V4L2_BUF_TYPE_VIDEO_OUTPUT_MPLANE = 10,
        V4L2_BUF_TYPE_SDR_CAPTURE = 11,
        V4L2_BUF_TYPE_SDR_OUTPUT = 12,
        V4L2_BUF_TYPE_META_CAPTURE = 13,
        V4L2_BUF_TYPE_META_OUTPUT = 14,
        V4L2_BUF_TYPE_PRIVATE = 0x80,
    }

    public enum v4l2_field : uint
    {
        V4L2_FIELD_ANY = 0, /* driver can choose from none,
					 top, bottom, interlaced
					 depending on whatever it thinks
					 is approximate ... */
        V4L2_FIELD_NONE = 1, /* this device has no fields ... */
        V4L2_FIELD_TOP = 2, /* top field only */
        V4L2_FIELD_BOTTOM = 3, /* bottom field only */
        V4L2_FIELD_INTERLACED = 4, /* both fields interlaced */
        V4L2_FIELD_SEQ_TB = 5, /* both fields sequential into one
					 buffer, top-bottom order */
        V4L2_FIELD_SEQ_BT = 6, /* same as above + bottom-top order */
        V4L2_FIELD_ALTERNATE = 7, /* both fields alternating into
					 separate buffers */
        V4L2_FIELD_INTERLACED_TB = 8, /* both fields interlaced, top field
					 first and the top field is
					 transmitted first */
        V4L2_FIELD_INTERLACED_BT = 9, /* both fields interlaced, top field
					 first and the bottom field is
					 transmitted first */
    }

    public enum v4l2_colorspace: uint
    {
        /*
         * Default colorspace, i.e. let the driver figure it out.
         * Can only be used with video capture.
         */
        V4L2_COLORSPACE_DEFAULT = 0,

        /* SMPTE 170M: used for broadcast NTSC/PAL SDTV */
        V4L2_COLORSPACE_SMPTE170M = 1,

        /* Obsolete pre-1998 SMPTE 240M HDTV standard, superseded by Rec 709 */
        V4L2_COLORSPACE_SMPTE240M = 2,

        /* Rec.709: used for HDTV */
        V4L2_COLORSPACE_REC709 = 3,

        /*
         * Deprecated, do not use. No driver will ever return this. This was
         * based on a misunderstanding of the bt878 datasheet.
         */
        V4L2_COLORSPACE_BT878 = 4,

        /*
         * NTSC 1953 colorspace. This only makes sense when dealing with
         * really, really old NTSC recordings. Superseded by SMPTE 170M.
         */
        V4L2_COLORSPACE_470_SYSTEM_M = 5,

        /*
         * EBU Tech 3213 PAL/SECAM colorspace.
         */
        V4L2_COLORSPACE_470_SYSTEM_BG = 6,

        /*
         * Effectively shorthand for V4L2_COLORSPACE_SRGB, V4L2_YCBCR_ENC_601
         * and V4L2_QUANTIZATION_FULL_RANGE. To be used for (Motion-)JPEG.
         */
        V4L2_COLORSPACE_JPEG = 7,

        /* For RGB colorspaces such as produces by most webcams. */
        V4L2_COLORSPACE_SRGB = 8,

        /* opRGB colorspace */
        V4L2_COLORSPACE_OPRGB = 9,

        /* BT.2020 colorspace, used for UHDTV. */
        V4L2_COLORSPACE_BT2020 = 10,

        /* Raw colorspace: for RAW unprocessed images */
        V4L2_COLORSPACE_RAW = 11,

        /* DCI-P3 colorspace, used by cinema projectors */
        V4L2_COLORSPACE_DCI_P3 = 12,

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_requestbuffers
    {
        public uint count;
        public uint type;     /* enum v4l2_buf_type */
        public uint memory;       /* enum v4l2_memory */
        public uint capabilities;
        public byte flags;
        public fixed byte reserved[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_exportbuffer
    {
        public uint type; /* enum v4l2_buf_type */
        public uint index;
        public uint plane;
        public uint flags;
        public int fd;
        public fixed uint reserved[11];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_capability
    {
        public fixed byte driver[16];
        public fixed byte card[32];
        public fixed byte bus_info[32];
        public uint version;
        public uint capabilities;
        public uint device_caps;
        public fixed uint reserved[3];
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_buffer
    {
        public uint index;
        public uint type;
        public uint bytesused;
        public uint flags;
        public uint field;
        public c_timeval timestamp;
        public v4l2_timecode timecode;
        public uint sequence;
        /* memory location */
        public uint memory;
        public union1 m;
        public uint length;
        public uint reserved2;
        public union2 u2;

        [StructLayout(LayoutKind.Explicit)]
        public struct union1
        {
            [FieldOffset(0)]
            public uint offset;
            [FieldOffset(0)]
            public CULong userptr;
            [FieldOffset(0)]
            public v4l2_plane* planes;
            [FieldOffset(0)]
            public int fd;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct union2
        {
            [FieldOffset(0)]
            public int request_fd;
            [FieldOffset(0)]
            public uint reserved;
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_plane
    {
        public uint bytesused;
        public uint length;
        public union1 m;
        public uint data_offset;
        public fixed uint reserved[11];

        [StructLayout(LayoutKind.Explicit)]
        public struct union1
        {
            [FieldOffset(0)]
            public uint mem_offset;
            [FieldOffset(0)]
            public CULong userptr;
            [FieldOffset(0)]
            public int fd;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_timecode
    {
        public uint type;
        public uint flags;
        public byte frames;
        public byte seconds;
        public byte minutes;
        public byte hours;
        public fixed byte userbits[4];
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct c_timeval
    {
        public IntPtr tv_sec;
        public IntPtr tv_usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_fmtdesc
    {
        public uint index;             /* Format number      */
        public uint type;              /* enum v4l2_buf_type */
        public uint flags;
        public fixed byte description[32];   /* Description string */
        public uint pixelformat;       /* Format fourcc      */
        public uint mbus_code;        /* Media bus code    */
        public fixed uint reserved[3];
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_format
    {
        public uint type;
        public union1 fmt;

        [StructLayout(LayoutKind.Explicit)]
        public struct union1
        {
            [FieldOffset(0)]
            public v4l2_pix_format pix;     /* V4L2_BUF_TYPE_VIDEO_CAPTURE */
            [FieldOffset(0)]
            public v4l2_pix_format_mplane pix_mp;  /* V4L2_BUF_TYPE_VIDEO_CAPTURE_MPLANE */
            [FieldOffset(0)]
            public v4l2_window win;     /* V4L2_BUF_TYPE_VIDEO_OVERLAY */
            [FieldOffset(0)]
            public v4l2_vbi_format vbi;     /* V4L2_BUF_TYPE_VBI_CAPTURE */
            [FieldOffset(0)]
            public v4l2_sliced_vbi_format sliced;  /* V4L2_BUF_TYPE_SLICED_VBI_CAPTURE */
            [FieldOffset(0)]
            public v4l2_sdr_format sdr;     /* V4L2_BUF_TYPE_SDR_CAPTURE */
            [FieldOffset(0)]
            public v4l2_meta_format meta;    /* V4L2_BUF_TYPE_META_CAPTURE */
            [FieldOffset(0)]
            public fixed byte raw_data[200];                   /* user-defined */
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_pix_format
    {
        public uint width;
        public uint height;
        public uint pixelformat;
        public uint field;        /* enum v4l2_field */
        public uint bytesperline; /* for padding, zero if unused */
        public uint sizeimage;
        public uint colorspace;   /* enum v4l2_colorspace */
        public uint priv;     /* private data, depends on pixelformat */
        public uint flags;        /* format flags (V4L2_PIX_FMT_FLAG_*) */
        public union1 u1;
        public uint quantization; /* enum v4l2_quantization */
        public uint xfer_func;	/* enum v4l2_xfer_func */

        [StructLayout(LayoutKind.Explicit)]
        public struct union1
        {
            /* enum v4l2_ycbcr_encoding */
            [FieldOffset(0)]
            public uint ycbcr_enc;
            /* enum v4l2_hsv_encoding */
            [FieldOffset(0)]
            public uint hsv_enc;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct v4l2_pix_format_mplane
    {
        public uint width;
        public uint height;
        public uint pixelformat;
        public uint field;
        public uint colorspace;

        public v4l2_plane_pix_format_array1 plane_fmt;
        public byte num_planes;
        public byte flags;
        public union1 u1;
        public byte quantization;
        public byte xfer_func;
        public fixed byte reserved[7];

        [StructLayout(LayoutKind.Explicit)]
        public struct union1
        {
            [FieldOffset(0)]
            public byte ycbcr_enc;
            [FieldOffset(0)]
            public byte hsv_enc;
        }

        [InlineArray(VIDEO_MAX_PLANES)]
        public struct v4l2_plane_pix_format_array1
        {
            public v4l2_plane_pix_format e;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct v4l2_plane_pix_format
    {
        public uint sizeimage;
        public uint bytesperline;
        public fixed ushort reserved[6];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_window
    {
        public v4l2_rect w;
        public uint field;   /* enum v4l2_field */
        public uint chromakey;
        public v4l2_clip* clips;
        public uint clipcount;
        public void* bitmap;
        public byte global_alpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_vbi_format
    {
        public uint sampling_rate;        /* in 1 Hz */
        public uint offset;
        public uint samples_per_line;
        public uint sample_format;        /* V4L2_PIX_FMT_* */
        public fixed int start[2];
        public fixed uint count[2];
        public uint flags;            /* V4L2_VBI_* */
        public fixed uint reserved[2];      /* must be zero */
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_sliced_vbi_format
    {
        public ushort service_set;
        /* service_lines[0][...] specifies lines 0-23 (1-23 used) of the first field
           service_lines[1][...] specifies lines 0-23 (1-23 used) of the second field
                     (equals frame lines 313-336 for 625 line video
                      standards, 263-286 for 525 line standards) */
        public fixed ushort service_lines[2 * 24];
        public uint io_size;
        public fixed uint reserved[2];            /* must be zero */
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct v4l2_sdr_format
    {
        public uint pixelformat;
        public uint buffersize;
        public fixed byte reserved[24];
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct v4l2_meta_format
    {
        public uint dataformat;
        public uint buffersize;
        public uint width;
        public uint height;
        public uint bytesperline;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_rect
    {
        public int left;
        public int top;
        public uint width;
        public uint height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_clip
    {
        public v4l2_rect c;
        public v4l2_clip* next;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_streamparm
    {
        public uint type;         /* enum v4l2_buf_type */
        public union1 parm;

        [StructLayout(LayoutKind.Explicit)]
        public struct union1
        {
            [FieldOffset(0)]
            public v4l2_captureparm capture;
            [FieldOffset(0)]
            public v4l2_outputparm output;
            [FieldOffset(0)]
            public fixed byte raw_data[200];  /* user-defined */
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_outputparm
    {
        public uint capability;    /*  Supported modes */
        public uint outputmode;    /*  Current mode */
        public v4l2_fract timeperframe; /*  Time per frame in seconds */
        public uint extendedmode; /*  Driver-specific extensions */
        public uint writebuffers; /*  # of buffers for write */
        public fixed uint reserved[4];
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_captureparm
    {
        public uint capability;     /*  Supported modes */
        public uint capturemode;    /*  Current mode */
        public v4l2_fract timeperframe;  /*  Time per frame in seconds */
        public uint extendedmode;  /*  Driver-specific extensions */
        public uint readbuffers;   /*  # of buffers for read */
        public fixed uint reserved[4];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct v4l2_fract
    {
        public uint numerator;
        public uint denominator;
    }

    /* RGB formats (1 or 2 bytes per pixel) */
    public static readonly uint V4L2_PIX_FMT_RGB332 = v4l2_fourcc('R', 'G', 'B', '1');  /*  8  RGB-3-3-2     */
    public static readonly uint V4L2_PIX_FMT_RGB444 = v4l2_fourcc('R', '4', '4', '4');  /* 16  xxxxrrrr ggggbbbb */
    public static readonly uint V4L2_PIX_FMT_ARGB444 = v4l2_fourcc('A', 'R', '1', '2');  /* 16  aaaarrrr ggggbbbb */
    public static readonly uint V4L2_PIX_FMT_XRGB444 = v4l2_fourcc('X', 'R', '1', '2');  /* 16  xxxxrrrr ggggbbbb */
    public static readonly uint V4L2_PIX_FMT_RGBA444 = v4l2_fourcc('R', 'A', '1', '2');  /* 16  rrrrgggg bbbbaaaa */
    public static readonly uint V4L2_PIX_FMT_RGBX444 = v4l2_fourcc('R', 'X', '1', '2');  /* 16  rrrrgggg bbbbxxxx */
    public static readonly uint V4L2_PIX_FMT_ABGR444 = v4l2_fourcc('A', 'B', '1', '2');  /* 16  aaaabbbb ggggrrrr */
    public static readonly uint V4L2_PIX_FMT_XBGR444 = v4l2_fourcc('X', 'B', '1', '2');  /* 16  xxxxbbbb ggggrrrr */
    public static readonly uint V4L2_PIX_FMT_BGRA444 = v4l2_fourcc('G', 'A', '1', '2');  /* 16  bbbbgggg rrrraaaa */
    public static readonly uint V4L2_PIX_FMT_BGRX444 = v4l2_fourcc('B', 'X', '1', '2');  /* 16  bbbbgggg rrrrxxxx */
    public static readonly uint V4L2_PIX_FMT_RGB555 = v4l2_fourcc('R', 'G', 'B', 'O');  /* 16  RGB-5-5-5     */
    public static readonly uint V4L2_PIX_FMT_ARGB555 = v4l2_fourcc('A', 'R', '1', '5');  /* 16  ARGB-1-5-5-5  */
    public static readonly uint V4L2_PIX_FMT_XRGB555 = v4l2_fourcc('X', 'R', '1', '5');  /* 16  XRGB-1-5-5-5  */
    public static readonly uint V4L2_PIX_FMT_RGBA555 = v4l2_fourcc('R', 'A', '1', '5');  /* 16  RGBA-5-5-5-1  */
    public static readonly uint V4L2_PIX_FMT_RGBX555 = v4l2_fourcc('R', 'X', '1', '5');  /* 16  RGBX-5-5-5-1  */
    public static readonly uint V4L2_PIX_FMT_ABGR555 = v4l2_fourcc('A', 'B', '1', '5');  /* 16  ABGR-1-5-5-5  */
    public static readonly uint V4L2_PIX_FMT_XBGR555 = v4l2_fourcc('X', 'B', '1', '5');  /* 16  XBGR-1-5-5-5  */
    public static readonly uint V4L2_PIX_FMT_BGRA555 = v4l2_fourcc('B', 'A', '1', '5');  /* 16  BGRA-5-5-5-1  */
    public static readonly uint V4L2_PIX_FMT_BGRX555 = v4l2_fourcc('B', 'X', '1', '5');  /* 16  BGRX-5-5-5-1  */
    public static readonly uint V4L2_PIX_FMT_RGB565 = v4l2_fourcc('R', 'G', 'B', 'P');  /* 16  RGB-5-6-5     */
    public static readonly uint V4L2_PIX_FMT_RGB555X = v4l2_fourcc('R', 'G', 'B', 'Q');  /* 16  RGB-5-5-5 BE  */
    public static readonly uint V4L2_PIX_FMT_ARGB555X = v4l2_fourcc_be('A', 'R', '1', '5'); /* 16  ARGB-5-5-5 BE */
    public static readonly uint V4L2_PIX_FMT_XRGB555X = v4l2_fourcc_be('X', 'R', '1', '5'); /* 16  XRGB-5-5-5 BE */
    public static readonly uint V4L2_PIX_FMT_RGB565X = v4l2_fourcc('R', 'G', 'B', 'R');  /* 16  RGB-5-6-5 BE  */

    /* RGB formats (3 or 4 bytes per pixel) */
    public static readonly uint V4L2_PIX_FMT_BGR666 = v4l2_fourcc('B', 'G', 'R', 'H');  /* 18  BGR-6-6-6	  */
    public static readonly uint V4L2_PIX_FMT_BGR24 = v4l2_fourcc('B', 'G', 'R', '3');  /* 24  BGR-8-8-8     */
    public static readonly uint V4L2_PIX_FMT_RGB24 = v4l2_fourcc('R', 'G', 'B', '3');  /* 24  RGB-8-8-8     */
    public static readonly uint V4L2_PIX_FMT_BGR32 = v4l2_fourcc('B', 'G', 'R', '4');  /* 32  BGR-8-8-8-8   */
    public static readonly uint V4L2_PIX_FMT_ABGR32 = v4l2_fourcc('A', 'R', '2', '4');  /* 32  BGRA-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_XBGR32 = v4l2_fourcc('X', 'R', '2', '4');  /* 32  BGRX-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_BGRA32 = v4l2_fourcc('R', 'A', '2', '4');  /* 32  ABGR-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_BGRX32 = v4l2_fourcc('R', 'X', '2', '4');  /* 32  XBGR-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_RGB32 = v4l2_fourcc('R', 'G', 'B', '4');  /* 32  RGB-8-8-8-8   */
    public static readonly uint V4L2_PIX_FMT_RGBA32 = v4l2_fourcc('A', 'B', '2', '4');  /* 32  RGBA-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_RGBX32 = v4l2_fourcc('X', 'B', '2', '4');  /* 32  RGBX-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_ARGB32 = v4l2_fourcc('B', 'A', '2', '4');  /* 32  ARGB-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_XRGB32 = v4l2_fourcc('B', 'X', '2', '4');  /* 32  XRGB-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_RGBX1010102 = v4l2_fourcc('R', 'X', '3', '0');  /* 32  RGBX-10-10-10-2 */
    public static readonly uint V4L2_PIX_FMT_RGBA1010102 = v4l2_fourcc('R', 'A', '3', '0');  /* 32  RGBA-10-10-10-2 */
    public static readonly uint V4L2_PIX_FMT_ARGB2101010 = v4l2_fourcc('A', 'R', '3', '0');  /* 32  ARGB-2-10-10-10 */

    /* RGB formats (6 or 8 bytes per pixel) */
    public static readonly uint V4L2_PIX_FMT_BGR48_12 = v4l2_fourcc('B', '3', '1', '2');  /* 48  BGR 12-bit per component */
    public static readonly uint V4L2_PIX_FMT_BGR48 = v4l2_fourcc('B', 'G', 'R', '6');  /* 48  BGR 16-bit per component */
    public static readonly uint V4L2_PIX_FMT_RGB48 = v4l2_fourcc('R', 'G', 'B', '6');  /* 48  RGB 16-bit per component */
    public static readonly uint V4L2_PIX_FMT_ABGR64_12 = v4l2_fourcc('B', '4', '1', '2');  /* 64  BGRA 12-bit per component */

    /* Grey formats */
    public static readonly uint V4L2_PIX_FMT_GREY = v4l2_fourcc('G', 'R', 'E', 'Y');  /*  8  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y4 = v4l2_fourcc('Y', '0', '4', ' ');  /*  4  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y6 = v4l2_fourcc('Y', '0', '6', ' ');  /*  6  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y10 = v4l2_fourcc('Y', '1', '0', ' ');  /* 10  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y12 = v4l2_fourcc('Y', '1', '2', ' ');  /* 12  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y012 = v4l2_fourcc('Y', '0', '1', '2');  /* 12  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y14 = v4l2_fourcc('Y', '1', '4', ' ');  /* 14  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y16 = v4l2_fourcc('Y', '1', '6', ' ');  /* 16  Greyscale     */
    public static readonly uint V4L2_PIX_FMT_Y16_BE = v4l2_fourcc_be('Y', '1', '6', ' '); /* 16  Greyscale BE  */

    /* Grey bit-packed formats */
    public static readonly uint V4L2_PIX_FMT_Y10BPACK = v4l2_fourcc('Y', '1', '0', 'B');  /* 10  Greyscale bit-packed */
    public static readonly uint V4L2_PIX_FMT_Y10P = v4l2_fourcc('Y', '1', '0', 'P');  /* 10  Greyscale, MIPI RAW10 packed */
    public static readonly uint V4L2_PIX_FMT_IPU3_Y10 = v4l2_fourcc('i', 'p', '3', 'y');  /* IPU3 packed 10-bit greyscale */
    public static readonly uint V4L2_PIX_FMT_Y12P = v4l2_fourcc('Y', '1', '2', 'P');  /* 12  Greyscale, MIPI RAW12 packed */
    public static readonly uint V4L2_PIX_FMT_Y14P = v4l2_fourcc('Y', '1', '4', 'P');  /* 14  Greyscale, MIPI RAW14 packed */

    /* Palette formats */
    public static readonly uint V4L2_PIX_FMT_PAL8 = v4l2_fourcc('P', 'A', 'L', '8');  /*  8  8-bit palette */

    /* Chrominance formats */
    public static readonly uint V4L2_PIX_FMT_UV8 = v4l2_fourcc('U', 'V', '8', ' ');  /*  8  UV 4:4 */

    /* Luminance+Chrominance formats */
    public static readonly uint V4L2_PIX_FMT_YUYV = v4l2_fourcc('Y', 'U', 'Y', 'V');  /* 16  YUV 4:2:2     */
    public static readonly uint V4L2_PIX_FMT_YYUV = v4l2_fourcc('Y', 'Y', 'U', 'V');  /* 16  YUV 4:2:2     */
    public static readonly uint V4L2_PIX_FMT_YVYU = v4l2_fourcc('Y', 'V', 'Y', 'U');  /* 16 YVU 4:2:2 */
    public static readonly uint V4L2_PIX_FMT_UYVY = v4l2_fourcc('U', 'Y', 'V', 'Y');  /* 16  YUV 4:2:2     */
    public static readonly uint V4L2_PIX_FMT_VYUY = v4l2_fourcc('V', 'Y', 'U', 'Y');  /* 16  YUV 4:2:2     */
    public static readonly uint V4L2_PIX_FMT_Y41P = v4l2_fourcc('Y', '4', '1', 'P');  /* 12  YUV 4:1:1     */
    public static readonly uint V4L2_PIX_FMT_YUV444 = v4l2_fourcc('Y', '4', '4', '4');  /* 16  xxxxyyyy uuuuvvvv */
    public static readonly uint V4L2_PIX_FMT_YUV555 = v4l2_fourcc('Y', 'U', 'V', 'O');  /* 16  YUV-5-5-5     */
    public static readonly uint V4L2_PIX_FMT_YUV565 = v4l2_fourcc('Y', 'U', 'V', 'P');  /* 16  YUV-5-6-5     */
    public static readonly uint V4L2_PIX_FMT_YUV24 = v4l2_fourcc('Y', 'U', 'V', '3');  /* 24  YUV-8-8-8     */
    public static readonly uint V4L2_PIX_FMT_YUV32 = v4l2_fourcc('Y', 'U', 'V', '4');  /* 32  YUV-8-8-8-8   */
    public static readonly uint V4L2_PIX_FMT_AYUV32 = v4l2_fourcc('A', 'Y', 'U', 'V');  /* 32  AYUV-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_XYUV32 = v4l2_fourcc('X', 'Y', 'U', 'V');  /* 32  XYUV-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_VUYA32 = v4l2_fourcc('V', 'U', 'Y', 'A');  /* 32  VUYA-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_VUYX32 = v4l2_fourcc('V', 'U', 'Y', 'X');  /* 32  VUYX-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_YUVA32 = v4l2_fourcc('Y', 'U', 'V', 'A');  /* 32  YUVA-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_YUVX32 = v4l2_fourcc('Y', 'U', 'V', 'X');  /* 32  YUVX-8-8-8-8  */
    public static readonly uint V4L2_PIX_FMT_M420 = v4l2_fourcc('M', '4', '2', '0');  /* 12  YUV 4:2:0 2 lines y, 1 line uv interleaved */
    public static readonly uint V4L2_PIX_FMT_YUV48_12 = v4l2_fourcc('Y', '3', '1', '2');  /* 48  YUV 4:4:4 12-bit per component */

    /*
     * YCbCr packed format. For each Y2xx format, xx bits of valid data occupy the MSBs
     * of the 16 bit components, and 16-xx bits of zero padding occupy the LSBs.
     */
    public static readonly uint V4L2_PIX_FMT_Y210 = v4l2_fourcc('Y', '2', '1', '0');  /* 32  YUYV 4:2:2 */
    public static readonly uint V4L2_PIX_FMT_Y212 = v4l2_fourcc('Y', '2', '1', '2');  /* 32  YUYV 4:2:2 */
    public static readonly uint V4L2_PIX_FMT_Y216 = v4l2_fourcc('Y', '2', '1', '6');  /* 32  YUYV 4:2:2 */

    /* two planes -- one Y, one Cr + Cb interleaved  */
    public static readonly uint V4L2_PIX_FMT_NV12 = v4l2_fourcc('N', 'V', '1', '2');  /* 12  Y/CbCr 4:2:0  */
    public static readonly uint V4L2_PIX_FMT_NV21 = v4l2_fourcc('N', 'V', '2', '1');  /* 12  Y/CrCb 4:2:0  */
    public static readonly uint V4L2_PIX_FMT_NV15 = v4l2_fourcc('N', 'V', '1', '5');  /* 15  Y/CbCr 4:2:0 10-bit packed */
    public static readonly uint V4L2_PIX_FMT_NV16 = v4l2_fourcc('N', 'V', '1', '6');  /* 16  Y/CbCr 4:2:2  */
    public static readonly uint V4L2_PIX_FMT_NV61 = v4l2_fourcc('N', 'V', '6', '1');  /* 16  Y/CrCb 4:2:2  */
    public static readonly uint V4L2_PIX_FMT_NV20 = v4l2_fourcc('N', 'V', '2', '0');  /* 20  Y/CbCr 4:2:2 10-bit packed */
    public static readonly uint V4L2_PIX_FMT_NV24 = v4l2_fourcc('N', 'V', '2', '4');  /* 24  Y/CbCr 4:4:4  */
    public static readonly uint V4L2_PIX_FMT_NV42 = v4l2_fourcc('N', 'V', '4', '2');  /* 24  Y/CrCb 4:4:4  */
    public static readonly uint V4L2_PIX_FMT_P010 = v4l2_fourcc('P', '0', '1', '0');  /* 24  Y/CbCr 4:2:0 10-bit per component */
    public static readonly uint V4L2_PIX_FMT_P012 = v4l2_fourcc('P', '0', '1', '2');  /* 24  Y/CbCr 4:2:0 12-bit per component */

    /* two non contiguous planes - one Y, one Cr + Cb interleaved  */
    public static readonly uint V4L2_PIX_FMT_NV12M = v4l2_fourcc('N', 'M', '1', '2');  /* 12  Y/CbCr 4:2:0  */
    public static readonly uint V4L2_PIX_FMT_NV21M = v4l2_fourcc('N', 'M', '2', '1');  /* 21  Y/CrCb 4:2:0  */
    public static readonly uint V4L2_PIX_FMT_NV16M = v4l2_fourcc('N', 'M', '1', '6');  /* 16  Y/CbCr 4:2:2  */
    public static readonly uint V4L2_PIX_FMT_NV61M = v4l2_fourcc('N', 'M', '6', '1');  /* 16  Y/CrCb 4:2:2  */
    public static readonly uint V4L2_PIX_FMT_P012M = v4l2_fourcc('P', 'M', '1', '2');  /* 24  Y/CbCr 4:2:0 12-bit per component */

    /* three planes - Y Cb, Cr */
    public static readonly uint V4L2_PIX_FMT_YUV410 = v4l2_fourcc('Y', 'U', 'V', '9');  /*  9  YUV 4:1:0     */
    public static readonly uint V4L2_PIX_FMT_YVU410 = v4l2_fourcc('Y', 'V', 'U', '9');  /*  9  YVU 4:1:0     */
    public static readonly uint V4L2_PIX_FMT_YUV411P = v4l2_fourcc('4', '1', '1', 'P');  /* 12  YVU411 planar */
    public static readonly uint V4L2_PIX_FMT_YUV420 = v4l2_fourcc('Y', 'U', '1', '2');  /* 12  YUV 4:2:0     */
    public static readonly uint V4L2_PIX_FMT_YVU420 = v4l2_fourcc('Y', 'V', '1', '2');  /* 12  YVU 4:2:0     */
    public static readonly uint V4L2_PIX_FMT_YUV422P = v4l2_fourcc('4', '2', '2', 'P');  /* 16  YVU422 planar */

    /* three non contiguous planes - Y, Cb, Cr */
    public static readonly uint V4L2_PIX_FMT_YUV420M = v4l2_fourcc('Y', 'M', '1', '2');  /* 12  YUV420 planar */
    public static readonly uint V4L2_PIX_FMT_YVU420M = v4l2_fourcc('Y', 'M', '2', '1');  /* 12  YVU420 planar */
    public static readonly uint V4L2_PIX_FMT_YUV422M = v4l2_fourcc('Y', 'M', '1', '6');  /* 16  YUV422 planar */
    public static readonly uint V4L2_PIX_FMT_YVU422M = v4l2_fourcc('Y', 'M', '6', '1');  /* 16  YVU422 planar */
    public static readonly uint V4L2_PIX_FMT_YUV444M = v4l2_fourcc('Y', 'M', '2', '4');  /* 24  YUV444 planar */
    public static readonly uint V4L2_PIX_FMT_YVU444M = v4l2_fourcc('Y', 'M', '4', '2');  /* 24  YVU444 planar */

    /* Tiled YUV formats */
    public static readonly uint V4L2_PIX_FMT_NV12_4L4 = v4l2_fourcc('V', 'T', '1', '2');    /* 12  Y/CbCr 4:2:0  4x4 tiles */
    public static readonly uint V4L2_PIX_FMT_NV12_16L16 = v4l2_fourcc('H', 'M', '1', '2');  /* 12  Y/CbCr 4:2:0 16x16 tiles */
    public static readonly uint V4L2_PIX_FMT_NV12_32L32 = v4l2_fourcc('S', 'T', '1', '2');  /* 12  Y/CbCr 4:2:0 32x32 tiles */
    public static readonly uint V4L2_PIX_FMT_NV15_4L4 = v4l2_fourcc('V', 'T', '1', '5');  /* 15 Y/CbCr 4:2:0 10-bit 4x4 tiles */
    public static readonly uint V4L2_PIX_FMT_P010_4L4 = v4l2_fourcc('T', '0', '1', '0');  /* 12  Y/CbCr 4:2:0 10-bit 4x4 macroblocks */
    public static readonly uint V4L2_PIX_FMT_NV12_8L128 = v4l2_fourcc('A', 'T', '1', '2');  /* Y/CbCr 4:2:0 8x128 tiles */
    public static readonly uint V4L2_PIX_FMT_NV12_10BE_8L128 = v4l2_fourcc_be('A', 'X', '1', '2'); /* Y/CbCr 4:2:0 10-bit 8x128 tiles */

    /* Tiled YUV formats, non contiguous planes */
    public static readonly uint V4L2_PIX_FMT_NV12MT = v4l2_fourcc('T', 'M', '1', '2');  /* 12  Y/CbCr 4:2:0 64x32 tiles */
    public static readonly uint V4L2_PIX_FMT_NV12MT_16X16 = v4l2_fourcc('V', 'M', '1', '2');  /* 12  Y/CbCr 4:2:0 16x16 tiles */
    public static readonly uint V4L2_PIX_FMT_NV12M_8L128 = v4l2_fourcc('N', 'A', '1', '2');  /* Y/CbCr 4:2:0 8x128 tiles */
    public static readonly uint V4L2_PIX_FMT_NV12M_10BE_8L128 = v4l2_fourcc_be('N', 'T', '1', '2'); /* Y/CbCr 4:2:0 10-bit 8x128 tiles */

    /* Bayer formats - see http://www.siliconimaging.com/RGB%20Bayer.htm */
    public static readonly uint V4L2_PIX_FMT_SBGGR8 = v4l2_fourcc('B', 'A', '8', '1');  /*  8  BGBG.. GRGR.. */
    public static readonly uint V4L2_PIX_FMT_SGBRG8 = v4l2_fourcc('G', 'B', 'R', 'G');  /*  8  GBGB.. RGRG.. */
    public static readonly uint V4L2_PIX_FMT_SGRBG8 = v4l2_fourcc('G', 'R', 'B', 'G');  /*  8  GRGR.. BGBG.. */
    public static readonly uint V4L2_PIX_FMT_SRGGB8 = v4l2_fourcc('R', 'G', 'G', 'B');  /*  8  RGRG.. GBGB.. */
    public static readonly uint V4L2_PIX_FMT_SBGGR10 = v4l2_fourcc('B', 'G', '1', '0');  /* 10  BGBG.. GRGR.. */
    public static readonly uint V4L2_PIX_FMT_SGBRG10 = v4l2_fourcc('G', 'B', '1', '0');  /* 10  GBGB.. RGRG.. */
    public static readonly uint V4L2_PIX_FMT_SGRBG10 = v4l2_fourcc('B', 'A', '1', '0');  /* 10  GRGR.. BGBG.. */
    public static readonly uint V4L2_PIX_FMT_SRGGB10 = v4l2_fourcc('R', 'G', '1', '0');  /* 10  RGRG.. GBGB.. */
    /* 10bit raw bayer packed, 5 bytes for every 4 pixels */
    public static readonly uint V4L2_PIX_FMT_SBGGR10P = v4l2_fourcc('p', 'B', 'A', 'A');
    public static readonly uint V4L2_PIX_FMT_SGBRG10P = v4l2_fourcc('p', 'G', 'A', 'A');
    public static readonly uint V4L2_PIX_FMT_SGRBG10P = v4l2_fourcc('p', 'g', 'A', 'A');
    public static readonly uint V4L2_PIX_FMT_SRGGB10P = v4l2_fourcc('p', 'R', 'A', 'A');
    /* 10bit raw bayer a-law compressed to 8 bits */
    public static readonly uint V4L2_PIX_FMT_SBGGR10ALAW8 = v4l2_fourcc('a', 'B', 'A', '8');
    public static readonly uint V4L2_PIX_FMT_SGBRG10ALAW8 = v4l2_fourcc('a', 'G', 'A', '8');
    public static readonly uint V4L2_PIX_FMT_SGRBG10ALAW8 = v4l2_fourcc('a', 'g', 'A', '8');
    public static readonly uint V4L2_PIX_FMT_SRGGB10ALAW8 = v4l2_fourcc('a', 'R', 'A', '8');
    /* 10bit raw bayer DPCM compressed to 8 bits */
    public static readonly uint V4L2_PIX_FMT_SBGGR10DPCM8 = v4l2_fourcc('b', 'B', 'A', '8');
    public static readonly uint V4L2_PIX_FMT_SGBRG10DPCM8 = v4l2_fourcc('b', 'G', 'A', '8');
    public static readonly uint V4L2_PIX_FMT_SGRBG10DPCM8 = v4l2_fourcc('B', 'D', '1', '0');
    public static readonly uint V4L2_PIX_FMT_SRGGB10DPCM8 = v4l2_fourcc('b', 'R', 'A', '8');
    public static readonly uint V4L2_PIX_FMT_SBGGR12 = v4l2_fourcc('B', 'G', '1', '2');  /* 12  BGBG.. GRGR.. */
    public static readonly uint V4L2_PIX_FMT_SGBRG12 = v4l2_fourcc('G', 'B', '1', '2');  /* 12  GBGB.. RGRG.. */
    public static readonly uint V4L2_PIX_FMT_SGRBG12 = v4l2_fourcc('B', 'A', '1', '2');  /* 12  GRGR.. BGBG.. */
    public static readonly uint V4L2_PIX_FMT_SRGGB12 = v4l2_fourcc('R', 'G', '1', '2');  /* 12  RGRG.. GBGB.. */
    /* 12bit raw bayer packed, 3 bytes for every 2 pixels */
    public static readonly uint V4L2_PIX_FMT_SBGGR12P = v4l2_fourcc('p', 'B', 'C', 'C');
    public static readonly uint V4L2_PIX_FMT_SGBRG12P = v4l2_fourcc('p', 'G', 'C', 'C');
    public static readonly uint V4L2_PIX_FMT_SGRBG12P = v4l2_fourcc('p', 'g', 'C', 'C');
    public static readonly uint V4L2_PIX_FMT_SRGGB12P = v4l2_fourcc('p', 'R', 'C', 'C');
    public static readonly uint V4L2_PIX_FMT_SBGGR14 = v4l2_fourcc('B', 'G', '1', '4');  /* 14  BGBG.. GRGR.. */
    public static readonly uint V4L2_PIX_FMT_SGBRG14 = v4l2_fourcc('G', 'B', '1', '4');  /* 14  GBGB.. RGRG.. */
    public static readonly uint V4L2_PIX_FMT_SGRBG14 = v4l2_fourcc('G', 'R', '1', '4');  /* 14  GRGR.. BGBG.. */
    public static readonly uint V4L2_PIX_FMT_SRGGB14 = v4l2_fourcc('R', 'G', '1', '4');  /* 14  RGRG.. GBGB.. */
    /* 14bit raw bayer packed, 7 bytes for every 4 pixels */
    public static readonly uint V4L2_PIX_FMT_SBGGR14P = v4l2_fourcc('p', 'B', 'E', 'E');
    public static readonly uint V4L2_PIX_FMT_SGBRG14P = v4l2_fourcc('p', 'G', 'E', 'E');
    public static readonly uint V4L2_PIX_FMT_SGRBG14P = v4l2_fourcc('p', 'g', 'E', 'E');
    public static readonly uint V4L2_PIX_FMT_SRGGB14P = v4l2_fourcc('p', 'R', 'E', 'E');
    public static readonly uint V4L2_PIX_FMT_SBGGR16 = v4l2_fourcc('B', 'Y', 'R', '2');  /* 16  BGBG.. GRGR.. */
    public static readonly uint V4L2_PIX_FMT_SGBRG16 = v4l2_fourcc('G', 'B', '1', '6');  /* 16  GBGB.. RGRG.. */
    public static readonly uint V4L2_PIX_FMT_SGRBG16 = v4l2_fourcc('G', 'R', '1', '6');  /* 16  GRGR.. BGBG.. */
    public static readonly uint V4L2_PIX_FMT_SRGGB16 = v4l2_fourcc('R', 'G', '1', '6');  /* 16  RGRG.. GBGB.. */

    /* HSV formats */
    public static readonly uint V4L2_PIX_FMT_HSV24 = v4l2_fourcc('H', 'S', 'V', '3');
    public static readonly uint V4L2_PIX_FMT_HSV32 = v4l2_fourcc('H', 'S', 'V', '4');

    /* compressed formats */
    public static readonly uint V4L2_PIX_FMT_MJPEG = v4l2_fourcc('M', 'J', 'P', 'G');  /* Motion-JPEG   */
    public static readonly uint V4L2_PIX_FMT_JPEG = v4l2_fourcc('J', 'P', 'E', 'G');  /* JFIF JPEG     */
    public static readonly uint V4L2_PIX_FMT_DV = v4l2_fourcc('d', 'v', 's', 'd');  /* 1394          */
    public static readonly uint V4L2_PIX_FMT_MPEG = v4l2_fourcc('M', 'P', 'E', 'G');  /* MPEG-1/2/4 Multiplexed */
    public static readonly uint V4L2_PIX_FMT_H264 = v4l2_fourcc('H', '2', '6', '4');  /* H264 with start codes */
    public static readonly uint V4L2_PIX_FMT_H264_NO_SC = v4l2_fourcc('A', 'V', 'C', '1');  /* H264 without start codes */
    public static readonly uint V4L2_PIX_FMT_H264_MVC = v4l2_fourcc('M', '2', '6', '4');  /* H264 MVC */
    public static readonly uint V4L2_PIX_FMT_H263 = v4l2_fourcc('H', '2', '6', '3');  /* H263          */
    public static readonly uint V4L2_PIX_FMT_MPEG1 = v4l2_fourcc('M', 'P', 'G', '1');  /* MPEG-1 ES     */
    public static readonly uint V4L2_PIX_FMT_MPEG2 = v4l2_fourcc('M', 'P', 'G', '2');  /* MPEG-2 ES     */
    public static readonly uint V4L2_PIX_FMT_MPEG2_SLICE = v4l2_fourcc('M', 'G', '2', 'S');  /* MPEG-2 parsed slice data */
    public static readonly uint V4L2_PIX_FMT_MPEG4 = v4l2_fourcc('M', 'P', 'G', '4');  /* MPEG-4 part 2 ES */
    public static readonly uint V4L2_PIX_FMT_XVID = v4l2_fourcc('X', 'V', 'I', 'D');  /* Xvid           */
    public static readonly uint V4L2_PIX_FMT_VC1_ANNEX_G = v4l2_fourcc('V', 'C', '1', 'G');  /* SMPTE 421M Annex G compliant stream */
    public static readonly uint V4L2_PIX_FMT_VC1_ANNEX_L = v4l2_fourcc('V', 'C', '1', 'L');  /* SMPTE 421M Annex L compliant stream */
    public static readonly uint V4L2_PIX_FMT_VP8 = v4l2_fourcc('V', 'P', '8', '0');  /* VP8 */
    public static readonly uint V4L2_PIX_FMT_VP8_FRAME = v4l2_fourcc('V', 'P', '8', 'F');  /* VP8 parsed frame */
    public static readonly uint V4L2_PIX_FMT_VP9 = v4l2_fourcc('V', 'P', '9', '0');  /* VP9 */
    public static readonly uint V4L2_PIX_FMT_VP9_FRAME = v4l2_fourcc('V', 'P', '9', 'F');  /* VP9 parsed frame */
    public static readonly uint V4L2_PIX_FMT_HEVC = v4l2_fourcc('H', 'E', 'V', 'C');  /* HEVC aka H.265 */
    public static readonly uint V4L2_PIX_FMT_FWHT = v4l2_fourcc('F', 'W', 'H', 'T'); /* Fast Walsh Hadamard Transform (vicodec);  */
    public static readonly uint V4L2_PIX_FMT_FWHT_STATELESS = v4l2_fourcc('S', 'F', 'W', 'H'); /* Stateless FWHT (vicodec);  */
    public static readonly uint V4L2_PIX_FMT_H264_SLICE = v4l2_fourcc('S', '2', '6', '4');  /* H264 parsed slices */
    public static readonly uint V4L2_PIX_FMT_HEVC_SLICE = v4l2_fourcc('S', '2', '6', '5');  /* HEVC parsed slices */
    public static readonly uint V4L2_PIX_FMT_AV1_FRAME = v4l2_fourcc('A', 'V', '1', 'F');  /* AV1 parsed frame */
    public static readonly uint V4L2_PIX_FMT_AV1 = v4l2_fourcc('A', 'V', '0', '1');  /* AV1 */
    public static readonly uint V4L2_PIX_FMT_SPK = v4l2_fourcc('S', 'P', 'K', '0');  /* Sorenson Spark */
    public static readonly uint V4L2_PIX_FMT_RV30 = v4l2_fourcc('R', 'V', '3', '0');  /* RealVideo 8 */
    public static readonly uint V4L2_PIX_FMT_RV40 = v4l2_fourcc('R', 'V', '4', '0');  /* RealVideo 9 & 10 */

    /*  Vendor-specific formats   */
    public static readonly uint V4L2_PIX_FMT_CPIA1 = v4l2_fourcc('C', 'P', 'I', 'A');  /* cpia1 YUV */
    public static readonly uint V4L2_PIX_FMT_WNVA = v4l2_fourcc('W', 'N', 'V', 'A');  /* Winnov hw compress */
    public static readonly uint V4L2_PIX_FMT_SN9C10X = v4l2_fourcc('S', '9', '1', '0');  /* SN9C10x compression */
    public static readonly uint V4L2_PIX_FMT_SN9C20X_I420 = v4l2_fourcc('S', '9', '2', '0');  /* SN9C20x YUV 4:2:0 */
    public static readonly uint V4L2_PIX_FMT_PWC1 = v4l2_fourcc('P', 'W', 'C', '1');  /* pwc older webcam */
    public static readonly uint V4L2_PIX_FMT_PWC2 = v4l2_fourcc('P', 'W', 'C', '2');  /* pwc newer webcam */
    public static readonly uint V4L2_PIX_FMT_ET61X251 = v4l2_fourcc('E', '6', '2', '5');  /* ET61X251 compression */
    public static readonly uint V4L2_PIX_FMT_SPCA501 = v4l2_fourcc('S', '5', '0', '1');  /* YUYV per line */
    public static readonly uint V4L2_PIX_FMT_SPCA505 = v4l2_fourcc('S', '5', '0', '5');  /* YYUV per line */
    public static readonly uint V4L2_PIX_FMT_SPCA508 = v4l2_fourcc('S', '5', '0', '8');  /* YUVY per line */
    public static readonly uint V4L2_PIX_FMT_SPCA561 = v4l2_fourcc('S', '5', '6', '1');  /* compressed GBRG bayer */
    public static readonly uint V4L2_PIX_FMT_PAC207 = v4l2_fourcc('P', '2', '0', '7');  /* compressed BGGR bayer */
    public static readonly uint V4L2_PIX_FMT_MR97310A = v4l2_fourcc('M', '3', '1', '0');  /* compressed BGGR bayer */
    public static readonly uint V4L2_PIX_FMT_JL2005BCD = v4l2_fourcc('J', 'L', '2', '0');  /* compressed RGGB bayer */
    public static readonly uint V4L2_PIX_FMT_SN9C2028 = v4l2_fourcc('S', 'O', 'N', 'X');  /* compressed GBRG bayer */
    public static readonly uint V4L2_PIX_FMT_SQ905C = v4l2_fourcc('9', '0', '5', 'C');  /* compressed RGGB bayer */
    public static readonly uint V4L2_PIX_FMT_PJPG = v4l2_fourcc('P', 'J', 'P', 'G');  /* Pixart 73xx JPEG */
    public static readonly uint V4L2_PIX_FMT_OV511 = v4l2_fourcc('O', '5', '1', '1');  /* ov511 JPEG */
    public static readonly uint V4L2_PIX_FMT_OV518 = v4l2_fourcc('O', '5', '1', '8');  /* ov518 JPEG */
    public static readonly uint V4L2_PIX_FMT_STV0680 = v4l2_fourcc('S', '6', '8', '0');  /* stv0680 bayer */
    public static readonly uint V4L2_PIX_FMT_TM6000 = v4l2_fourcc('T', 'M', '6', '0');  /* tm5600/tm60x0 */
    public static readonly uint V4L2_PIX_FMT_CIT_YYVYUY = v4l2_fourcc('C', 'I', 'T', 'V');  /* one line of Y then 1 line of VYUY */
    public static readonly uint V4L2_PIX_FMT_KONICA420 = v4l2_fourcc('K', 'O', 'N', 'I');  /* YUV420 planar in blocks of 256 pixels */
    public static readonly uint V4L2_PIX_FMT_JPGL = v4l2_fourcc('J', 'P', 'G', 'L');  /* JPEG-Lite */
    public static readonly uint V4L2_PIX_FMT_SE401 = v4l2_fourcc('S', '4', '0', '1');  /* se401 janggu compressed rgb */
    public static readonly uint V4L2_PIX_FMT_S5C_UYVY_JPG = v4l2_fourcc('S', '5', 'C', 'I');  /* S5C73M3 interleaved UYVY/JPEG */
    public static readonly uint V4L2_PIX_FMT_Y8I = v4l2_fourcc('Y', '8', 'I', ' ');  /* Greyscale 8-bit L/R interleaved */
    public static readonly uint V4L2_PIX_FMT_Y12I = v4l2_fourcc('Y', '1', '2', 'I');  /* Greyscale 12-bit L/R interleaved */
    public static readonly uint V4L2_PIX_FMT_Y16I = v4l2_fourcc('Y', '1', '6', 'I');  /* Greyscale 16-bit L/R interleaved */
    public static readonly uint V4L2_PIX_FMT_Z16 = v4l2_fourcc('Z', '1', '6', ' ');  /* Depth data 16-bit */
    public static readonly uint V4L2_PIX_FMT_MT21C = v4l2_fourcc('M', 'T', '2', '1');  /* Mediatek compressed block mode  */
    public static readonly uint V4L2_PIX_FMT_MM21 = v4l2_fourcc('M', 'M', '2', '1');  /* Mediatek 8-bit block mode, two non-contiguous planes */
    public static readonly uint V4L2_PIX_FMT_MT2110T = v4l2_fourcc('M', 'T', '2', 'T');  /* Mediatek 10-bit block tile mode */
    public static readonly uint V4L2_PIX_FMT_MT2110R = v4l2_fourcc('M', 'T', '2', 'R');  /* Mediatek 10-bit block raster mode */
    public static readonly uint V4L2_PIX_FMT_INZI = v4l2_fourcc('I', 'N', 'Z', 'I');  /* Intel Planar Greyscale 10-bit and Depth 16-bit */
    public static readonly uint V4L2_PIX_FMT_CNF4 = v4l2_fourcc('C', 'N', 'F', '4');  /* Intel 4-bit packed depth confidence information */
    public static readonly uint V4L2_PIX_FMT_HI240 = v4l2_fourcc('H', 'I', '2', '4');  /* BTTV 8-bit dithered RGB */
    public static readonly uint V4L2_PIX_FMT_QC08C = v4l2_fourcc('Q', '0', '8', 'C');  /* Qualcomm 8-bit compressed */
    public static readonly uint V4L2_PIX_FMT_QC10C = v4l2_fourcc('Q', '1', '0', 'C');  /* Qualcomm 10-bit compressed */
    public static readonly uint V4L2_PIX_FMT_AJPG = v4l2_fourcc('A', 'J', 'P', 'G');  /* Aspeed JPEG */
    public static readonly uint V4L2_PIX_FMT_HEXTILE = v4l2_fourcc('H', 'X', 'T', 'L');  /* Hextile compressed */

    /* 10bit raw packed, 32 bytes for every 25 pixels, last LSB 6 bits unused */
    public static readonly uint V4L2_PIX_FMT_IPU3_SBGGR10 = v4l2_fourcc('i', 'p', '3', 'b');  /* IPU3 packed 10-bit BGGR bayer */
    public static readonly uint V4L2_PIX_FMT_IPU3_SGBRG10 = v4l2_fourcc('i', 'p', '3', 'g');  /* IPU3 packed 10-bit GBRG bayer */
    public static readonly uint V4L2_PIX_FMT_IPU3_SGRBG10 = v4l2_fourcc('i', 'p', '3', 'G');  /* IPU3 packed 10-bit GRBG bayer */
    public static readonly uint V4L2_PIX_FMT_IPU3_SRGGB10 = v4l2_fourcc('i', 'p', '3', 'r');  /* IPU3 packed 10-bit RGGB bayer */

    /* Raspberry Pi PiSP compressed formats. */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP1_RGGB = v4l2_fourcc('P', 'C', '1', 'R');  /* PiSP 8-bit mode 1 compressed RGGB bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP1_GRBG = v4l2_fourcc('P', 'C', '1', 'G');  /* PiSP 8-bit mode 1 compressed GRBG bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP1_GBRG = v4l2_fourcc('P', 'C', '1', 'g');  /* PiSP 8-bit mode 1 compressed GBRG bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP1_BGGR = v4l2_fourcc('P', 'C', '1', 'B');  /* PiSP 8-bit mode 1 compressed BGGR bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP1_MONO = v4l2_fourcc('P', 'C', '1', 'M');  /* PiSP 8-bit mode 1 compressed monochrome */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP2_RGGB = v4l2_fourcc('P', 'C', '2', 'R');  /* PiSP 8-bit mode 2 compressed RGGB bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP2_GRBG = v4l2_fourcc('P', 'C', '2', 'G');  /* PiSP 8-bit mode 2 compressed GRBG bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP2_GBRG = v4l2_fourcc('P', 'C', '2', 'g');  /* PiSP 8-bit mode 2 compressed GBRG bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP2_BGGR = v4l2_fourcc('P', 'C', '2', 'B');  /* PiSP 8-bit mode 2 compressed BGGR bayer */
    public static readonly uint V4L2_PIX_FMT_PISP_COMP2_MONO = v4l2_fourcc('P', 'C', '2', 'M');  /* PiSP 8-bit mode 2 compressed monochrome */

    /* Renesas RZ/V2H CRU packed formats. 64-bit units with contiguous pixels */
    public static readonly uint V4L2_PIX_FMT_RAW_CRU10 = v4l2_fourcc('C', 'R', '1', '0');
    public static readonly uint V4L2_PIX_FMT_RAW_CRU12 = v4l2_fourcc('C', 'R', '1', '2');
    public static readonly uint V4L2_PIX_FMT_RAW_CRU14 = v4l2_fourcc('C', 'R', '1', '4');
    public static readonly uint V4L2_PIX_FMT_RAW_CRU20 = v4l2_fourcc('C', 'R', '2', '0');

    /* SDR formats - used only for Software Defined Radio devices */
    public static readonly uint V4L2_SDR_FMT_CU8 = v4l2_fourcc('C', 'U', '0', '8');  /* IQ u8 */
    public static readonly uint V4L2_SDR_FMT_CU16LE = v4l2_fourcc('C', 'U', '1', '6');  /* IQ u16le */
    public static readonly uint V4L2_SDR_FMT_CS8 = v4l2_fourcc('C', 'S', '0', '8');  /* complex s8 */
    public static readonly uint V4L2_SDR_FMT_CS14LE = v4l2_fourcc('C', 'S', '1', '4');  /* complex s14le */
    public static readonly uint V4L2_SDR_FMT_RU12LE = v4l2_fourcc('R', 'U', '1', '2');  /* real u12le */
    public static readonly uint V4L2_SDR_FMT_PCU16BE = v4l2_fourcc('P', 'C', '1', '6');  /* planar complex u16be */
    public static readonly uint V4L2_SDR_FMT_PCU18BE = v4l2_fourcc('P', 'C', '1', '8');  /* planar complex u18be */
    public static readonly uint V4L2_SDR_FMT_PCU20BE = v4l2_fourcc('P', 'C', '2', '0');  /* planar complex u20be */

    /* Touch formats - used for Touch devices */
    public static readonly uint V4L2_TCH_FMT_DELTA_TD16 = v4l2_fourcc('T', 'D', '1', '6');  /* 16-bit signed deltas */
    public static readonly uint V4L2_TCH_FMT_DELTA_TD08 = v4l2_fourcc('T', 'D', '0', '8');  /* 8-bit signed deltas */
    public static readonly uint V4L2_TCH_FMT_TU16 = v4l2_fourcc('T', 'U', '1', '6');  /* 16-bit unsigned touch data */
    public static readonly uint V4L2_TCH_FMT_TU08 = v4l2_fourcc('T', 'U', '0', '8');  /* 8-bit unsigned touch data */

    /* Meta-data formats */
    public static readonly uint V4L2_META_FMT_VSP1_HGO = v4l2_fourcc('V', 'S', 'P', 'H');  /* R-Car VSP1 1-D Histogram */
    public static readonly uint V4L2_META_FMT_VSP1_HGT = v4l2_fourcc('V', 'S', 'P', 'T');  /* R-Car VSP1 2-D Histogram */
    public static readonly uint V4L2_META_FMT_UVC = v4l2_fourcc('U', 'V', 'C', 'H');  /* UVC Payload Header metadata */
    public static readonly uint V4L2_META_FMT_D4XX = v4l2_fourcc('D', '4', 'X', 'X');  /* D4XX Payload Header metadata */
    public static readonly uint V4L2_META_FMT_UVC_MSXU_1_5 = v4l2_fourcc('U', 'V', 'C', 'M');  /* UVC MSXU metadata */
    public static readonly uint V4L2_META_FMT_VIVID = v4l2_fourcc('V', 'I', 'V', 'D');  /* Vivid Metadata */

    /* Vendor specific - used for RK_ISP1 camera sub-system */
    public static readonly uint V4L2_META_FMT_RK_ISP1_PARAMS = v4l2_fourcc('R', 'K', '1', 'P');  /* Rockchip ISP1 3A Parameters */
    public static readonly uint V4L2_META_FMT_RK_ISP1_STAT_3A = v4l2_fourcc('R', 'K', '1', 'S');  /* Rockchip ISP1 3A Statistics */
    public static readonly uint V4L2_META_FMT_RK_ISP1_EXT_PARAMS = v4l2_fourcc('R', 'K', '1', 'E');  /* Rockchip ISP1 3a Extensible Parameters */

    /* Vendor specific - used for C3_ISP */
    public static readonly uint V4L2_META_FMT_C3ISP_PARAMS = v4l2_fourcc('C', '3', 'P', 'M');  /* Amlogic C3 ISP Parameters */
    public static readonly uint V4L2_META_FMT_C3ISP_STATS = v4l2_fourcc('C', '3', 'S', 'T');  /* Amlogic C3 ISP Statistics */

    /* Vendor specific - used for RaspberryPi PiSP */
    public static readonly uint V4L2_META_FMT_RPI_BE_CFG = v4l2_fourcc('R', 'P', 'B', 'C');  /* PiSP BE configuration */
    public static readonly uint V4L2_META_FMT_RPI_FE_CFG = v4l2_fourcc('R', 'P', 'F', 'C');  /* PiSP FE configuration */
    public static readonly uint V4L2_META_FMT_RPI_FE_STATS = v4l2_fourcc('R', 'P', 'F', 'S');  /* PiSP FE stats */

    /* Vendor specific - used for Arm Mali-C55 ISP */
    public static readonly uint V4L2_META_FMT_MALI_C55_PARAMS = v4l2_fourcc('C', '5', '5', 'P');  /* ARM Mali-C55 Parameters */
    public static readonly uint V4L2_META_FMT_MALI_C55_STATS = v4l2_fourcc('C', '5', '5', 'S');  /* ARM Mali-C55 3A Statistics */
}