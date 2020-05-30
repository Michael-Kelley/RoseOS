﻿using System;

internal static unsafe class Platform {
	public static IntPtr Allocate(ulong size) {
		IntPtr ptr = default;
		EFI.EFI.ST.Ref.BootServices.Ref.AllocatePool(EFI.MemoryType.LoaderData, size, &ptr);

		return ptr;
	}

	public static void Free(IntPtr buf) {
		EFI.EFI.ST.Ref.BootServices.Ref.FreePool(buf);
	}

	public unsafe static void Print(string msg) {
		fixed (char* c = msg)
			EFI.EFI.ST.Ref.ConOut.Ref.OutputString(c);
	}

	public unsafe static void Print(char* msg, int len) {
		var s = new string(msg, 0, len);
		Print(s);
		s.Dispose();
	}

	public unsafe static void PrintLine(string msg) {
		Print(msg);

		char* x = stackalloc char[3];
		x[0] = '\r';
		x[1] = '\n';
		x[2] = '\0';
		EFI.EFI.ST.Ref.ConOut.Ref.OutputString(x);
	}

	//public unsafe static void PrintLine(char* msg, int len) {
	//	Print(msg, len);

	//	char* x = stackalloc char[3];
	//	x[0] = '\r';
	//	x[1] = '\n';
	//	x[2] = '\0';
	//	Print(x, 2);
	//}

	//public unsafe static char ReadKey() {
	//	char* x = stackalloc char[2];
	//	x[1] = '\0';
	//	int read = 0;

	//	Win32.ReadConsoleW(StdIn, (IntPtr)x, 1, (IntPtr)(&read), IntPtr.Zero);

	//	return x[0];
	//}

	public static ConsoleColor GetConsoleBackgroundColour()
	=> ConsoleColor.Black;

	public static void SetConsoleBackgroundColour(ConsoleColor colour) { }

	public static ConsoleColor GetConsoleForegroundColour()
	=> ConsoleColor.White;

	public static void SetConsoleForegroundColour(ConsoleColor colour) { }

	public static void ClearConsole() {
		EFI.EFI.ST.Ref.ConOut.Ref.ClearScreen();
	}

	public static unsafe void ZeroMemory(IntPtr ptr, ulong len) {
		EFI.EFI.ST.Ref.BootServices.Ref.SetMem(ptr, len, 0);
	}

	public static unsafe void CopyMemory(IntPtr dst, IntPtr src, ulong len) {
		EFI.EFI.ST.Ref.BootServices.Ref.CopyMem(dst, src, len);
	}
}