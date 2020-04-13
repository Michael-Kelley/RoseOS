﻿#if !WIN_TARGET

using System;

internal static unsafe class Native {
	public static IntPtr Allocate(ulong size)
		=> EFI.Allocate(size);

	public unsafe static void Print(string msg) {
		fixed (char* c = msg)
			EFI.ST->ConOut->OutputString(EFI.ST->ConOut, c);
	}

	//public unsafe static void Print(char* msg, int len) {
	//	Win32.WriteConsoleW(StdOut, (IntPtr)msg, len, IntPtr.Zero, IntPtr.Zero);
	//}

	public unsafe static void PrintLine(string msg) {
		Print(msg);

		char* x = stackalloc char[3];
		x[0] = '\r';
		x[1] = '\n';
		x[2] = '\0';
		EFI.ST->ConOut->OutputString(EFI.ST->ConOut, x);
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

	public static void ClearConsole() {
		EFI.ST->ConOut->ClearScreen(EFI.ST->ConOut);
	}

	public static unsafe void ZeroMemory(IntPtr ptr, ulong len) {
		EFI.ST->BootServices->SetMem(ptr, len, 0);
	}

	public static unsafe void CopyMemory(IntPtr dst, IntPtr src, ulong len) {
		EFI.ST->BootServices->CopyMem(dst, src, len);
	}
}

#endif