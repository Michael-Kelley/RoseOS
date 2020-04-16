using System;
using System.Runtime;
using System.Runtime.InteropServices;

public class EntryPoint {
	[NativeCallable(EntryPoint = "kernel_main")]
	public static unsafe void KernelMain(EFI_SYSTEM_TABLE* efi) {
		EFI.Initialize(efi);
		Platform.Print("\r\nHello from the kernel!\r\n");
	}
}