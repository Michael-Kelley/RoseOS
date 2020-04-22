#if !PLATFORM_KERNEL

using NativeTypeWrappers;

namespace System {
	public static class Console {
		static ushort s_consoleAttribute;
		static ushort s_cursorX;
		static ushort s_cursorY;
#pragma warning disable 169
		static ushort s_windowSizeX;
		static ushort s_windowSizeY;
#pragma warning restore 169

		static char lastKey = '\0';
		static ushort lastScanCode;


		//public static unsafe bool CursorVisible {
		//	set {
		//		EFI.ST.Ref.ConOut->EnableCursor(EFI.ST.Ref.ConOut, value);
		//	}
		//}

		//public unsafe static ConsoleColor ForegroundColor {
		//	set {
		//		s_consoleAttribute = (ushort)value;
		//		uint color = s_consoleAttribute;
		//		EFI.ST.Ref.ConOut->SetAttribute(EFI.ST.Ref.ConOut, color);
		//	}
		//}

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
			// TODO: Change this to use Native instead of platform-specific code
			if (lastKey == '\0') {
				EFI.EFI.ST.Ref.ConIn.Ref.Reset(false);

				var ec = EFI.EFI.ST.Ref.BootServices.Ref.WaitForSingleEvent(EFI.EFI.ST.Ref.ConIn.Ref.WaitForKey);
				ec = EFI.EFI.ST.Ref.ConIn.Ref.ReadKeyStroke(out var key);
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

			//var c = Native.ReadKey();

			//return new ConsoleKeyInfo(c, default, false, false, false);
		}

		//public unsafe static void SetCursorPosition(int x, int y) {
		//	s_cursorX = (ushort)x;
		//	s_cursorY = (ushort)y;
		//	EFI.ST.Ref.ConOut->SetCursorPosition(
		//		EFI.ST.Ref.ConOut,
		//		(uint)x,
		//		(uint)y);
		//}

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

		public static unsafe void Write(char c) {
			WriteChar(ref EFI.EFI.ST.Ref.ConOut.Ref, c);
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

			EFI.EFI.ST.Ref.ConOut.Ref.OutputString(x + i);
		}

		public static unsafe void Write(string s) {
			Platform.Print(s);
		}

		public static unsafe void WriteLine(string s = "") {
			Platform.PrintLine(s);
		}

		public static unsafe string ReadLine() {
			var buf = stackalloc char[256];
			var i = 0;

			// TODO: Replace ReadKey with WaitForEvent and ReadKeyStroke
			while (true) {
				var c = ReadKey();

				if (c.KeyChar == '\x0D')		// Enter
					break;

				if (c.KeyChar >= 32)            // Ignore all non-alphanumeric-or-symbol characters
					buf[i++] = c.KeyChar;
				else if (c.KeyChar == '\x08' && i > 0)   // Backspace
					i--;
			}

			var x = stackalloc char[3];
			x[0] = '\r';
			x[1] = '\n';
			x[2] = '\0';
			EFI.EFI.ST.Ref.ConOut.Ref.OutputString(x);

			return new string(buf, 0, i);
		}
	}
}

#endif