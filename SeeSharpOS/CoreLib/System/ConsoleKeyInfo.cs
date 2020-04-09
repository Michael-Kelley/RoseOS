
namespace System {
	public readonly struct ConsoleKeyInfo {
		public readonly ConsoleKey Key;
		public readonly char KeyChar;
		public readonly ConsoleModifiers Modifiers;

		public ConsoleKeyInfo(char keyChar, ConsoleKey key, bool shift, bool alt, bool ctrl) {
			Key = key;
			KeyChar = keyChar;
			Modifiers = ConsoleModifiers.None;

			if (alt) Modifiers |= ConsoleModifiers.Alt;
			if (shift) Modifiers |= ConsoleModifiers.Shift;
			if (ctrl) Modifiers |= ConsoleModifiers.Control;
		}
	}
}