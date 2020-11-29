using System;
using System.Runtime.InteropServices;


public struct MemoryMap {
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Descriptor {
		public readonly uint Type;
		public readonly ulong PhysicalStart;
		public readonly ulong VirtualStart;
		public readonly ulong NumberOfPages;
		public readonly ulong Attribute;

		public bool IsUsable
			=> Type == 7 || (Type >= 1 && Type <= 4);
	}


	readonly IntPtr _pointer;
	readonly ulong descriptorSize;


	public int Length { get; private set; }
	public ulong Key { get; private set; }


	public unsafe Descriptor this[int index]
		=> *(Descriptor*)(_pointer + (uint)index * (uint)descriptorSize);
}