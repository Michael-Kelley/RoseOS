#if PLATFORM_WIN

using System;
using System.Runtime.InteropServices;

internal static class Platform {
	static class Win32 {
		[DllImport("*")]
		public static extern IntPtr GlobalAlloc(uint uFlags, ulong dwBytes);

		[DllImport("*")]
		public static extern IntPtr GetStdHandle(uint nStdHandle);

		[DllImport("*")]
		public static extern uint WriteConsoleW(IntPtr hConsoleOutput, IntPtr lpBuffer, int nNumberOfCharsToWrite, IntPtr lpNumberOfCharsWritten, IntPtr lpReserved);

		[DllImport("*")]
		public static extern uint ReadConsoleW(IntPtr hConsoleOutput, IntPtr lpBuffer, int nNumberOfCharsToRead, IntPtr lpNumberOfCharsRead, IntPtr pInputControl);

		//[DllImport("*")]
		//public static extern void RtlCopyMemory(IntPtr destination, IntPtr source, ulong length);
	}


	static IntPtr _stdout = IntPtr.Zero;
	static IntPtr StdOut {
		get {
			if (_stdout.Equals(IntPtr.Zero))
				_stdout = Win32.GetStdHandle(unchecked((uint)-11));

			return _stdout;
		}
	}

	static IntPtr _stdin = IntPtr.Zero;
	static IntPtr StdIn {
		get {
			if (_stdin.Equals(IntPtr.Zero))
				_stdin = Win32.GetStdHandle(unchecked((uint)-10));

			return _stdin;
		}
	}


	public static IntPtr Allocate(ulong size) {
		return Win32.GlobalAlloc(0, size);
	}

	public unsafe static void Print(string msg) {
		fixed (char* c = &msg._firstChar)
			Win32.WriteConsoleW(StdOut, (IntPtr)c, msg.Length, IntPtr.Zero, IntPtr.Zero);
	}

	public unsafe static void Print(char* msg, int len) {
		Win32.WriteConsoleW(StdOut, (IntPtr)msg, len, IntPtr.Zero, IntPtr.Zero);
	}

	public unsafe static void PrintLine(string msg) {
		Print(msg);

		char* x = stackalloc char[3];
		x[0] = '\r';
		x[1] = '\n';
		x[2] = '\0';
		Print(x, 2);
	}

	public unsafe static void PrintLine(char* msg, int len) {
		Print(msg, len);

		char* x = stackalloc char[3];
		x[0] = '\r';
		x[1] = '\n';
		x[2] = '\0';
		Print(x, 2);
	}

	public unsafe static char ReadKey() {
		char* x = stackalloc char[2];
		x[1] = '\0';
		int read = 0;

		Win32.ReadConsoleW(StdIn, (IntPtr)x, 1, (IntPtr)(&read), IntPtr.Zero);

		return x[0];
	}

	public static void ClearConsole() {

	}

	public static unsafe void ZeroMemory(IntPtr ptr, ulong len) {
		var p = (long*)ptr;
		var count = len / 8;
		var rem = len % 8;

		for (var i = 0UL; i < count; i++)
			p[i] = 0;

		if (rem > 0) {
			var b = (byte*)ptr;
			b += count * sizeof(long);

			for (var i = 0UL; i < rem; i++)
				b[i] = 0;
		}
	}

	public static unsafe void CopyMemory(IntPtr dst, IntPtr src, ulong len) {
		//Win32.RtlCopyMemory(dst, src, len);

		var d = (long*)dst;
		var s = (long*)src;
		var count = len / 8;
		var rem = len % 8;

		for (var i = 0UL; i < count; i++)
			d[i] = s[i];

		if (rem > 0) {
			var bd = (byte*)dst;
			var bs = (byte*)src;

			bd += count * sizeof(long);
			bs += count * sizeof(long);

			for (var i = 0UL; i < rem; i++)
				bd[i] = bs[i];
		}
	}
}

#endif