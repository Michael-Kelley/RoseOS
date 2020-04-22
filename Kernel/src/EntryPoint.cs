using System;
using System.Runtime.InteropServices;

using Internal.Runtime.CompilerServices;

public class EntryPoint {
	[NativeCallable(EntryPoint = "kernel_main")]
	public static void KernelMain(FrameBuffer fb) {
		fb.Fill(fb.MakePixel(50, 150, 255));
	}
}