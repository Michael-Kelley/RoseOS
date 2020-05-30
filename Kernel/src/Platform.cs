using System;
using System.Runtime.InteropServices;

internal static unsafe class Platform {
	[DllImport("*")]
	public static extern IntPtr kmalloc(ulong size);

	[DllImport("*")]
	public static extern void kfree(IntPtr ptr);


	// Based on Nord (https://www.nordtheme.com/)
	static readonly uint[] ttyColours = new uint[]{
		0x002E3440,	// Black
		0x005E81AC,	// Blue
		0x00A3BE8C,	// Green
		0x0088C0D0,	// Cyan
		0x00BF616A,	// Red
		0x00B48EAD,	// Magenta
		0x00D08770,	// Yellow
		0x00D8DEE9,	// White
		0x004C566A,	// BrightBlack
		0x0081A1C1,	// BrightBlue
		0x00E3FECC,	// BrightGreen
		0x0088E0F0,	// BrightCyan
		0x00FFA1AA,	// BrightRed
		0x00F4CEED,	// BrightMagenta
		0x00EBCB8B,	// BrightYellow
		0x00ECEFF4	// BrightWhite
	};

	static uint[] ttyGlyphData = null;
	static uint ttyX, ttyY, ttyCols, ttyRows;
	static ConsoleColor ttyBgColour = ConsoleColor.Black, ttyFgColour = ConsoleColor.White;
	static uint ttyBgValue = ttyColours[(int)ConsoleColor.Black], ttyFgValue = ttyColours[(int)ConsoleColor.White];


	public static IntPtr Allocate(ulong size)
		=> kmalloc(size);

	public static void Free(IntPtr ptr) {
		kfree(ptr);
	}

	public static unsafe void ZeroMemory(IntPtr ptr, ulong len) {
		var count = len / 8;
		var rem = len % 8;

		for (var i = 0U; i < count; i++)
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

	public unsafe static void Print(string msg) {
		if (ttyGlyphData == null) {
			ttyGlyphData = new uint[Font.FONT_WIDTH * Font.FONT_HEIGHT];
			ttyRows = FrameBuffer.I.Width / Font.FONT_WIDTH - 2;
			ttyCols = FrameBuffer.I.Height / Font.FONT_HEIGHT - 2;
			ttyX = (FrameBuffer.I.Width % Font.FONT_WIDTH) / 2 + Font.FONT_WIDTH;
			ttyY = (FrameBuffer.I.Height % Font.FONT_HEIGHT) / 2 + Font.FONT_HEIGHT;
		}

		for (int i = 0; i < msg.Length; i++) {
			var c = msg[i];

			if (c == '\r') {
				ttyX = (FrameBuffer.I.Width % Font.FONT_WIDTH) / 2 + Font.FONT_WIDTH;
				continue;
			}

			if (c == '\n') {
				ttyY += Font.FONT_HEIGHT;
				continue;
			}

			if (c == '\x27') {
				i += Console.HandleANSIEscapeSequence(msg, i + 1);
				continue;
			}

			Font.GetGlyphData(c, ttyFgValue, ttyBgValue, ref ttyGlyphData);
			FrameBuffer.I.Blt(ttyGlyphData, Font.FONT_WIDTH, Font.FONT_HEIGHT, ttyX, ttyY);
			ttyX += Font.FONT_WIDTH;

			if (ttyX >= ttyRows * Font.FONT_WIDTH) {
				ttyX = (FrameBuffer.I.Width % Font.FONT_WIDTH) / 2 + Font.FONT_WIDTH;
				ttyY += Font.FONT_HEIGHT;
			}
		}
	}

	public unsafe static void Print(char* msg, int len) {
		Print(new string(msg, 0, len));
	}

	public unsafe static void PrintLine(string msg) {
		Print(msg);
		Print("\r\n");
	}

	public unsafe static void PrintLine(char* msg, int len) {
		Print(msg, len);
		Print("\r\n");
	}

	public static ConsoleColor GetConsoleBackgroundColour()
		=> ttyBgColour;

	public static void SetConsoleBackgroundColour(ConsoleColor colour) {
		ttyBgColour = colour;

		if (((uint)colour & (uint)ConsoleColor.Custom) == (uint)ConsoleColor.Custom)
			ttyBgValue = (uint)colour & ((uint)ConsoleColor.Custom - 1);
		else
			ttyBgValue = ttyColours[(int)colour];
	}

	public static ConsoleColor GetConsoleForegroundColour()
		=> ttyFgColour;

	public static void SetConsoleForegroundColour(ConsoleColor colour) {
		ttyFgColour = colour;

		if (((uint)colour & (uint)ConsoleColor.Custom) == (uint)ConsoleColor.Custom)
			ttyFgValue = (uint)colour & ((uint)ConsoleColor.Custom - 1);
		else
			ttyFgValue = ttyColours[(int)colour];
	}

	public static void ClearConsole() {
		FrameBuffer.I.Fill(ttyBgValue);
	}
}