using System;
using System.Runtime.InteropServices;

internal static unsafe class Platform {
	[DllImport("*")]
	public static extern IntPtr kmalloc(ulong size);

	[DllImport("*")]
	public static extern void kfree(IntPtr ptr);

	public static IntPtr Allocate(ulong size)
		=> kmalloc(size);

	public static void Free(IntPtr ptr) {
		kfree(ptr);
	}

	static uint[] ttyGlyphData = null;
	static uint ttyX, ttyY, ttyCols, ttyRows;

	public unsafe static void Print(string msg) {
		if (ttyGlyphData == null) {
			Debug.Print('T'); Debug.Print('T'); Debug.Print('Y'); Debug.Print('!');
			ttyGlyphData = new uint[Font.FONT_WIDTH * Font.FONT_HEIGHT];
			ttyRows = FrameBuffer.I.Width / Font.FONT_WIDTH;
			ttyCols = FrameBuffer.I.Height / Font.FONT_HEIGHT;
			ttyX = (FrameBuffer.I.Width % Font.FONT_WIDTH) / 2;
			ttyY = (FrameBuffer.I.Height % Font.FONT_HEIGHT) / 2;

			ttyRows -= 2;
			ttyCols -= 2;
			ttyX += Font.FONT_WIDTH;
			ttyY += Font.FONT_HEIGHT;
		}

		for (int i = 0; i < msg.Length; i++) {
			var c = msg[i];
			Font.GetGlyphData(c, 0x00FFFFFF, 0, ref ttyGlyphData);
			FrameBuffer.I.Blt(ttyGlyphData, Font.FONT_WIDTH, Font.FONT_HEIGHT, ttyX, ttyY);
			ttyX += Font.FONT_WIDTH;

			if (ttyX >= ttyRows * Font.FONT_WIDTH) {
				ttyX = (FrameBuffer.I.Width % Font.FONT_WIDTH) / 2;
				ttyX += Font.FONT_WIDTH;
				ttyY += Font.FONT_HEIGHT;
			}
		}
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
		FrameBuffer.I.Fill(0);
	}

	public static unsafe void ZeroMemory(IntPtr ptr, ulong len) {
		var count = len / 8;
		var rem = len % 8;

		for (var i = 0U; i < len; i++)
			((ulong*)ptr)[i] = 0;

		for (var i = 0U; i < rem; i++)
			((byte*)ptr)[count + i] = 0;
	}

	public static unsafe void CopyMemory(IntPtr dst, IntPtr src, ulong len) {
		var count = len / 8;
		var rem = len % 8;

		for (var i = 0U; i < count; i++)
			((ulong*)dst)[i] = ((ulong*)src)[i];

		for (var i = 0U; i < rem; i++)
			((byte*)dst)[count + i] = ((byte*)src)[count + i];
	}
}