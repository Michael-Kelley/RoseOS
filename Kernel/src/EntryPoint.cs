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
		Console.WriteLine("Hello from the kernel!\nThis is a line feed without carriage return,\r\nand this is with.");
		Console.WriteLine("Let's test some ANSI escape sequences :)");
		Console.WriteLine("\x27[30;107m *** \x27[31;49m *** \x27[32m *** \x27[33m *** \x27[34m *** \x27[35m *** \x27[36m *** \x27[37m *** ");
		Console.WriteLine("\x27[90m *** \x27[91m *** \x27[92m *** \x27[93m *** \x27[94m *** \x27[95m *** \x27[96m *** \x27[97m *** ");
		Console.WriteLine("\x27[39;40m *** \x27[30;41m *** \x27[42m *** \x27[43m *** \x27[44m *** \x27[45m *** \x27[46m *** \x27[47m *** ");
		Console.WriteLine("\x27[100m *** \x27[101m *** \x27[102m *** \x27[103m *** \x27[104m *** \x27[105m *** \x27[106m *** \x27[107m *** ");
	}

	static void Main() { }
}