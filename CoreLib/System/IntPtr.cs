using System.Runtime.CompilerServices;

namespace System {
	public unsafe struct IntPtr {
		void* _value;

		public IntPtr(void* value) { _value = value; }
		public IntPtr(int value) { _value = (void*)value; }
		public IntPtr(long value) { _value = (void*)value; }

		[Intrinsic]
		public static readonly IntPtr Zero;

		//public override bool Equals(object o)
		//	=> _value == ((IntPtr)o)._value;

		public bool Equals(IntPtr ptr)
			=> _value == ptr._value;

		//public override int GetHashCode()
		//	=> (int)_value;

		public static explicit operator IntPtr(int value) => new IntPtr(value);
		public static explicit operator IntPtr(long value) => new IntPtr(value);
		public static explicit operator IntPtr(void* value) => new IntPtr(value);
		public static explicit operator void*(IntPtr value) => value._value;

		public static explicit operator int(IntPtr value) {
			long l = (long)value._value;
			return checked((int)l);
		}

		public static explicit operator long(IntPtr value) => (long)value._value;
	}
}