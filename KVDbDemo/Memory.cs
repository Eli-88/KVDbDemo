using System.Runtime.InteropServices;

namespace KVDbDemo;


public static class OS
{
    public const int PROT_READ = 0x01;
    public const int PROT_WRITE = 0x02;
    public const int MAP_ANONYMOUS = 0x1000;
    public const int MAP_PRIVATE = 0x0002;
    public const IntPtr MAP_FAIL = -1;
    
    [DllImport("libSystem.dylib", SetLastError = true, EntryPoint = "mmap")]
    public static extern IntPtr Mmap(IntPtr addr, ulong len, int prot, int flags, int fd, ulong offset);
    
    [DllImport("libSystem.dylib", SetLastError = true, EntryPoint = "munmap")]
    public static extern int Munmap(IntPtr addr, ulong len);
}