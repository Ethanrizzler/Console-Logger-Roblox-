using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RobloxOutPut;

namespace RobloxOutPut
{
    class RobloxOutPut
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern void OutputDebugStringW(string lpOutputString);
        private static IntPtr _originalFunctionPointer = IntPtr.Zero;
        private delegate void OutputDebugStringWDelegate(string lpOutputString);
        private static OutputDebugStringWDelegate _hookedFunction;
        private static void HookedOutputDebugString(string message)
        {
            Console.WriteLine("Dectected Log: " + message);
            Marshal.GetDelegateForFunctionPointer<OutputDebugStringWDelegate>(_originalFunctionPointer)(message);
        }
        private static void InstallHook()
        {
            IntPtr kernel32Handle = GetModuleHandle("kernel32.dll");
            IntPtr functionAddress = GetProcAddress(kernel32Handle, "OutputDebugStringW");
            _originalFunctionPointer = functionAddress;
            _hookedFunction = new OutputDebugStringWDelegate(HookedOutputDebugString);
            IntPtr hookedFunctionPointer = Marshal.GetFunctionPointerForDelegate(_hookedFunction);
            WriteProcessMemory(GetCurrentProcess(), functionAddress, ref hookedFunctionPointer, IntPtr.Size, out _);
        }
        static void Main(string[] args)
        {
            Process[] nigger = Process.GetProcessesByName("RobloxPlayerBeta");
            if (nigger.Length == 0)
            {
                Console.WriteLine("Open roblox or Close roblox from task manager!");
                return;
            }

            Console.WriteLine("Hooked");
            InstallHook();

            Console.WriteLine("getting logs!");
            Console.ReadLine();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref IntPtr lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();
    }
}
