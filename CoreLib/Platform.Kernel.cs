#if PLATFORM_KERNEL

using System;

internal static unsafe class Platform {
	public static IntPtr Allocate(ulong size) {
		IntPtr ptr = default;
		//..

		return ptr;
	}

	public static void Free(IntPtr buf) {
		//..
	}

	public unsafe static void Print(string msg) {
		//..
	}

	public unsafe static void Print(char* msg, int len) {
		Print(new string(msg, 0, len));
	}

	public unsafe static void PrintLine(string msg) {
		Print(msg);

		char* x = stackalloc char[3];
		x[0] = '\r';
		x[1] = '\n';
		x[2] = '\0';
		//..
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
		//..
	}

	public static unsafe void ZeroMemory(IntPtr ptr, ulong len) {
		//..
	}

	public static unsafe void CopyMemory(IntPtr dst, IntPtr src, ulong len) {
		//..
	}
}

#endif