using System;
using System.Runtime;

public static class Application {
	const uint LOADER_VERSION = 0x00000001;

	[RuntimeExport("EfiMain")]
	static unsafe long EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable) {
		EFI.Initialize(systemTable);
		var st = systemTable;
		var bs = st->BootServices;

		Console.Clear();

#if DEBUG
		bs->SetWatchdogTimer(0, 0);
		Console.WriteLine("SeeSharpOS Loader (DEBUG)");
#else
		Console.WriteLine("SeeSharpOS Loader");
#endif

		Console.WriteLine("=================\r\n");

		PrintLine("Loader Version:         ", LOADER_VERSION >> 16, ".", (LOADER_VERSION & 0xFF00) >> 8, ".", LOADER_VERSION & 0xFF);
		var vendor = new string(st->FirmwareVendor);
		PrintLine("UEFI Firmware Vendor:   ", vendor);
		var rev = st->FirmwareRevision;
		PrintLine("UEFI Firmware Revision: ", rev >> 16, ".", (rev & 0xFF00) >> 8, ".", rev & 0xFF);
		var ver = st->Hdr.Revision;

		if ((ver & 0xFFFF) % 10 == 0)
			PrintLine("UEFI Version:           ", ver >> 16, ".", (ver & 0xFFFF) / 10);
		else
			PrintLine("UEFI Version:           ", ver >> 16, ".", (ver & 0xFFFF) / 10, ".", (ver & 0xFFFF) % 10);

		vendor.Dispose();
		EFI_STATUS res;

		EFI_LOADED_IMAGE_PROTOCOL* li;
		res = bs->OpenProtocol(imageHandle, ref EFI.LoadedImageProtocolGuid, (IntPtr*)&li, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);

#if DEBUG
		if (res != EFI_STATUS.Success)
			Console.WriteLine("OpenProtocol(LoadedImage) failed!");
#endif

		EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* fs;
		res = bs->OpenProtocol(li->DeviceHandle, ref EFI.SimpleFileSystemProtocolGuid, (IntPtr*)&fs, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);

#if DEBUG
		if (res != EFI_STATUS.Success)
			Console.WriteLine("OpenProtocol(SimpleFileSystem) failed!");
#endif

		res = fs->OpenVolume(fs, out EFI_FILE_PROTOCOL* drive);

#if DEBUG
		if (res != EFI_STATUS.Success)
			Console.WriteLine("OpenVolume failed!");
#endif

		res = drive->Open(drive, out EFI_FILE_PROTOCOL* kernel, "kernel.bin", EFI_FILE_MODE.Read, EFI_FILE_ATTR.ReadOnly);

#if DEBUG
		if (res != EFI_STATUS.Success)
			Console.WriteLine("Open failed!");
#endif

		var fileInfoSize = (ulong)sizeof(EFI_FILE_INFO);
		kernel->GetInfo(kernel, ref EFI.FileInfoGuid, ref fileInfoSize, out EFI_FILE_INFO fileInfo);
		PrintLine("Kernel File Size: ", (uint)fileInfo.FileSize);
		var buf = stackalloc char[(int)fileInfo.FileSize];
		var fileSize = fileInfo.FileSize;
		
		res = kernel->Read(kernel, ref fileSize, (byte*)buf);

#if DEBUG
		if (res != EFI_STATUS.Success)
			Console.WriteLine("Read failed!");
#endif

		ulong hdr_mem_loc = 0x400000;
		
		Console.Write("\r\n\r\nPress any key to quit...");
		Console.ReadKey();

		return 0;
	}

	static void Main() { }

	// TODO: Delete this once String.Format is implemented
	static unsafe void PrintLine(params object[] args) {
		for (int i = 0; i < args.Length; i++)
			Console.Write(args[i].ToString());

		Console.WriteLine();
	}
}