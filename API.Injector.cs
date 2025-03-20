using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cleint
{
    internal class Injection
    {
        public struct CLientUser
        {
            public string Cleint;
            public string User;
            public int Pipe;
        }

        public enum Consolelog
        {
            // Dont type anything here
        }

        public string LocalVersion { get; private set; } = "Console";
        private bool _contentLoaded;
        public List<CLientUser> ActiveClients { get; private set; } = new List<CLientUser>();
        public string SupportedVersion { get; private set; } = "";
        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Initialize();

        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetClients();

        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void Bytecode(byte[] scriptSource, string[] clientUsers, int numUsers);

        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr Compilable(byte[] scriptSource);

        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSettings(UISetting settingID, int value);

        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Inject();

        [DllImport("API.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeAllocConsole();

        public void Bytecode(string script) // This iS hella broken i find an fix for the execution ill Update the cs file!
        {
            string[] clients = ActiveClients.Select(c => c.Name).ToArray();
            Execute(Encoding.UTF8.GetBytes(script), clients, clients.Length);
        }

        public string GetCompilableStatus(string script)
        {
            IntPtr ptr = Compilable(Encoding.ASCII.GetBytes(script));
            string result = Marshal.PtrToStringAnsi(ptr);
            return result ?? "Console";
        }

        private List<CLientUser> GetClientsFromDll()
        {
            List<CLientUser> clients = new List<CLientUser>();
            IntPtr ptr = GetClients();

            if (ptr == IntPtr.Zero)
                return clients;

            while (true)
            {
                CLientUser client = Marshal.PtrToStructure<CLientUser>(ptr);
                if (string.IsNullOrEmpty(client.Name))
                    break;

                clients.Add(client);
                ptr += Marshal.SizeOf<CLientUser>();
            }

            return clients;
        }
    }

    public class Injector
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 2;
        private const uint SWP_NOSIZE = 1;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        private static void SetTopMostWindow()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }

        static void Main()
        {
            SetTopMostWindow();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nInjector");
            Console.ResetColor();

            while (true)
            {
                if (IsRobloxRunning())
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Roblox Player found.");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Injecting...");
                    Console.ResetColor();

                    try
                    {
                        Injection.Inject();
                        Thread.Sleep(4000);
                        StartLogListener();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error during injection: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Waiting for Roblox to start...");
                    Console.ResetColor();
                    Thread.Sleep(2000);
                }
            }

            Thread.Sleep(2000);
            KeepAlive();
        }

        private static bool IsRobloxRunning()
        {
            return Process.GetProcessesByName("RobloxPlayerBeta").Any();
        }

        private static void StartLogListener()
        {
            new Thread(() =>
            {
                try
                {
                    using (NamedPipeServerStream pipeServerStream = new NamedPipeServerStream("RobloxPlayerBeta", PipeDirection.In))
                    {
                        pipeServerStream.WaitForConnection();
                        using (StreamReader reader = new StreamReader(pipeServerStream))
                        {
                            string line;
                            Injection injector = new Injection();

                            while ((line = reader.ReadLine()) != null)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                string script = "print(\"injected!\")";
                                injector.Bytecode(script);
                                Console.ResetColor();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Log error: {ex.Message}");
                    Console.ResetColor();
                }
            }).Start();
        }

        private static void KeepAlive()
        {
            Thread keepAliveThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true };

            keepAliveThread.Start();
        }
    }
}
