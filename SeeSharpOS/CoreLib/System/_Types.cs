
namespace System {
	public struct Void { }

	// The layout of primitive types is special cased because it would be recursive.
	// These really don't need any fields to work.
	public struct Boolean {
		public override string ToString()
			=> this ? "true" : "false";
	}

	public struct Char {
		public override string ToString() {
			var r = " ";
			r._firstChar = this;

			return r;
		}
	}

	public struct SByte { }

	public struct Byte { }

	public struct Int16 { }

	public struct UInt16 { }

	public struct Int32 {
		// TODO: ToString for all other primitives
		public unsafe override string ToString() {
			var val = this;
			bool isNeg = BitHelpers.IsBitSet(val, 31);
			char* x = stackalloc char[12];
			var i = 10;

			x[11] = '\0';

			do {
				var d = val % 10;
				val /= 10;

				d += 0x30;
				x[i--] = (char)d;
			} while (val > 0);

			if (isNeg)
				x[i] = '-';
			else
				i++;

			return new string(x + i, 0, 11 - i);
		}
	}

	public struct UInt32 {
		public unsafe override string ToString() {
			var val = this;
			char* x = stackalloc char[11];
			var i = 9;

			x[10] = '\0';

			do {
				var d = val % 10;
				val /= 10;

				d += 0x30;
				x[i--] = (char)d;
			} while (val > 0);

			i++;

			return new string(x + i, 0, 10 - i);
		}
	}

	public struct Int64 { }

	public struct UInt64 { }

	public struct UIntPtr { }
	public struct Single { }
	public struct Double { }

	public abstract class ValueType { }
	public abstract class Enum : ValueType { }

	public struct Nullable<T> where T : struct { }

	public abstract class Delegate { }
	public abstract class MulticastDelegate : Delegate { }

	public struct RuntimeTypeHandle { }
	public struct RuntimeMethodHandle { }
	public struct RuntimeFieldHandle { }
}