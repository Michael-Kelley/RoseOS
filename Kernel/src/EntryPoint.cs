using System;
using System.Runtime.InteropServices;

public class EntryPoint {
	[UnmanagedCallersOnly(EntryPoint = "kernel_main", CallingConvention = CallingConvention.StdCall)]
	public static void KernelMain(IntPtr modulesSeg, FrameBuffer fb, MemoryMap mmap) {
		Debug.Initialise();

		for (int i = 0; i < mmap.Length; i++) {
			if (mmap[i].IsUsable)
				Allocator.AddMap((IntPtr)mmap[i].PhysicalStart, (uint)mmap[i].NumberOfPages);
		}
		
		Internal.Runtime.CompilerHelpers.StartupCodeHelpers.InitialiseRuntime(modulesSeg);

		FrameBuffer._instance = fb;
		Font.Initialise();

		Console.Clear();
		Console.Write("Hello from the kernel!");
	}

	static void Main() { }
}