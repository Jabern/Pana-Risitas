

using Launcher.Utils;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Launcher
{
  internal class Game
  {
    private const uint DETACHED_PROCESS = 8;
    private const uint CREATE_SUSPENDED = 4;
    private const uint MEM_RELEASE = 32768;
    private const int INFINITE = -1;
    private const int WAIT_ABANDONED = 128;
    private const int WAIT_OBJECT_0 = 0;
    private const int WAIT_TIMEOUT = 258;
    private const int WAIT_FAILED = -1;

    public static bool LaunchGTAInjected(string ip, string port, string nickname, string password = "", bool debug = false, string sampdll = "\\samp.dll")
    {
      string str = Registry.getValue("gta_sa");
      string lpApplicationName = str + "\\gta_sa.exe";
      Game.PROCESS_INFORMATION lpProcessInformation = new Game.PROCESS_INFORMATION();
      Game.STARTUPINFO lpStartupInfo = new Game.STARTUPINFO();
      string lpCommandLine;
      if (!debug)
        lpCommandLine = "-c -h " + ip + " -p " + port + " -n " + nickname;
      else
        lpCommandLine = "-d -h " + ip + " -p " + port + " -n " + nickname;
      if (password.Length != 0)
        lpCommandLine = lpCommandLine + " -z " + password;
      Console.WriteLine(lpApplicationName + " " + lpCommandLine);
      if (Game.NativeMethods.CreateProcess(lpApplicationName, lpCommandLine, IntPtr.Zero, IntPtr.Zero, false, 12U, IntPtr.Zero, str, ref lpStartupInfo, out lpProcessInformation))
      {
        Game.InjectDLL(lpProcessInformation, str, sampdll);
        Game.NativeMethods.ResumeThread(lpProcessInformation.hThread);
        Game.NativeMethods.WaitForSingleObject(lpProcessInformation.hProcess, -1);
        Game.NativeMethods.CloseHandle(lpProcessInformation.hProcess);
      }
      return true;
    }

    private static bool InjectDLL(Game.PROCESS_INFORMATION ProcessInfo, string gtaDir, string dll)
    {
      byte[] bytes = new ASCIIEncoding().GetBytes(gtaDir + dll);
      Console.WriteLine(gtaDir + dll);
      IntPtr hModule = Game.NativeMethods.LoadLibraryA("kernel32.dll");
      string procName = "LoadLibraryA";
      UIntPtr procAddress = Game.NativeMethods.GetProcAddress(hModule, procName);
      Game.NativeMethods.FreeLibrary(hModule);
      if (procAddress == UIntPtr.Zero)
        return false;
      IntPtr hProcess = ProcessInfo.hProcess;
      if (hProcess == IntPtr.Zero)
        return false;
      IntPtr num = Game.NativeMethods.VirtualAllocEx(hProcess, (IntPtr) 0, (uint) bytes.Length, 12288U, 4U);
      UIntPtr lpNumberOfBytesWritten;
      if (num == IntPtr.Zero || !Game.NativeMethods.WriteProcessMemory(hProcess, num, bytes, (uint) bytes.Length, out lpNumberOfBytesWritten))
        return false;
      IntPtr lpThreadId;
      IntPtr remoteThread = Game.NativeMethods.CreateRemoteThread(hProcess, (IntPtr) 0, 0U, procAddress, num, 4U, out lpThreadId);
      if (remoteThread == IntPtr.Zero)
        return false;
      Game.NativeMethods.ResumeThread(remoteThread);
      Game.NativeMethods.WaitForSingleObject(remoteThread, int.MaxValue);
      Game.NativeMethods.VirtualFreeEx(ProcessInfo.hProcess, num, UIntPtr.Zero, 32768U);
      return true;
    }

    public struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public uint dwProcessId;
      public uint dwThreadId;
    }

    public struct STARTUPINFO
    {
      public uint cb;
      public string lpReserved;
      public string lpDesktop;
      public string lpTitle;
      public uint dwX;
      public uint dwY;
      public uint dwXSize;
      public uint dwYSize;
      public uint dwXCountChars;
      public uint dwYCountChars;
      public uint dwFillAttribute;
      public uint dwFlags;
      public short wShowWindow;
      public short cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    public struct SECURITY_ATTRIBUTES
    {
      public int length;
      public IntPtr lpSecurityDescriptor;
      public bool bInheritHandle;
    }

    internal static class NativeMethods
    {
      [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
      internal static extern IntPtr LoadLibraryA(string lpFileName);

      [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
      internal static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

      [DllImport("kernel32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool FreeLibrary(IntPtr hModule);

      [DllImport("kernel32.dll")]
      internal static extern IntPtr OpenProcess(Game.NativeMethods.ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

      [DllImport("kernel32.dll")]
      internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

      [DllImport("kernel32", SetLastError = true)]
      internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

      [DllImport("kernel32.dll")]
      internal static extern int CloseHandle(IntPtr hObject);

      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

      [DllImport("kernel32.dll")]
      internal static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Game.STARTUPINFO lpStartupInfo, out Game.PROCESS_INFORMATION lpProcessInformation);

      [DllImport("kernel32.dll")]
      internal static extern int ResumeThread(IntPtr hThread);

      [Flags]
      public enum ProcessAccess
      {
        AllAccess = 1050235,
        CreateThread = 2,
        DuplicateHandle = 64,
        QueryInformation = 1024,
        SetInformation = 512,
        Terminate = 1,
        VMOperation = 8,
        VMRead = 16,
        VMWrite = 32,
        Synchronize = 1048576,
      }
    }
  }
}
