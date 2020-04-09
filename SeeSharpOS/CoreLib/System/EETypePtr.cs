using System.Runtime.InteropServices;

using Internal.Runtime;

namespace System {
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EETypePtr {
		public EEType* _value;
	}
}