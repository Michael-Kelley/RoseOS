using System;
using System.Runtime;
using System.Runtime.InteropServices;

public static class Native {
	[DllImport("*")]
	public static extern int add_1_to_value(int value);
}

public static class Application {
	[RuntimeExport("EfiMain")]
	unsafe static long EfiMain(IntPtr imageHandle, EFI_SYSTEM_TABLE* systemTable) {
		EFI.Initialize(systemTable);
		Console.Clear();
		var v = Native.add_1_to_value(1337);
		Console.Write((ushort)v);
		Console.WriteLine("");
		var a = new A(42);

		DoSomething(a);
		Console.WriteLine("Hello! What is your name?");
		var s = Console.ReadLine();
		Console.Write("It's nice to meet you, ");
		Console.Write(s);
		Console.WriteLine(" :)");
		Console.WriteLine("Goodbye for now! Press any key to quit...");
		Console.ReadKey();
		Console.Clear();

		return 0;
	}

	static void DoSomething(A a) {
		Console.Write("a.B = ");
		Console.Write((ushort)a.B);
		Console.WriteLine("");
	}

	public static void Main() { }

	class A {
		public int B;

		public A(int b) {
			B = b;
		}
	}
}