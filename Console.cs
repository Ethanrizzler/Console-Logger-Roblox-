using System;
using System.Diagnostics;
using EasyHook; // Get this Via Nuget

namespace Capture
{
    class Program
    {
        public delegate void OutputDebugStringWDelegate(string message);

        class Hook : IEntryPoint
        {
            public Hook(RemoteHooking.IpcServerChannel channel)
            {
                LocalHook hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "OutputDebugStringW"),
                    new OutputDebugStringWDelegate(OutputDebugStringWHook),
                    this);
                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
                Console.WriteLine("Hooked");
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);  
                }
            }
            public void OutputDebugStringWHook(string message)
            {
                Console.WriteLine("Output:" + message);
            }
            public void Run(string channelName)
            {
            }
        }

        static void Main(string[] args)
        {
            string channelName = null;

            try
            {
                Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
                if (processes.Length == 0)
                {
                    Console.WriteLine("RobloxPlayerBeta isnt open");
                    return;
                }
                string dllPath = "API.dll";
                RemoteHooking.IpcCreateServer<Hook>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);
                RemoteHooking.Inject(processes[0].Id, dllPath, dllPath, channelName);
                Console.WriteLine("Connecting to the Output");
                Console.ReadKey(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: ?" + ex.Message);
            }
        }
    }
}
