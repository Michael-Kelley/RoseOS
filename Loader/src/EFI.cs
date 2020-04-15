using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


public enum EFI_STATUS : ulong {
	Success,
}

public enum EFI_MEMORY_TYPE : uint {
	ReservedMemoryType,
	LoaderCode,
	LoaderData,
	BootServicesCode,
	BootServicesData,
	RuntimeServicesCode,
	RuntimeServicesData,
	ConventionalMemory,
	UnusableMemory,
	ACPIReclaimMemory,
	ACPIMemoryNVS,
	MemoryMappedIO,
	MemoryMappedIOPortSpace,
	PalCode,
	MaxMemoryType
}

[Flags]
public enum EFI_FILE_MODE : ulong {
	Read = 1,
	Write = 2,
	Create = 0x8000000000000000
}

[Flags]
public enum EFI_FILE_ATTR : ulong {
	ReadOnly = 1,
	Hidden = 2,
	System = 4,
	Reserved = 8,
	Directory = 16,
	Archive = 32,

	ValidAttr = ReadOnly | Hidden | System | Directory | Archive
}


[StructLayout(LayoutKind.Sequential)]
public struct EFI_HANDLE {
	private IntPtr _handle;

	public static EFI_HANDLE Zero = new EFI_HANDLE { _handle = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct EFI_TABLE_HEADER {
	public readonly ulong Signature;
	public readonly uint Revision;
	public readonly uint HeaderSize;
	public readonly uint Crc32;
	public readonly uint Reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_SYSTEM_TABLE {
	public readonly EFI_TABLE_HEADER Hdr;
	public readonly char* FirmwareVendor;
	public readonly uint FirmwareRevision;
	public readonly EFI_HANDLE ConsoleInHandle;
	public readonly EFI_SIMPLE_TEXT_INPUT_PROTOCOL* ConIn;
	public readonly EFI_HANDLE ConsoleOutHandle;
	public readonly EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* ConOut;
	public readonly EFI_HANDLE StandardErrorHandle;
	public readonly EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* StdErr;
	public readonly EFI_RUNTIME_SERVICES* RuntimeServices;
	public readonly EFI_BOOT_SERVICES* BootServices;
	public readonly ulong NumberOfTableEntries;
	public readonly void* ConfigurationTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_RUNTIME_SERVICES {
	public readonly EFI_TABLE_HEADER Hdr;
	private readonly IntPtr _GetTime;
	private readonly IntPtr _SetTime;
	private readonly IntPtr _GetWakeupTime;
	private readonly IntPtr _SetWakeupTime;
	private readonly IntPtr _SetVirtualAddressMap;
	private readonly IntPtr _ConvertPointer;
	private readonly IntPtr _GetVariable;
	private readonly IntPtr _GetNextVariableName;
	private readonly IntPtr _SetVariable;
	private readonly IntPtr _GetNextHighMonotonicCount;
	private readonly IntPtr _ResetSystem;
	private readonly IntPtr _UpdateCapsule;
	private readonly IntPtr _QueryCapsuleCapabilities;
	private readonly IntPtr _QueryVariableInfo;

	public ulong GetTime(out EFI_TIME time, out EFI_TIME_CAPABILITIES capabilities) {
		fixed (EFI_TIME* timeAddress = &time)
		fixed (EFI_TIME_CAPABILITIES* capabilitiesAddress = &capabilities)
			return RawCalliHelper.StdCall(_GetTime, timeAddress, capabilitiesAddress);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct EFI_TIME {
	public ushort Year;
	public byte Month;
	public byte Day;
	public byte Hour;
	public byte Minute;
	public byte Second;
	public byte Pad1;
	public uint Nanosecond;
	public short TimeZone;
	public byte Daylight;
	public byte PAD2;
}

[StructLayout(LayoutKind.Sequential)]
public struct SIMPLE_TEXT_OUTPUT_MODE {
	public readonly int MaxMode;
	public readonly int Mode;
	public readonly int Attribute;
	public readonly int CursorColumn;
	public readonly int CursorRow;
	public readonly bool CursorVisible;
}

[StructLayout(LayoutKind.Sequential)]
public struct EFI_TIME_CAPABILITIES {
	public uint Resolution;
	public uint Accuracy;
	public bool SetsToZero;
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct EFI_INPUT_KEY {
	public readonly ushort ScanCode;
	public readonly ushort UnicodeChar;
}

public readonly struct EFI_EVENT {
	private readonly IntPtr _value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_SIMPLE_TEXT_INPUT_PROTOCOL {
	readonly IntPtr _reset;
	readonly IntPtr _readKeyStroke;

	public readonly EFI_EVENT WaitForKey;

	public void Reset(void* handle, bool ExtendedVerification) {
		RawCalliHelper.StdCall(_reset, (byte*)handle, ExtendedVerification);
	}

	public EFI_STATUS ReadKeyStroke(void* handle, EFI_INPUT_KEY* Key)
		=> (EFI_STATUS)RawCalliHelper.StdCall(_readKeyStroke, (byte*)handle, Key);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_BOOT_SERVICES {
	readonly EFI_TABLE_HEADER Hdr;

	readonly IntPtr _RaiseTPL;
	readonly IntPtr _RestoreTPL;
	readonly IntPtr _AllocatePages;
	readonly IntPtr _FreePages;
	readonly IntPtr _GetMemoryMap;
	readonly IntPtr _AllocatePool;
	readonly IntPtr _FreePool;
	readonly IntPtr _CreateEvent;
	readonly IntPtr _SetTimer;
	readonly IntPtr _WaitForEvent;
	readonly IntPtr _SignalEvent;
	readonly IntPtr _CloseEvent;
	readonly IntPtr _CheckEvent;
	readonly IntPtr _InstallProtocolInterface;
	readonly IntPtr _ReinstallProtocolInterface;
	readonly IntPtr _UninstallProtocolInterface;
	readonly IntPtr _HandleProtocol;
	readonly IntPtr _Reserved;
	readonly IntPtr _RegisterProtocolNotify;
	readonly IntPtr _LocateHandle;
	readonly IntPtr _LocateDevicePath;
	readonly IntPtr _InstallConfigurationTable;
	readonly IntPtr _LoadImage;
	readonly IntPtr _StartImage;
	readonly IntPtr _Exit;
	readonly IntPtr _UnloadImage;
	readonly IntPtr _ExitBootServices;
	readonly IntPtr _GetNextMonotonicCount;
	readonly IntPtr _Stall;
	readonly IntPtr _SetWatchdogTimer;
	readonly IntPtr _ConnectController;
	readonly IntPtr _DisconnectController;
	readonly IntPtr _OpenProtocol;
	readonly IntPtr _CloseProtocol;
	readonly IntPtr _OpenProtocolInformation;
	readonly IntPtr _ProtocolsPerHandle;
	readonly IntPtr _LocateHandleBuffer;
	readonly IntPtr _LocateProtocol;
	readonly IntPtr _InstallMultipleProtocolInterfaces;
	readonly IntPtr _UninstallMultipleProtocolInterfaces;
	readonly IntPtr _CalculateCrc32;
	readonly IntPtr _CopyMem;
	readonly IntPtr _SetMem;
	readonly IntPtr _CreateEventEx;


	public EFI_STATUS WaitForEvent(uint count, EFI_EVENT* events, uint* index)
		=> (EFI_STATUS)RawCalliHelper.StdCall(_WaitForEvent, count, events, index);

	public EFI_STATUS WaitForSingleEvent(EFI_EVENT evt) {
		uint i = 0;

		return WaitForEvent(1, &evt, &i);
	}

	public EFI_STATUS Stall(uint Microseconds)
		=> (EFI_STATUS)RawCalliHelper.StdCall(_Stall, Microseconds);

	public EFI_STATUS AllocatePool(EFI_MEMORY_TYPE type, ulong size, IntPtr* buf)
		=> (EFI_STATUS)RawCalliHelper.StdCall(_AllocatePool, type, size, buf);

	public EFI_STATUS FreePool(IntPtr buf)
		=> (EFI_STATUS)RawCalliHelper.StdCall(_FreePool, buf);

	public void CopyMem(IntPtr dst, IntPtr src, ulong length)
		=> RawCalliHelper.StdCall(_CopyMem, dst, src, length);

	public void SetMem(IntPtr buf, ulong size, byte val)
		=> RawCalliHelper.StdCall(_SetMem, buf, size, val);

	public EFI_STATUS SetWatchdogTimer(ulong timeout, ulong code, ulong dataSize = 0, char* data = null)
		=> (EFI_STATUS)RawCalliHelper.StdCall(_SetWatchdogTimer, timeout, code, dataSize, data);

	public EFI_STATUS OpenProtocol(EFI_HANDLE handle, ref EFI_GUID protocol, IntPtr* iface, EFI_HANDLE agent, EFI_HANDLE controller, uint attr) {
		fixed (EFI_GUID* pProt = &protocol)
			return (EFI_STATUS)RawCalliHelper.StdCall(_OpenProtocol, handle, pProt, iface, agent, controller, attr);
	}
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL {
	private readonly IntPtr _reset;
	private readonly IntPtr _outputString;
	private readonly IntPtr _testString;
	private readonly IntPtr _queryMode;
	private readonly IntPtr _setMode;
	private readonly IntPtr _setAttribute;
	private readonly IntPtr _clearScreen;
	private readonly IntPtr _setCursorPosition;
	private readonly IntPtr _enableCursor;

	public readonly SIMPLE_TEXT_OUTPUT_MODE* Mode;

	public void Reset(void* handle, bool ExtendedVerification) {
		RawCalliHelper.StdCall(_reset, (byte*)handle, &ExtendedVerification);
	}

	public ulong OutputString(void* handle, char* str)
		=> RawCalliHelper.StdCall(_outputString, (byte*)handle, str);

	public ulong TestString(void* handle, char* str)
		=> RawCalliHelper.StdCall(_testString, (byte*)handle, str);

	public void QueryMode(void* handle, uint ModeNumber, uint* Columns, uint* Rows) {
		RawCalliHelper.StdCall(_queryMode, (byte*)handle, &ModeNumber, Columns, Rows);
	}

	public void SetMode(void* handle, uint ModeNumber) {
		RawCalliHelper.StdCall(_setMode, (byte*)handle, &ModeNumber);
	}

	public void SetAttribute(void* handle, uint Attribute) {
		RawCalliHelper.StdCall(_setAttribute, (byte*)handle, Attribute);
	}

	public void ClearScreen(void* handle) {
		RawCalliHelper.StdCall(_clearScreen, (byte*)handle);
	}

	public void SetCursorPosition(void* handle, uint Column, uint Row) {
		RawCalliHelper.StdCall(_setCursorPosition, (byte*)handle, Column, Row);
	}

	public void EnableCursor(void* handle, bool Visible) {
		RawCalliHelper.StdCall(_enableCursor, (byte*)handle, Visible);
	}
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_GUID {
	public uint Data1;
	public ushort Data2;
	public ushort Data3;
	public fixed byte Data4[8];

	public EFI_GUID(uint d1, ushort d2, ushort d3, byte[] d4) {
		Data1 = d1;
		Data2 = d2;
		Data3 = d3;

		fixed (byte* dst = Data4)
		fixed (byte* src = &d4[0])
			Platform.CopyMemory((IntPtr)dst, (IntPtr)src, 8);

		//d4.Dispose();
	}
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_FILE_PROTOCOL {
	public readonly ulong Revision;

	readonly IntPtr _Open;
	readonly IntPtr _Close;
	readonly IntPtr _Delete;
	readonly IntPtr _Read;
	readonly IntPtr _Write;
	readonly IntPtr _GetPosition;
	readonly IntPtr _SetPosition;
	readonly IntPtr _GetInfo;
	readonly IntPtr _SetInfo;
	readonly IntPtr _Flush;
	readonly IntPtr _OpenEx;
	readonly IntPtr _ReadEx;
	readonly IntPtr _WriteEx;
	readonly IntPtr _FlushEx;


	public EFI_STATUS Open(EFI_FILE_PROTOCOL* handle, out EFI_FILE_PROTOCOL* newHandle, string filename, EFI_FILE_MODE mode, EFI_FILE_ATTR attr) {
		fixed (EFI_FILE_PROTOCOL** ptr = &newHandle)
		fixed (char* f = &filename._firstChar)
			return (EFI_STATUS)RawCalliHelper.StdCall(_Open, handle, (IntPtr*)ptr, f, mode, attr);
	}

	public EFI_STATUS GetInfo(EFI_FILE_PROTOCOL* handle, ref EFI_GUID type, ref ulong bufSize, out EFI_FILE_INFO buf) {
		fixed (EFI_GUID* pType = &type)
		fixed (ulong* pSize = &bufSize)
		fixed (EFI_FILE_INFO* pBuf = &buf)
			return (EFI_STATUS)RawCalliHelper.StdCall(_GetInfo, handle, pType, pSize, pBuf);
	}

	public EFI_STATUS Read(EFI_FILE_PROTOCOL* handle, ref ulong bufSize, byte* buf) {
		fixed (ulong* pSize = &bufSize)
			return (EFI_STATUS)RawCalliHelper.StdCall(_Read, handle, pSize, buf);
	}
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_LOADED_IMAGE_PROTOCOL {
	public readonly uint Revision;
	public readonly EFI_HANDLE ParentHandle;
	public readonly EFI_SYSTEM_TABLE* SystemTable;
	public readonly EFI_HANDLE DeviceHandle;
	//public readonly EFI_DEVICE_PATH_PROTOCOL* FilePath;
	public readonly IntPtr FilePath;
	public readonly IntPtr Reserved;
	public readonly uint LoadOptionsSize;
	public readonly IntPtr LoadOptions;
	public readonly IntPtr ImageBase;
	public readonly ulong ImageSize;
	public readonly EFI_MEMORY_TYPE ImageCodeType;
	public readonly EFI_MEMORY_TYPE ImageDataType;

	public readonly IntPtr _Unload;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct EFI_SIMPLE_FILE_SYSTEM_PROTOCOL {
	public readonly ulong Revision;

	readonly IntPtr _OpenVolume;


	public EFI_STATUS OpenVolume(EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* handle, out EFI_FILE_PROTOCOL* root) {
		fixed (EFI_FILE_PROTOCOL** ptr = &root)
			return (EFI_STATUS)RawCalliHelper.StdCall(_OpenVolume, handle, (IntPtr*)ptr);
	}
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_INFO {
	public ulong Size;
	public ulong FileSize;
	public ulong PhysicalSize;
	public EFI_TIME CreateTime;
	public EFI_TIME LastAccessTime;
	public EFI_TIME ModificationTime;
	public ulong Attribute;
	public fixed char FileName[128];
}

public unsafe static class EFI {
	public static EFI_GUID LoadedImageProtocolGuid;
	public static EFI_GUID SimpleFileSystemProtocolGuid;
	public static EFI_GUID FileInfoGuid;

	public const uint OPEN_PROTOCOL_GET_PROTOCOL = 0x00000002;

	public static EFI_SYSTEM_TABLE* ST { get; private set; }


	public static void Initialize(EFI_SYSTEM_TABLE* systemTable) {
		ST = systemTable;
		LoadedImageProtocolGuid = new EFI_GUID(0x5B1B31A1, 0x9562, 0x11d2, new byte[] { 0x8E, 0x3F, 0x00, 0xA0, 0xC9, 0x69, 0x72, 0x3B });
		SimpleFileSystemProtocolGuid = new EFI_GUID(0x964e5b22, 0x6459, 0x11d2, new byte[] { 0x8e, 0x39, 0x0, 0xa0, 0xc9, 0x69, 0x72, 0x3b });
		FileInfoGuid = new EFI_GUID(0x09576e92, 0x6d3f, 0x11d2, new byte[] { 0x8e, 0x39, 0x00, 0xa0, 0xc9, 0x69, 0x72, 0x3b });
	}
}