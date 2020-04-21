using System.Runtime.InteropServices;

public static class Native {
	[DllImport("*")]
	public static extern void outw(ushort value, ushort port);
}