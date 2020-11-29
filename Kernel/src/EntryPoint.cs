using System;
using System.Runtime.InteropServices;


public class EntryPoint {
	[UnmanagedCallersOnly(EntryPoint = "kernel_main", CallingConvention = CallingConvention.StdCall)]
	public static void KernelMain(IntPtr modulesSeg, FrameBuffer fb, MemoryMap mmap) {
		Debug.Initialise();

		for (int i = 0; i < mmap.Length; i++) {
			if (mmap[i].IsUsable)
				Allocator.AddFreePages((IntPtr)mmap[i].PhysicalStart, (uint)mmap[i].NumberOfPages);
		}
		
		Internal.Runtime.CompilerHelpers.StartupCodeHelpers.InitialiseRuntime(modulesSeg);

		FrameBuffer._instance = fb;
		Font.Initialise();

		IDT.Disable();
		GDT.Initialise();
		IDT.Initialise();
		IDT.Enable();

		Console.Clear();
		Console.WriteLine("Hello from the kernel!\nThis is a line feed without carriage return,\r\nand this is with.");
		Console.WriteLine("Let's test some ANSI escape sequences :)");
		Console.WriteLine("\x1b[30;107m *** \x1b[31;49m *** \x1b[32m *** \x1b[33m *** \x1b[34m *** \x1b[35m *** \x1b[36m *** \x1b[37m *** ");
		Console.WriteLine("\x1b[90m *** \x1b[91m *** \x1b[92m *** \x1b[93m *** \x1b[94m *** \x1b[95m *** \x1b[96m *** \x1b[97m *** ");
		Console.WriteLine("\x1b[39;40m *** \x1b[30;41m *** \x1b[42m *** \x1b[43m *** \x1b[44m *** \x1b[45m *** \x1b[46m *** \x1b[47m *** ");
		Console.WriteLine("\x1b[100m *** \x1b[101m *** \x1b[102m *** \x1b[103m *** \x1b[104m *** \x1b[105m *** \x1b[106m *** \x1b[107m *** \x1b[0m");
		Console.WriteLine();

		for (; ; ) {
			Console.Write("> ");
			var cmd = Console.ReadLine();

			switch (cmd) {
				case "mem":
					Console.Write("Allocations: ");
					Console.Write(Allocator.Allocations);
					Console.WriteLine();
					Console.Write("Total Memory: ");
					Console.Write(Allocator.TotalMemory);
					Console.WriteLine(" bytes");
					Console.Write("Free Memory: ");
					Console.Write(Allocator.FreeMemory);
					Console.WriteLine(" bytes");
					Console.Write("Used Memory: ");
					Console.Write(Allocator.UsedMemory);
					Console.Write(" bytes (");
					Console.Write(Allocator.UsedMemory / 4096);
					Console.WriteLine(" pages)");
					break;
			}

			cmd.Dispose();
		}
	}

	static void Main() { }
}