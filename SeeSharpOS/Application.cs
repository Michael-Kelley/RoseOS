using System;
using System.Runtime;
using System.Runtime.InteropServices;

using Internal.Runtime.CompilerServices;

//public static class Native {
//	[DllImport("*")]
//	public static extern int add_1_to_value(int value);
//}

public static class Application {
	public class A {
		public int I;

		public A(int i) {
			I = i;
		}
	}

#if WIN_TARGET

	[RuntimeExport("ConsoleMain")]
	static unsafe int ConsoleMain() {
		//Console.Clear();
		Console.Write("Attach now!");
		Console.ReadKey();
		Console.Write("Hello, ");
		var c = stackalloc char[] { 'W', 'o', 'r', 'l', 'd', '!', '\0' };
		var s = new string(c);
		Console.WriteLine(s);

		return 0;
	}

#else

	[RuntimeExport("EfiMain")]
	static unsafe long EfiMain(IntPtr imageHandle, EFI_SYSTEM_TABLE* systemTable) {
		EFI.Initialize(systemTable);
		var st = systemTable;
		var bs = st->BootServices;

		Console.Clear();

#if DEBUG
		bs->SetWatchdogTimer(0, 0);
#endif
		var vendor = new string(st->FirmwareVendor);
		PrintLine("UEFI Firmware Vendor:   ", vendor);
		var rev = st->FirmwareRevision;
		PrintLine("UEFI Firmware Revision: ", (uint)(rev >> 16), ".", (uint)(rev >> 20), ".", (uint)(rev >> 24));

		Console.ReadKey();

		return 0;
	}

#endif

	static void Main() { }

	// TODO: Delete this once String.Format is implemented
	static unsafe void PrintLine(params object[] args) {
		for (int i = 0; i < args.Length; i++)
			Console.Write(args[i].ToString());

		Console.WriteLine();
	}
}