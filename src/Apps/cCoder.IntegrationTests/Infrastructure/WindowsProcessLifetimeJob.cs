// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace cCoder.IntegrationTests.Infrastructure;

internal sealed class WindowsProcessLifetimeJob : IDisposable
{
    private const uint KillOnJobClose = 0x00002000;
    private const int ExtendedLimitInformation = 9;
    private readonly SafeFileHandle jobHandle;

    public WindowsProcessLifetimeJob()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        jobHandle = CreateJobObject(jobAttributes: IntPtr.Zero, name: null);

        if (jobHandle.IsInvalid)
        {
            throw new Win32Exception(error: Marshal.GetLastWin32Error());
        }

        JobObjectExtendedLimitInformation information = new()
        {
            BasicLimitInformation = new JobObjectBasicLimitInformation
            {
                LimitFlags = KillOnJobClose
            }
        };

        int informationLength = Marshal.SizeOf<JobObjectExtendedLimitInformation>();
        IntPtr informationPointer = Marshal.AllocHGlobal(cb: informationLength);

        try
        {
            Marshal.StructureToPtr(structure: information, ptr: informationPointer, fDeleteOld: false);

            if (!SetInformationJobObject(
                job: jobHandle,
                informationClass: ExtendedLimitInformation,
                information: informationPointer,
                informationLength: (uint)informationLength))
            {
                throw new Win32Exception(error: Marshal.GetLastWin32Error());
            }
        }
        finally
        {
            Marshal.FreeHGlobal(hglobal: informationPointer);
        }
    }

    public void Add(Process process)
    {
        ArgumentNullException.ThrowIfNull(argument: process);

        if (jobHandle is null)
        {
            return;
        }

        if (!AssignProcessToJobObject(job: jobHandle, process: process.SafeHandle))
        {
            throw new Win32Exception(error: Marshal.GetLastWin32Error());
        }
    }

    public void Dispose() =>
        jobHandle?.Dispose();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateJobObject(
        IntPtr jobAttributes,
        string name);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetInformationJobObject(
        SafeFileHandle job,
        int informationClass,
        IntPtr information,
        uint informationLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AssignProcessToJobObject(
        SafeFileHandle job,
        SafeProcessHandle process);

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectBasicLimitInformation
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectExtendedLimitInformation
    {
        public JobObjectBasicLimitInformation BasicLimitInformation;
        public IoCounters IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }
}