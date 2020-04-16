using System;
using System.Runtime;
using System.Runtime.CompilerServices;

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
		Console.WriteLine();
		EFI_STATUS res;

		EFI_LOADED_IMAGE_PROTOCOL* li;
		res = bs->OpenProtocol(imageHandle, ref EFI.LoadedImageProtocolGuid, (IntPtr*)&li, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);

		if (res != EFI_STATUS.Success)
			Error("OpenProtocol(LoadedImage) failed!");

		EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* fs;
		res = bs->OpenProtocol(li->DeviceHandle, ref EFI.SimpleFileSystemProtocolGuid, (IntPtr*)&fs, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);

		if (res != EFI_STATUS.Success)
			Error("OpenProtocol(SimpleFileSystem) failed!");

		res = fs->OpenVolume(fs, out EFI_FILE_PROTOCOL* drive);

		if (res != EFI_STATUS.Success)
			Error("OpenVolume failed!");

		res = drive->Open(drive, out EFI_FILE_PROTOCOL* kernel, "kernel.bin", EFI_FILE_MODE.Read, EFI_FILE_ATTR.ReadOnly);

		if (res != EFI_STATUS.Success)
			Error("Open failed!");

		var fileInfoSize = (ulong)sizeof(EFI_FILE_INFO);
		kernel->GetInfo(kernel, ref EFI.FileInfoGuid, ref fileInfoSize, out EFI_FILE_INFO fileInfo);
		PrintLine("Kernel File Size: ", (uint)fileInfo.FileSize);
		var buf = stackalloc char[(int)fileInfo.FileSize];
		var fileSize = fileInfo.FileSize;

		ulong hdr_mem_loc = 0x400000;
		var dosHdr = stackalloc IMAGE_DOS_HEADER[1];
		var len = (ulong)sizeof(IMAGE_DOS_HEADER);
		kernel->Read(kernel, ref len, (byte*)dosHdr);

		if (dosHdr->e_magic != 0x5A4D) // IMAGE_DOS_SIGNATURE ("MZ")
			return Error("'kernel.bin' is not a valid PE image!");

		kernel->SetPosition(kernel, (ulong)dosHdr->e_lfanew);
		var ntHdr = stackalloc IMAGE_NT_HEADERS64[1];
		len = (ulong)sizeof(IMAGE_NT_HEADERS64);
		kernel->Read(kernel, ref len, (byte*)ntHdr);

		if (ntHdr->Signature != 0x4550) // IMAGE_NT_SIGNATURE ("PE\0\0")
			return Error("'kernel.bin' is not a valid NT image!");

		if (!(ntHdr->FileHeader.Machine == 0x8664 /* IMAGE_FILE_MACHINE_X64 */ && ntHdr->OptionalHeader.Magic == 0x020B /* IMAGE_NT_OPTIONAL_HDR64_MAGIC */))
			return Error("'kernel.bin' must be a 64-bit PE image!");

		ulong sectionCount = ntHdr->FileHeader.NumberOfSections;
		ulong virtSize = 0;
		len = sectionCount * (ulong)sizeof(IMAGE_SECTION_HEADER);
		var sectionHdrs = new IMAGE_SECTION_HEADER[sectionCount];

		fixed(IMAGE_SECTION_HEADER* ptr = sectionHdrs)
			kernel->Read(kernel, ref len, (byte*)ptr);

		for (var i = 0U; i < sectionCount; i++) {
			ref var sec = ref sectionHdrs[i];
			virtSize =
				virtSize > sec.VirtualAddress + sec.PhysicalAddress_VirtualSize
				? virtSize
				: sec.VirtualAddress + sec.PhysicalAddress_VirtualSize;
		}

		ulong hdrSize = ntHdr->OptionalHeader.SizeOfHeaders;
		ulong pages = ((hdrSize) >> 12) + (((hdrSize) & 0xFFF) > 0 ? 1U : 0U);
		ulong mem = ntHdr->OptionalHeader.ImageBase;
		res = bs->AllocatePages(EFI_ALLOCATE_TYPE.Address, EFI_MEMORY_TYPE.LoaderData, pages, (IntPtr)(&mem));

		if (res != EFI_STATUS.Success)
			return Error("Failed to allocate memory for kernel!");

		Platform.ZeroMemory((IntPtr)mem, pages << 12);
		kernel->SetPosition(kernel, 0U);
		kernel->Read(kernel, ref hdrSize, (byte*)mem);

		for (var i = 0U; i < sectionCount; i++) {
			ref var sec = ref sectionHdrs[i];
			var name = new char[8];

			fixed (byte* n = sec.Name)
				for (int j = 0; j < 8; j++)
					name[j] = (char)n[j];

			if (sec.SizeOfRawData == 0) {
				fixed (char* n = name)
					PrintLine("Skipping empty section (", new string(n, 0, 8), ")");

				continue;
			}

			fixed (char* n = name)
				PrintLine("Reading section (", new string(n, 0, 8), ")");

			var addr = (byte*)mem;
			addr += sec.VirtualAddress;
			res = kernel->SetPosition(kernel, sec.PointerToRawData);

			if (res != EFI_STATUS.Success)
				Console.WriteLine("Failed to set position!");

			len = sec.SizeOfRawData;
			res = kernel->Read(kernel, ref len, addr);

			if (res != EFI_STATUS.Success)
				Console.WriteLine("Failed to read section!");
		}

		Console.WriteLine("Finished reading sections");
		sectionHdrs.Dispose();

		var epLoc = mem + ntHdr->OptionalHeader.AddressOfEntryPoint;
		RawCalliHelper.StdCall((IntPtr)epLoc, st);

		Console.Write("\r\n\r\nPress any key to quit...");
		Console.ReadKey();

		return 0;
	}

	static int Error(string msg) {
		Console.Write("ERROR: ");
		Console.Write(msg);
		Console.WriteLine("\r\n\r\nPress any key to quit...");
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