﻿using NativeTypeWrappers;

namespace System {
	public static class Console {
		const int MAX_INPUT_LENGTH = 255;


		static ushort s_consoleAttribute;
		static ushort s_cursorX;
		static ushort s_cursorY;
		static ushort s_windowSizeX;
		static ushort s_windowSizeY;
		static char lastKey = '\0';
		static ushort lastScanCode;


		//public static unsafe bool CursorVisible {
		//	set {
		//		EFI.ST.Ref.ConOut->EnableCursor(EFI.ST.Ref.ConOut, value);
		//	}
		//}

		public static ConsoleColor ForegroundColor {
			get => Platform.GetConsoleForegroundColour();
			set => Platform.SetConsoleForegroundColour(value);

			//s_consoleAttribute = (ushort)value;
			//uint color = s_consoleAttribute;
			//EFI.ST.Ref.ConOut->SetAttribute(EFI.ST.Ref.ConOut, color);
		}

		public static ConsoleColor BackgroundColor {
			get => Platform.GetConsoleBackgroundColour();
			set => Platform.SetConsoleBackgroundColour(value);
		}

		public static ConsoleColor ColorFromRGB(byte red, byte green, byte blue)
			=> (ConsoleColor)(blue | (green << 8) | (red << 16) | (int)ConsoleColor.Custom);

		//public static unsafe bool KeyAvailable {
		//	get {
		//		if (lastKey == '\0')
		//			return true;

		//		EFI_INPUT_KEY key;
		//		var errorCode = EFI.ST.Ref.ConIn->ReadKeyStroke(EFI.ST.Ref.ConIn, &key);
		//		lastKey = (char)key.UnicodeChar;
		//		return errorCode == 0;
		//	}
		//}


		public static unsafe void Clear() {
			Platform.ClearConsole();
		}

		public static unsafe ConsoleKeyInfo ReadKey(bool intercept = false) {
#if PLATFORM_EFI
			// TODO: Change this to use Native instead of platform-specific code
			if (lastKey == '\0') {
				EFI.EFI.ST.Ref.ConIn.Ref.Reset(false);

				EFI.EFI.ST.Ref.BootServices.Ref.WaitForSingleEvent(EFI.EFI.ST.Ref.ConIn.Ref.WaitForKey);
				EFI.EFI.ST.Ref.ConIn.Ref.ReadKeyStroke(out var key);
				lastKey = (char)key.UnicodeChar;
				lastScanCode = key.ScanCode;
			}

			char c = lastKey;
			ushort s = lastScanCode;

			ConsoleKey k = default;

			if (c == 'w')
				k = ConsoleKey.UpArrow;
			else if (c == 'd')
				k = ConsoleKey.RightArrow;
			else if (c == 's')
				k = ConsoleKey.DownArrow;
			else if (c == 'a')
				k = ConsoleKey.LeftArrow;
			else if (c == '\r')
				k = ConsoleKey.Enter;

			if (lastScanCode != 0) {
				k = lastScanCode switch
				{
					1 => ConsoleKey.UpArrow,
					2 => ConsoleKey.DownArrow,
					3 => ConsoleKey.RightArrow,
					4 => ConsoleKey.LeftArrow,
					_ => k,
				};
			}

			lastKey = '\0';

			if (!intercept)
				Write(c);

			return new ConsoleKeyInfo(c, k, false, false, false);
#else
			var c = Platform.ReadKey();

			if (!intercept) {
				if (c.KeyChar == '\n')
					Write("\r\n");
				else
					Write(c.KeyChar);
			}

			var r = new ConsoleKeyInfo(c.KeyChar, (ConsoleKey)c.Key,
				c.Modifiers.HasFlag(KeyModifier.Shift), c.Modifiers.HasFlag(KeyModifier.Alt), c.Modifiers.HasFlag(KeyModifier.Ctrl));

			return r;
#endif
		}

		//public unsafe static void SetCursorPosition(int x, int y) {
		//	s_cursorX = (ushort)x;
		//	s_cursorY = (ushort)y;
		//	EFI.ST.Ref.ConOut->SetCursorPosition(
		//		EFI.ST.Ref.ConOut,
		//		(uint)x,
		//		(uint)y);
		//}

#if PLATFORM_EFI
		unsafe static void WriteChar(ref EFI.SimpleTextOutputProtocol ConOut, char data) {
			// Translate some unicode characters into the IBM hardware codepage
			data = data switch
			{
				'│' => '\u2502',
				'┌' => '\u250c',
				'┐' => '\u2510',
				'─' => '\u2500',
				'└' => '\u2514',
				'┘' => '\u2518',
				_ => data,
			};

			char* x = stackalloc char[2];
			x[0] = data;
			x[1] = '\0';
			ConOut.OutputString(x);
		}
#endif

		public static unsafe void Write(char val) {
#if PLATFORM_EFI
			WriteChar(ref EFI.EFI.ST.Ref.ConOut.Ref, val);
#else
			Platform.Print(&val, 1);
#endif
		}

		public static unsafe void Write(ushort val) {
			char* x = stackalloc char[6];
			var i = 4;

			x[5] = '\0';

			do {
				var d = val % 10;
				val /= 10;

				d += 0x30;
				x[i--] = (char)d;
			} while (val > 0);

			i++;

#if PLATFORM_EFI
			EFI.EFI.ST.Ref.ConOut.Ref.OutputString(x + i);
#else
			Platform.Print(x + i, 5 - i);
#endif
		}

		public static unsafe void Write(ulong val) {
			char* x = stackalloc char[21];
			var i = 19;

			x[20] = '\0';

			do {
				var d = (ushort)(val % 10);
				val /= 10;

				d += 0x30;
				x[i--] = (char)d;
			} while (val > 0);

			i++;

#if PLATFORM_EFI
			EFI.EFI.ST.Ref.ConOut.Ref.OutputString(x + i);
#else
			//Platform.Print(x + i, 20 - i);

			for (int j = i; j < 20; j++)
				Platform.Print(x[j]);
#endif
		}

		public static unsafe void Write(string s) {
			Platform.Print(s);
		}

		public static unsafe void WriteLine(string s = "") {
			Platform.PrintLine(s);
		}

		public static unsafe string ReadLine() {
			var buf = stackalloc char[MAX_INPUT_LENGTH];
			var i = 0;

			// TODO: Replace ReadKey with WaitForEvent and ReadKeyStroke
			for (; ; ) {
				var ki = ReadKey();
				var c = ki.KeyChar;

				if (ki.Key == ConsoleKey.Enter)	// Enter
					break;

				if (i == MAX_INPUT_LENGTH - 1)
					continue;

				if (c >= 32)            // Ignore all non-alphanumeric-or-symbol characters
					buf[i++] = c;
				else if (c == '\x08' && i > 0)   // Backspace
					i--;
			}

			//var x = stackalloc char[3];
			//x[0] = '\r';
			//x[1] = '\n';
			//x[2] = '\0';
			//EFI.EFI.ST.Ref.ConOut.Ref.OutputString(x);

			return new string(buf, 0, i);
		}

		public static int HandleANSIEscapeSequence(string text, int charPos) {
			char c = text[charPos++];
			int processed = 0;

			switch (c) {
				case '[': {
						processed++;
						c = text[charPos++];
						int v;

						for (; ; ) {
							if (c == 'm') {
								processed++;
								break;
							}

							if (c == ';') {
								processed++;
								c = text[charPos++];
								continue;
							}

							if (!Char.IsDigit(c))
								return 0;

							v = 0;

							for (; ; ) {
								v *= 10;
								v += c - '0';
								processed++;
								c = text[charPos++];

								if (!Char.IsDigit(c))
									break;
							}

							if (v >= 30 && v <= 37)
								Platform.SetConsoleForegroundColour((ConsoleColor)(v - 30));
							else if (v >= 40 && v <= 47)
								Platform.SetConsoleBackgroundColour((ConsoleColor)(v - 40));
							else if (v >= 90 && v <= 97)
								Platform.SetConsoleForegroundColour((ConsoleColor)(v - 82));
							else if (v >= 100 && v <= 107)
								Platform.SetConsoleBackgroundColour((ConsoleColor)(v - 92));
							else if (v == 39)
								Platform.SetConsoleForegroundColour(ConsoleColor.White);
							else if (v == 49)
								Platform.SetConsoleBackgroundColour(ConsoleColor.Black);
							else if (v == 0) {
								Platform.SetConsoleBackgroundColour(ConsoleColor.Black);
								Platform.SetConsoleForegroundColour(ConsoleColor.White);
							}
						}
					}
					break;
				default:
					return 0;
			}

			return processed;
		}
	}
}