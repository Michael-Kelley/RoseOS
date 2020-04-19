using System;

public class FrameBuffer {
	public enum PixelFormat {
		Unknown,
		R8G8B8,
		B8G8R8,
	}

	readonly IntPtr _ptr;
	readonly ulong _len;

	public readonly uint Width;
	public readonly uint Height;
	public readonly PixelFormat Format;

	public FrameBuffer(IntPtr pointer, ulong length, uint width, uint height, PixelFormat format) {
		_ptr = pointer;
		_len = length;
		Width = width;
		Height = height;
		Format = format;
	}

	public unsafe uint this[uint index] {
		get => ((uint*)_ptr)[index];
		set { ((uint*)_ptr)[index] = value; }
	}

	public unsafe uint this[uint x, uint y] {
		get => ((uint*)_ptr)[y * Width + x];
		set { ((uint*)_ptr)[y * Width + x] = value; }
	}

	public void Clear() {
		// TODO: Make this faster using ulong (fill length / 2, check if length % 2 == 1, if so set last uint)
		for (var i = 0u; i < _len; i++)
			this[i] = 0;
	}

	public void Fill(uint value) {
		for (var i = 0u; i < _len; i++)
			this[i] = value;
	}

	public uint MakePixel(byte r, byte g, byte b) {
		uint pixel = 0;

		switch (Format) {
			case PixelFormat.R8G8B8:
				pixel = r + ((uint)g << 8) + ((uint)b << 16);
				break;
			case PixelFormat.B8G8R8:
				pixel = b + ((uint)g << 8) + ((uint)r << 16);
				break;
		}

		return pixel;
	}
}