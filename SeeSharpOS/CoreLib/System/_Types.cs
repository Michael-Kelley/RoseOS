
namespace System {
	public struct Void { }

	// The layout of primitive types is special cased because it would be recursive.
	// These really don't need any fields to work.
	public struct Boolean { }
	public struct Char { }
	public struct SByte { }
	public struct Byte { }
	public struct Int16 { }
	public struct UInt16 { }
	public struct Int32 { }
	public struct UInt32 { }
	public struct Int64 { }
	public struct UInt64 { }
	public struct UIntPtr { }
	public struct Single { }
	public struct Double { }

	public abstract class ValueType { }
	public abstract class Enum : ValueType { }

	public struct Nullable<T> where T : struct { }

	public abstract class Array { }
	public abstract class Delegate { }
	public abstract class MulticastDelegate : Delegate { }

	public struct RuntimeTypeHandle { }
	public struct RuntimeMethodHandle { }
	public struct RuntimeFieldHandle { }
}