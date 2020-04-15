using System;
using System.Runtime;
using System.Runtime.InteropServices;

public class EntryPoint {
	[NativeCallable(EntryPoint = "kernel_main")]
	public static void KernelMain() {
		// QEMU shutdown
		Native.outw(0xB004, 0x2000);
	}
}