using System;

public abstract class Debug {
	const ushort COM1 = 0x3f8;

	public static void Initialise() {
		Native.outb(COM1 + 1, 0x00);    // Disable all interrupts
		Native.outb(COM1 + 3, 0x80);    // Enable DLAB (set baud rate divisor)
		Native.outb(COM1 + 0, 0x03);    // Set divisor to 3 (lo byte) 38400 baud
		Native.outb(COM1 + 1, 0x00);    //                  (hi byte)
		Native.outb(COM1 + 3, 0x03);    // 8 bits, no parity, one stop bit
		Native.outb(COM1 + 2, 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
		Native.outb(COM1 + 4, 0x0B);    // IRQs enabled, RTS/DSR set
	}

	public static void Print(string msg) {
		for (int i = 0; i < msg.Length; i++) {
			var c = msg[i];
			while ((Native.inb(COM1 + 5) & 0x20) == 0) { }	// Wait until we can send a character
			Native.outb(COM1, (byte)(c >> 8));
			while ((Native.inb(COM1 + 5) & 0x20) == 0) { }
			Native.outb(COM1, (byte)(c & 0xFF));
		}
	}

	public static void Print(char c) {
		while ((Native.inb(COM1 + 5) & 0x20) == 0) { }
		Native.outb(COM1, (byte)(c >> 8));
		while ((Native.inb(COM1 + 5) & 0x20) == 0) { }
		Native.outb(COM1, (byte)(c & 0xFF));
	}
}