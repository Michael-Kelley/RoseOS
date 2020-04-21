using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.CompilerServices;

namespace System {
	public sealed class String {
		// The layout of the string type is a contract with the compiler.
		int _length;
		internal char _firstChar;

		public int Length {
			get { return _length; }
			internal set { _length = value; }
		}

		public unsafe char this[int index] {
			[Intrinsic]
			get {
				return Unsafe.Add(ref _firstChar, index);
			}
		}

#pragma warning disable 824
		public extern unsafe String(char* ptr);
		public extern String(IntPtr ptr);
		public extern String(char[] buf);
		public extern unsafe String(char* ptr, int index, int length);
#pragma warning restore 824

		public unsafe static string FromASCII(IntPtr ptr, int length) {
			var buf = new char[9];
			buf[8] = '\0';
			var _ptr = (byte*)ptr;

			for (int i = 0; i < length; i++)
				buf[i] = (char)_ptr[i];

			var s = new string(buf);
			buf.Dispose();

			return s;
		}

		static unsafe string Ctor(char* ptr) {
			var i = 0;

			while (ptr[i++] != '\0') { }

			return Ctor(ptr, 0, i - 1);
		}

		static unsafe string Ctor(IntPtr ptr)
			=> Ctor((char*)ptr);

		static unsafe string Ctor(char[] buf) {
			fixed (char* _buf = buf)
				return Ctor(_buf, 0, buf.Length);
		}

		static unsafe string Ctor(char* ptr, int index, int length) {
			var et = EETypePtr.EETypePtrOf<string>();

			var start = ptr + index;
			var data = StartupCodeHelpers.RhpNewArray(et.Value, length);
			var s = Unsafe.As<object, string>(ref data);

			fixed (char* c = &s._firstChar) {
				Platform.CopyMemory((IntPtr)c, (IntPtr)start, (ulong)length * sizeof(char));
				c[length] = '\0';
			}

			return s;
		}

		public override string ToString() {
			return this;
		}

		// TODO: This
		public static string Format(string format, params object[] args) {
			var len = format.Length;

			for (int i = 0; i < len; i++) {
				args[0].ToString();
			}

			return format;
		}
	}
}