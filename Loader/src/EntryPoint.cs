using System;
using System.Runtime;
using System.Runtime.CompilerServices;

using Internal.Runtime.CompilerServices;
using NativeTypeWrappers;

public static class EntryPoint {
	const uint LOADER_VERSION = 0x00010000;	// 1.0.0

	[RuntimeExport("EfiMain")]
	static long EfiMain(EFI.Handle imageHandle, ReadonlyNativeReference<EFI.SystemTable> systemTable) {
		EFI.EFI.Initialise(systemTable);
		ref var st = ref systemTable.Ref;
		ref var bs = ref st.BootServices.Ref;

		Console.Clear();

#if DEBUG
		bs.SetWatchdogTimer(0, 0, 0, IntPtr.Zero);
		Console.WriteLine("SeeSharpOS Loader (DEBUG)");
#else
		Console.WriteLine("SeeSharpOS Loader");
#endif

		Console.WriteLine("=================\r\n");

		PrintLine("Loader Version:         ", LOADER_VERSION >> 16, ".", (LOADER_VERSION & 0xFF00) >> 8, ".", LOADER_VERSION & 0xFF);
		var vendor = st.FirmwareVendor.ToString();
		PrintLine("UEFI Firmware Vendor:   ", vendor);
		vendor.Dispose(); vendor = null;
		var rev = st.FirmwareRevision;
		PrintLine("UEFI Firmware Revision: ", rev >> 16, ".", (rev & 0xFF00) >> 8, ".", rev & 0xFF);
		var ver = st.Hdr.Revision;

		if ((ver & 0xFFFF) % 10 == 0)
			PrintLine("UEFI Version:           ", ver >> 16, ".", (ver & 0xFFFF) / 10);
		else
			PrintLine("UEFI Version:           ", ver >> 16, ".", (ver & 0xFFFF) / 10, ".", (ver & 0xFFFF) % 10);

		Console.WriteLine();
		EFI.Status res;

		res = bs.OpenProtocol(
			imageHandle,
			ref EFI.Guid.LoadedImageProtocol,
			out ReadonlyNativeReference<EFI.LoadedImageProtocol> li,
			imageHandle, EFI.Handle.Zero, EFI.EFI.OPEN_PROTOCOL_GET_PROTOCOL
		);

		if (res != EFI.Status.Success)
			Error("OpenProtocol(LoadedImage) failed!", res);

		res = bs.OpenProtocol(
			li.Ref.DeviceHandle,
			ref EFI.Guid.SimpleFileSystemProtocol,
			out ReadonlyNativeReference<EFI.SimpleFileSystemProtocol> fs,
			imageHandle, EFI.Handle.Zero, EFI.EFI.OPEN_PROTOCOL_GET_PROTOCOL
		);

		if (res != EFI.Status.Success)
			Error("OpenProtocol(SimpleFileSystem) failed!", res);

		res = fs.Ref.OpenVolume(out var rDrive);

		if (res != EFI.Status.Success)
			Error("OpenVolume failed!", res);

		ref var drive = ref rDrive.Ref;
		res = drive.Open(out var rKernel, "kernel.bin", EFI.FileMode.Read, EFI.FileAttribute.ReadOnly);

		if (res != EFI.Status.Success)
			Error("Open failed!", res);

		ref var kernel = ref rKernel.Ref;
		var fileInfoSize = (ulong)Unsafe.SizeOf<EFI.FileInfo>();
		kernel.GetInfo(ref EFI.Guid.FileInfo, ref fileInfoSize, out EFI.FileInfo fileInfo);
		PrintLine("Kernel File Size: ", (uint)fileInfo.FileSize, " bytes");

		kernel.Read(out PE.DOSHeader dosHdr);

		if (dosHdr.e_magic != 0x5A4D) // IMAGE_DOS_SIGNATURE ("MZ")
			return Error("'kernel.bin' is not a valid PE image!");

		kernel.SetPosition((ulong)dosHdr.e_lfanew);
		kernel.Read(out PE.NtHeaders64 ntHdr);

		if (ntHdr.Signature != 0x4550) // IMAGE_NT_SIGNATURE ("PE\0\0")
			return Error("'kernel.bin' is not a valid NT image!");

		if (!(ntHdr.FileHeader.Machine == 0x8664 /* IMAGE_FILE_MACHINE_X64 */ && ntHdr.OptionalHeader.Magic == 0x020B /* IMAGE_NT_OPTIONAL_HDR64_MAGIC */))
			return Error("'kernel.bin' must be a 64-bit PE image!");

		ulong sectionCount = ntHdr.FileHeader.NumberOfSections;
		ulong virtSize = 0;
		kernel.Read(out PE.SectionHeader[] sectionHdrs, (int)sectionCount);

		for (var i = 0U; i < sectionCount; i++) {
			ref var sec = ref sectionHdrs[i];
			virtSize =
				virtSize > sec.VirtualAddress + sec.PhysicalAddress_VirtualSize
				? virtSize
				: sec.VirtualAddress + sec.PhysicalAddress_VirtualSize;
		}

		ulong hdrSize = ntHdr.OptionalHeader.SizeOfHeaders;
		ulong pages = ((virtSize) >> 12) + (((virtSize) & 0xFFF) > 0 ? 1U : 0U);
		PrintLine("Virtual Size: ", virtSize, ", Pages: ", pages);
		var mem = (IntPtr)ntHdr.OptionalHeader.ImageBase;
		res = bs.AllocatePages(EFI.AllocateType.Address, EFI.MemoryType.LoaderData, pages, ref mem);
		PrintLine("mem: ", (ulong)mem);

		if (res != EFI.Status.Success)
			return Error("Failed to allocate memory for kernel!", res);

		Platform.ZeroMemory(mem, pages << 12);
		kernel.SetPosition(0U);
		kernel.Read(ref hdrSize, mem);

		var modulesSeg = IntPtr.Zero;

		for (var i = 0U; i < sectionCount; i++) {
			ref var sec = ref sectionHdrs[i];
			var name = string.FromASCII(sec.Name, 8);

			if (sec.SizeOfRawData == 0) {
				PrintLine("Skipping empty section (", name, ")");

				continue;
			}

			PrintLine("Reading section (", name, ")");

			if (name[1] == 'm' && name[2] == 'o')
				modulesSeg = mem + sec.VirtualAddress;

			name.Dispose();

			var addr = mem + sec.VirtualAddress;
			res = kernel.SetPosition(sec.PointerToRawData);

			if (res != EFI.Status.Success)
				Console.WriteLine("Failed to set position!");

			var len = (ulong)sec.SizeOfRawData;
			res = kernel.Read(ref len, addr);

			if (res != EFI.Status.Success)
				Console.WriteLine("Failed to read section!");
		}

		kernel.Close();
		drive.Close();
		Console.WriteLine("Finished reading sections");
		sectionHdrs.Dispose();

		ulong numHandles = 0;
		bs.LocateHandleBuffer(EFI.LocateSearchType.ByProtocol, ref EFI.Guid.GraphicsOutputProtocol, IntPtr.Zero, ref numHandles, out var gopHandles);
		bs.OpenProtocol(
			gopHandles[0],
			ref EFI.Guid.GraphicsOutputProtocol,
			out ReadonlyNativeReference<EFI.GraphicsOutputProtocol> rGop,
			imageHandle, EFI.Handle.Zero, EFI.EFI.OPEN_PROTOCOL_GET_PROTOCOL
		);
		ref var gop = ref rGop.Ref;
		ref var gopMode = ref gop.Mode.Ref;
		Console.WriteLine("GPU Modes:");

		for (var j = 0U; j < gopMode.MaxMode; j++) {
			gop.QueryMode(j, out var size, out var rInfo);
			ref var info = ref rInfo.Ref;

			Console.Write((ushort)info.HorizontalResolution);
			Console.Write('x');
			Console.Write((ushort)info.VerticalResolution);

			if (j != gopMode.MaxMode - 1)
				Console.Write(", ");
		}

		Console.WriteLine();

		var gpuMode = 0U;

		for (; ; ) {
			Console.Write("Select a mode to use: ");
			var s = Console.ReadLine();
			gpuMode = (uint)int.Parse(s);
			s.Dispose();

			if (gpuMode < gopMode.MaxMode)
				break;
		}

		gop.SetMode(gpuMode);
		Console.Clear();
		gopMode = ref gop.Mode.Ref;
		var mmap = new MemoryMap();
		mmap.Retrieve();

		var fb = new FrameBuffer(
			(IntPtr)gopMode.FrameBufferBase, gopMode.FrameBufferSize,
			gopMode.Info.Ref.HorizontalResolution, gopMode.Info.Ref.VerticalResolution,
			gopMode.Info.Ref.PixelFormat == EFI.GraphicsPixelFormat.BlueGreenRedReserved8BitPerColor ? FrameBuffer.PixelFormat.B8G8R8 : FrameBuffer.PixelFormat.R8G8B8
		);

#if DEBUG
		Console.Write("Unfreed allocations: ");
		Console.Write((ushort)EFI.BootServices.AllocateCount);
		Console.WriteLine("\r\nPress any key to continue to kernel...");
		Console.ReadKey();
#endif

		res = bs.ExitBootServices(imageHandle, mmap.Key);

		// No Console.Write* after this point!

		if (res != EFI.Status.Success) {
			mmap.Free();
			mmap.Retrieve();

			res = bs.ExitBootServices(imageHandle, mmap.Key);

			if (res != EFI.Status.Success) {
				mmap.Free();

				return (int)res;
			}
		}

		var epLoc = mem + ntHdr.OptionalHeader.AddressOfEntryPoint;
		RawCalliHelper.StdCall(epLoc, modulesSeg, fb, mmap);

		// We should never get here!
		// QEMU shutdown
		//Native.outw(0xB004, 0x2000);
		//Console.WriteLine("\r\n\r\nPress any key to quit...");
		//Console.ReadKey();

		return 0;
	}

	static int Error(string msg, EFI.Status ec = 0) {
		Console.Write("ERROR: ");
		Console.Write(msg);
		Console.WriteLine("\r\n\r\nPress any key to quit...");
		Console.ReadKey();

		return (int)ec;
	}

	static void Main() { }

	// TODO: Delete this once String.Format is implemented
	static unsafe void PrintLine(params object[] args) {
		for (int i = 0; i < args.Length; i++) {
			if (args[i] is string)
				Console.Write(args[i] as string);
			else {
				var s = args[i].ToString();
				Console.Write(s);
				s.Dispose();
				args[i].Dispose();
			}
		}

		args.Dispose();
		Console.WriteLine();
	}
}