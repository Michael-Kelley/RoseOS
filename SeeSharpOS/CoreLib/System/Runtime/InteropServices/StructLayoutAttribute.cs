
namespace System.Runtime.InteropServices {
	public sealed class StructLayoutAttribute : Attribute {
		public StructLayoutAttribute(LayoutKind layoutKind) { }
	}

	public enum LayoutKind {
		Sequential = 0,
		Explicit = 2,
		Auto = 3,
	}
}