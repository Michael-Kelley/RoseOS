using System;
using System.Runtime.InteropServices;

using Internal.Runtime.CompilerServices;

public class EntryPoint {
	[NativeCallable(EntryPoint = "kernel_main")]
	public static unsafe void KernelMain(IntPtr pFb) {
		var fb = Unsafe.As<IntPtr, FrameBuffer>(ref pFb);
		fb.Fill(fb.MakePixel(50, 150, 255));
	}
}