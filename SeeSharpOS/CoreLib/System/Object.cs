﻿using Internal.Runtime;

namespace System {
	public class Object {
		// The layout of object is a contract with the compiler.
		internal unsafe EEType* m_pEEType;

		public Object() { }
		~Object() { }

		//public virtual bool Equals(object o)
		//	=> false;

		//public virtual int GetHashCode()
		//	=> 0;

		public virtual string ToString()
			=> "{object}";
	}
}