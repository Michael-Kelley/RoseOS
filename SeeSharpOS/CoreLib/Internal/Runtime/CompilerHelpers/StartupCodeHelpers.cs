using System;
using System.Runtime;

using Internal.Runtime.CompilerServices;

namespace Internal.Runtime.CompilerHelpers {
	class StartupCodeHelpers {
		[RuntimeExport("RhpReversePInvoke2")]
		static void RhpReversePInvoke2(IntPtr frame) { }
		[RuntimeExport("RhpReversePInvokeReturn2")]
		static void RhpReversePInvokeReturn2(IntPtr frame) { }
		[RuntimeExport("RhpPInvoke")]
		static void RhpPinvoke(IntPtr frame) { }
		[RuntimeExport("RhpPInvokeReturn")]
		static void RhpPinvokeReturn(IntPtr frame) { }
		[RuntimeExport("RhpNewFast")]
		static unsafe object RhpNewFast(EETypePtr pEEType) {
			var data = EFI.Allocate(pEEType._value->BaseSize);

			return Unsafe.As<IntPtr, object>(ref data);
		}
	}
}