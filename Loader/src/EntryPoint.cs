using System;
using System.Runtime;
using System.Runtime.CompilerServices;

using Internal.Runtime.CompilerServices;

public static class EntryPoint {
	const uint LOADER_VERSION = 0x00000001;

	[RuntimeExport("EfiMain")]
	static unsafe long EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable) {
		EFI.Initialise(systemTable);
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

		res = bs->OpenProtocol(imageHandle, ref EFI_GUID.LoadedImageProtocol, out EFI_LOADED_IMAGE_PROTOCOL* li, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);

		if (res != EFI_STATUS.Success)
			Error("OpenProtocol(LoadedImage) failed!", res);

		res = bs->OpenProtocol(li->DeviceHandle, ref EFI_GUID.SimpleFileSystemProtocol, out EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* fs, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);

		if (res != EFI_STATUS.Success)
			Error("OpenProtocol(SimpleFileSystem) failed!", res);

		res = fs->OpenVolume(fs, out EFI_FILE_PROTOCOL* drive);

		if (res != EFI_STATUS.Success)
			Error("OpenVolume failed!", res);

		res = drive->Open(drive, out EFI_FILE_PROTOCOL* kernel, "kernel.bin", EFI_FILE_MODE.Read, EFI_FILE_ATTR.ReadOnly);

		if (res != EFI_STATUS.Success)
			Error("Open failed!", res);

		var fileInfoSize = (ulong)sizeof(EFI_FILE_INFO);
		kernel->GetInfo(kernel, ref EFI_GUID.FileInfo, ref fileInfoSize, out EFI_FILE_INFO fileInfo);
		PrintLine("Kernel File Size: ", (uint)fileInfo.FileSize);
		var buf = stackalloc char[(int)fileInfo.FileSize];
		var fileSize = fileInfo.FileSize;

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

		fixed (IMAGE_SECTION_HEADER* ptr = sectionHdrs)
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
			return Error("Failed to allocate memory for kernel!", res);

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

		ulong numHandles = 0;
		bs->LocateHandleBuffer(EFI_LOCATE_SEARCH_TYPE.ByProtocol, ref EFI_GUID.GraphicsOutputProtocol, IntPtr.Zero, ref numHandles, out var gopHandles);
		bs->OpenProtocol(gopHandles[0], ref EFI_GUID.GraphicsOutputProtocol, out EFI_GRAPHICS_OUTPUT_PROTOCOL* gop, imageHandle, EFI_HANDLE.Zero, EFI.OPEN_PROTOCOL_GET_PROTOCOL);
		Console.WriteLine("GPU Modes:");

		for (var j = 0U; j < gop->Mode->MaxMode; j++) {
			gop->QueryMode(gop, j, out var size, out var info);

			Console.Write(info->HorizontalResolution.ToString());
			Console.Write('x');
			Console.Write(info->VeticalResolution.ToString());

			if (j != gop->Mode->MaxMode - 1)
				Console.Write(", ");
		}

		Console.WriteLine();

		var gpuMode = 0U;

		for (; ; ) {
			Console.Write("Select a mode to use: ");
			gpuMode = (uint)int.Parse(Console.ReadLine());

			if (gpuMode < gop->Mode->MaxMode)
				break;
		}

		ulong memMapSize = 0;
		EFI_MEMORY_DESCRIPTOR* memMap = null;
		res = bs->GetMemoryMap(ref memMapSize, memMap, out var memMapKey, out var memMapDescSize, out var memMapDescVar);

		if (res == EFI_STATUS.BufferTooSmall) {
			memMapSize += memMapDescSize;
			res = bs->AllocatePool(EFI_MEMORY_TYPE.LoaderData, memMapSize, (IntPtr*)(&memMap));

			if (res != EFI_STATUS.Success)
				return Error("Failed to allocate memory for memMap (1)", res);

			res = bs->GetMemoryMap(ref memMapSize, memMap, out memMapKey, out memMapDescSize, out memMapDescVar);
		}
		else
			return Error("WTF?");

		gop->SetMode(gop, gpuMode);

		var fb = new FrameBuffer(
			(IntPtr)gop->Mode->FrameBufferBase, gop->Mode->FrameBufferSize,
			gop->Mode->Info->HorizontalResolution, gop->Mode->Info->VeticalResolution,
			gop->Mode->Info->PixelFormat == EFI_GRAPHICS_PIXEL_FORMAT.BlueGreenRedReserved8BitPerColor ? FrameBuffer.PixelFormat.B8G8R8 : FrameBuffer.PixelFormat.R8G8B8
		);

		res = bs->ExitBootServices(imageHandle, memMapKey);

		// No Console.Write* after this point!

		if (res != EFI_STATUS.Success) {
			bs->FreePool((IntPtr)memMap);

			memMapSize = 0;
			res = bs->GetMemoryMap(ref memMapSize, memMap, out memMapKey, out memMapDescSize, out memMapDescVar);

			if (res == EFI_STATUS.BufferTooSmall) {
				memMapSize += memMapDescSize;
				res = bs->AllocatePool(EFI_MEMORY_TYPE.LoaderData, memMapSize, (IntPtr*)(&memMap));

				if (res != EFI_STATUS.Success)
					return (int)res;

				res = bs->GetMemoryMap(ref memMapSize, memMap, out memMapKey, out memMapDescSize, out memMapDescVar);
			}

			res = bs->ExitBootServices(imageHandle, memMapKey);
		}

		if (res != EFI_STATUS.Success) {
			bs->FreePool((IntPtr)memMap);

			return (int)res;
		}

		var epLoc = mem + ntHdr->OptionalHeader.AddressOfEntryPoint;
		RawCalliHelper.StdCall((IntPtr)epLoc, Unsafe.As<FrameBuffer, IntPtr>(ref fb));

		// We should never get here!
		// QEMU shutdown
		//Native.outw(0xB004, 0x2000);
		Console.WriteLine("\r\n\r\nPress any key to quit...");
		Console.ReadKey();

		return 0;
	}

	static int Error(string msg, EFI_STATUS ec = 0) {
		Console.Write("ERROR: ");
		Console.Write(msg);
		Console.WriteLine("\r\n\r\nPress any key to quit...");
		Console.ReadKey();

		return (int)ec;
	}

	static void Main() { }

	// TODO: Delete this once String.Format is implemented
	static unsafe void PrintLine(params object[] args) {
		for (int i = 0; i < args.Length; i++)
			Console.Write(args[i].ToString());

		Console.WriteLine();
	}
}