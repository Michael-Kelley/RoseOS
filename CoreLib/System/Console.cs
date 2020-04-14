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
		//		EFI.ST->ConOut->EnableCursor(EFI.ST->ConOut, value);
		//	}
		//}

		//public unsafe static ConsoleColor ForegroundColor {
		//	set {
		//		s_consoleAttribute = (ushort)value;
		//		uint color = s_consoleAttribute;
		//		EFI.ST->ConOut->SetAttribute(EFI.ST->ConOut, color);
		//	}
		//}

		//public static unsafe bool KeyAvailable {
		//	get {
		//		if (lastKey == '\0')
		//			return true;

		//		EFI_INPUT_KEY key;
		//		var errorCode = EFI.ST->ConIn->ReadKeyStroke(EFI.ST->ConIn, &key);
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
				EFI.ST->ConIn->Reset(EFI.ST->ConIn, false);

				EFI_INPUT_KEY key;
				var ec = EFI.ST->BootServices->WaitForSingleEvent(EFI.ST->ConIn->WaitForKey);
				ec = EFI.ST->ConIn->ReadKeyStroke(EFI.ST->ConIn, &key);
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
		//	EFI.ST->ConOut->SetCursorPosition(
		//		EFI.ST->ConOut,
		//		(uint)x,
		//		(uint)y);
		//}

		unsafe static void WriteChar(EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* ConOut, char data) {
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
			ConOut->OutputString(ConOut, x);
		}

		public static unsafe void Write(char c) {
			WriteChar(EFI.ST->ConOut, c);
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

			EFI.ST->ConOut->OutputString(EFI.ST->ConOut, x + i);
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

				if (c.KeyChar == '\x0D')    // Enter
					break;

				buf[i++] = c.KeyChar;
			}

			var x = stackalloc char[3];
			x[0] = '\r';
			x[1] = '\n';
			x[2] = '\0';
			EFI.ST->ConOut->OutputString(EFI.ST->ConOut, x);

			return new string(buf, 0, i);
		}
	}
}