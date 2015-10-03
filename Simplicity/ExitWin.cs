using System;
using System.Runtime.InteropServices;


namespace cm
{
	/// <summary>
	/// Used for shutting down and restarting Windows. This class is implemented as a sealed and
	/// should be treated as a singleton.
	/// </summary>
	/// <remarks>
	/// By Alexander Lowe, http://www.lowesoftware.com.
	///
	/// Free to modify and redistribute. Code is provided as is with no warantee.
	/// </remarks>
	public sealed class ExitWindows
	{
		/// <remarks />
		private ExitWindows() {}

		#region Public Methods
		/// <summary>
		/// Properly adjusts the shutdown privilege for this process and shuts down Windows.
		/// </summary>
		/// <param name="Method">The method to use for shutdown.</param>
		/// <returns>True on success, false on error.</returns>
		static public bool Shutdown(ShutdownMethod Method)
		{
			// Adjust the process privilege
			bool ret=AdjustShutdownTokenPriveleges();
			if(!ret)
				return false;

			// exit windows
			return ExitWindowsEx(Method, ShutdownReason.FlagPlanned);
		}


		/// <summary>
		/// Properly adjusts the shutdown privilege for this process and shuts down Windows.
		/// </summary>
		/// <param name="Method">The method to use for shutdown.</param>
		/// <param name="Reason">The reason for the shutdown action.</param>
		/// <returns>True on success, false on error.</returns>
		static public bool Shutdown(ShutdownMethod Method, ShutdownReason Reason)
		{
			// Adjust the process privilege
			bool ret=AdjustShutdownTokenPriveleges();
			if(!ret)
				return false;

			// exit windows
			return ExitWindowsEx(Method, Reason);
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Adjust the token of the current process to include shutdown privileges.
		/// </summary>
		/// <returns>True on success, false on error.</returns>
		static private bool AdjustShutdownTokenPriveleges()
		{
			bool ret;

			IntPtr hProc=IntPtr.Zero;
			IntPtr hToken=IntPtr.Zero;
			LUID luidRestore;
			TOKEN_PRIVILEGES tokenPriviliges;

			// get the current current process security token
			hProc=System.Diagnostics.Process.GetCurrentProcess().Handle;
			ret=OpenProcessToken(hProc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken);
			if(!ret)
				return false;


			// lookup the LUID for the shutdown privilege
			ret=LookupPrivilegeValue(String.Empty, SE_SHUTDOWN_NAME, out luidRestore);
			if(!ret)
				return false;

			// adjust the privileges of the current process to include the shutdown privilege
			tokenPriviliges.PrivilegeCount=1;
			tokenPriviliges.Luid=luidRestore;
			tokenPriviliges.Attributes=SE_PRIVILEGE_ENABLED;

			//TOKEN_PRIVILEGES tokenTemp=new TOKEN_PRIVILEGES();

			ret=AdjustTokenPrivileges(hToken, false, ref tokenPriviliges, 0, IntPtr.Zero, IntPtr.Zero);
			if(!ret)
				return false;

			return true;
		}
		#endregion

		#region Shutdown argument type definitions
		/// <summary>
		/// Defines the methods of shutting down the system.
		/// </summary>
		[Flags]
		public enum ShutdownMethod : uint
		{
			LogOff = 0x00,
			ShutDown = 0x01,
			Reboot = 0x02,
			Force = 0x04,
			PowerOff = 0x08,
			ForceIfHung = 0x10
		}

		/// <summary>
		/// Defines the reason for the shutdown of the system.
		/// </summary>
		[Flags]
		public enum ShutdownReason : uint
		{
			MajorApplication = 0x00040000,
			MajorHardware = 0x00010000,
			MajorLegacyApi = 0x00070000,
			MajorOperatingSystem = 0x00020000,
			MajorOther = 0x00000000,
			MajorPower = 0x00060000,
			MajorSoftware = 0x00030000,
			MajorSystem = 0x00050000,

			MinorBlueScreen = 0x0000000F,
			MinorCordUnplugged = 0x0000000b,
			MinorDisk = 0x00000007,
			MinorEnvironment = 0x0000000c,
			MinorHardwareDriver = 0x0000000d,
			MinorHotfix = 0x00000011,
			MinorHung = 0x00000005,
			MinorInstallation = 0x00000002,
			MinorMaintenance = 0x00000001,
			MinorMMC = 0x00000019,
			MinorNetworkConnectivity = 0x00000014,
			MinorNetworkCard = 0x00000009,
			MinorOther = 0x00000000,
			MinorOtherDriver = 0x0000000e,
			MinorPowerSupply = 0x0000000a,
			MinorProcessor = 0x00000008,
			MinorReconfig = 0x00000004,
			MinorSecurity = 0x00000013,
			MinorSecurityFix = 0x00000012,
			MinorSecurityFixUninstall = 0x00000018,
			MinorServicePack = 0x00000010,
			MinorServicePackUninstall = 0x00000016,
			MinorTermSrv = 0x00000020,
			MinorUnstable = 0x00000006,
			MinorUpgrade = 0x00000003,
			MinorWMI = 0x00000015,

			FlagUserDefined = 0x40000000,
			FlagPlanned = 0x80000000
		}
		#endregion

		#region Win32 Imports
		[DllImport("user32")]
		static extern bool ExitWindowsEx(ShutdownMethod uMethod, ShutdownReason dwReason);

		[DllImport("advapi32", SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
			[MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
			ref TOKEN_PRIVILEGES NewState,
			UInt32 BufferLength,
			IntPtr PreviousState,
			IntPtr ReturnLength);

		[DllImport("advapi32", SetLastError=true)]
		static extern bool OpenProcessToken(IntPtr ProcessHandle,
			UInt32 DesiredAccess, out IntPtr TokenHandle);

		[DllImport("advapi32", SetLastError=true, CharSet=CharSet.Auto)]
		static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
			out LUID lpLuid);

		[StructLayout(LayoutKind.Sequential)]
		private struct TOKEN_PRIVILEGES
		{
			public int PrivilegeCount;
			public LUID Luid;
			public int Attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LUID
		{
			public uint LowPart;
			public uint HighPart;
		}

		private const int TOKEN_QUERY=0x08;
		private const int TOKEN_ADJUST_PRIVILEGES=0x20;
		private const string SE_SHUTDOWN_NAME="SeShutdownPrivilege";
		private const int SE_PRIVILEGE_ENABLED=0x02;

		#endregion
	}
}
