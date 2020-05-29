using System;
using System.Runtime.InteropServices;

public static class Native {
	[DllImport("*")]
	public static extern void outb(ushort port, byte value);

	[DllImport("*")]
	public static extern void outw(ushort port, ushort value);

	[DllImport("*")]
	public static extern byte inb(ushort port);

	[DllImport("*")]
	public static extern IntPtr kmalloc(ulong size);

	[DllImport("*")]
	public static extern IntPtr krealloc(IntPtr ptr, ulong newSize);

	[DllImport("*")]
	public static extern IntPtr kcalloc(ushort num, ushort size);

	[DllImport("*")]
	public static extern void kfree(IntPtr ptr);
}