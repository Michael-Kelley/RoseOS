using System;

public static class Application {
	static unsafe int Main() {
		Console.Write("Attach now!");
		Console.ReadKey();
		Console.Write("Hello, ");
		var c = stackalloc char[] { 'W', 'o', 'r', 'l', 'd', '!', '\0' };
		var s = new string(c);
		Console.WriteLine(s);

		return 0;
	}

	// TODO: Delete this once String.Format is implemented
	static unsafe void PrintLine(params object[] args) {
		for (int i = 0; i < args.Length; i++)
			Console.Write(args[i].ToString());

		Console.WriteLine();
	}
}