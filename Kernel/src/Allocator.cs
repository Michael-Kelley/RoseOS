using System;
using System.Runtime;
using System.Runtime.InteropServices;

public abstract class Allocator {
	const int TAG_ARRAY_LENGTH = 512;

	[StructLayout(LayoutKind.Sequential)]
	struct Tag {
		public IntPtr Address;
		public uint Pages;
		public bool IsFree;

		public Tag(IntPtr address, uint pages, bool free) {
			Address = address;
			Pages = pages;
			IsFree = free;
		}
	}

	unsafe struct DataChunk {
		public fixed byte Data[TAG_ARRAY_LENGTH * 16];
	}

	static DataChunk rawMap;
	static int count = 0;

	public static unsafe bool AddMap(IntPtr address, uint pages) {
		// TODO: "Defrag" tags by combining adjacent free memory regions into a single entry. Maybe think about doing this every time pages are freed?
		if (count == TAG_ARRAY_LENGTH)
			return false;

		Tag* map;

		fixed (byte* data = rawMap.Data)
			map = (Tag*)data;

		map[count++] = new Tag(address, pages, true);

		return true;
	}

	[RuntimeExport("liballoc_lock")]
	public static int Lock() {
		// TODO: Implement this
		return 0;
	}

	[RuntimeExport("liballoc_unlock")]
	public static int Unlock() {
		// TODO: Implement this
		return 0;
	}

	[RuntimeExport("liballoc_alloc")]
	public static unsafe IntPtr Alloc(ulong size) {
		Tag* map;

		fixed (byte* data = rawMap.Data)
			map = (Tag*)data;

		var smallest = 0x7FFFFFFF;
		int idx = -1;

		for (int i = 0; i < count; i++) {
			ref var _tag = ref map[i];

			if (!_tag.IsFree)
				continue;

			if (_tag.Pages < size)
				continue;

			if (_tag.Pages == size) {
				_tag.IsFree = false;

				return _tag.Address;
			}

			if (_tag.Pages < smallest) {
				smallest = (int)_tag.Pages;
				idx = i;
			}
		}

		if (idx == -1)
			return IntPtr.Zero;

		ref var tag = ref map[idx];
		var remPages = tag.Pages - size;
		var remAddr = tag.Address + (size * 4 * 1024);
		tag.Pages = (uint)size;
		tag.IsFree = false;

		AddMap(remAddr, (uint)remPages);

		return tag.Address;
	}

	[RuntimeExport("liballoc_free")]
	public static unsafe int Free(IntPtr ptr, ulong size) {
		Tag* map;

		fixed (byte* data = rawMap.Data)
			map = (Tag*)data;

		for (int i = 0; i < count; i++) {
			ref var tag = ref map[i];

			if (!tag.Address.Equals(ptr))
				continue;

			// TODO: Find out if this can ever happen, and manage it by freeing up the requested pages and added another entry for the remaining pages
			if (tag.Pages != size)
				return 1;

			tag.IsFree = true;

			return 0;
		}

		return 1;
	}
}