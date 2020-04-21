using System;
using System.Runtime.InteropServices;

namespace NativeTypeWrappers {
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct ReadonlyNativeString {
		readonly IntPtr _pointer;

		public override string ToString()
			=> new string(_pointer);
	}
}