using System.Runtime.CompilerServices;

using Internal.Runtime.CompilerServices;

namespace System {
	public sealed class String {
		// The layout of the string type is a contract with the compiler.
		public readonly int Length;
		public char _firstChar;

		public unsafe char this[int index] {
			[Intrinsic]
			get {
				return Unsafe.Add(ref _firstChar, index);
			}
		}

		//public extern String();
#pragma warning disable 824
		public extern unsafe String(char* ptr, int index, int length);
#pragma warning restore 824

		//unsafe string Ctor() {
		//	var data = EFI.AllocatePool(4UL + 1);
		//	var s = Unsafe.As<IntPtr, String>(ref data);
		//	s._firstChar = '\0';

		//	return s;
		//}

		static unsafe string Ctor(char* ptr, int index, int length) {
			var start = ptr + index;

			var data = EFI.Allocate(4UL + (ulong)length + 1);
			var s = Unsafe.As<IntPtr, string>(ref data);
			EFI.ST->BootServices->CopyMem(data, (IntPtr)(&length), 4);

			fixed (char* c = &s._firstChar) {
				EFI.ST->BootServices->CopyMem((IntPtr)c, (IntPtr)start, (ulong)length * sizeof(char));
				c[length] = '\0';
			}

			return s;
		}

		//unsafe string Ctor(char[]? v) {
		//	if (v == null)
		//		return "";

		//	return "";

		//	//var length = 10;
		//	//var data = EFI.AllocatePool(4UL + (ulong)length + 1);
		//	//var s = Unsafe.As<IntPtr, String>(ref data);
		//	//EFI.ST->BootServices->CopyMem(data, (byte*)&length, 4);

		//	//fixed (char* c = &s._firstChar) {
		//	//	EFI.ST->BootServices->CopyMem((byte*)c, (byte*)&v[0], (ulong)length);
		//	//	c[length] = '\0';
		//	//}
		//}

		//unsafe string Ctor(char[] value, int index, int length) {
		//	return "";
		//}

		//unsafe string Ctor(char* ptr) {
		//	return "";
		//}

		//unsafe string Ctor(sbyte* ptr) {
		//	return "";
		//}

		//unsafe string Ctor(sbyte* ptr, int index, int length) {
		//	return "";
		//}

		//unsafe string Ctor(sbyte* ptr, int index, int length, System.Text.Encoding? enc) {
		//	return "";
		//}

		//unsafe string Ctor(char c, int count) {
		//	return "";
		//}

		//unsafe string Ctor(ReadOnlySpan<char> value) {
		//	return "";
		//}
	}
}
