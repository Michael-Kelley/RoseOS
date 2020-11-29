using System.Runtime;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;


static class IDT {
	[DllImport("*")]
	static unsafe extern void set_idt_entries(void* idt);


	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct IDTEntry {
		public ushort BaseLow;
		public ushort Selector;
		public byte Reserved0;
		public byte Type_Attributes;
		public ushort BaseMid;
		public uint BaseHigh;
		public uint Reserved1;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IDTDescriptor {
		public ushort Limit;
		public ulong Base;
	}


	static IDTEntry[] idt;
	static IDTDescriptor idtr;
	

	public static bool Initialised { get; private set; }


	public unsafe static void Initialise() {
		idt = new IDTEntry[256];

		// Remap PIC
		Native.outb(0x20, 0x11);
		Native.outb(0xA0, 0x11);
		Native.outb(0x21, 0x20);
		Native.outb(0xA1, 40);
		Native.outb(0x21, 0x04);
		Native.outb(0xA1, 0x02);
		Native.outb(0x21, 0x01);
		Native.outb(0xA1, 0x01);
		Native.outb(0x21, 0x0);
		Native.outb(0xA1, 0x0);

		// TODO: Figure out a way to do this in C#
		set_idt_entries(Unsafe.AsPointer(ref idt[0]));

		fixed (IDTEntry* _idt = idt) {
			// Fill IDT descriptor
			idtr.Limit = (ushort)((sizeof(IDTEntry) * 256) - 1);
			idtr.Base = (ulong)_idt;
		}

		Native.load_idt(ref idtr);

		//Enable keyboard interrupts
		Native.outb(0x21, 0xFD);
		Native.outb(0xA1, 0xFF);

		Initialised = true;
	}

	public static void Enable() {
		Native._sti();
	}

	public static void Disable() {
		Native._cli();
	}


	[RuntimeExport("exception_handler")]
	public static void ExceptionHandler(int code) {
		switch (code) {
			case 0: Debug.Print("!! DIVIDE BY ZERO !!"); break;
			case 1: Debug.Print("!! SINGLE STEP !!"); break;
			case 2: Debug.Print("!! NMI !!"); break;
			case 3: Debug.Print("!! BREAKPOINT !!"); break;
			case 4: Debug.Print("!! OVERFLOW !!"); break;
			case 5: Debug.Print("!! BOUNDS CHECK !!"); break;
			case 6: Debug.Print("!! INVALID OPCODE !!"); break;
			case 7: Debug.Print("!! COPR UNAVAILABLE !!"); break;
			case 8: Debug.Print("!! DOUBLE FAULT !!"); break;
			case 9: Debug.Print("!! COPR SEGMENT OVERRUN !!"); break;
			case 10: Debug.Print("!! INVALID TSS !!"); break;
			case 11: Debug.Print("!! SEGMENT NOT FOUND !!"); break;
			case 12: Debug.Print("!! STACK EXCEPTION !!"); break;
			case 13: Debug.Print("!! GENERAL PROTECTION !!"); break;
			case 14: Debug.Print("!! PAGE FAULT !!"); break;
			case 16: Debug.Print("!! COPR ERROR !!"); break;
			default: Debug.Print(" !! UNKNOWN EXCEPTION !!"); break;
		}
	}

	[RuntimeExport("irq0_handler")]
	public static void IRQ0Handler() {
		Debug.Print("! 0 !");
		Native.outb(0x20, 0x20);
	}

	// Keyboard interrupt handler
	[RuntimeExport("irq1_handler")]
	public static void IRQ1Handler() {
		//Debug.Print("! 1 !");
		var scan = Native.inb(0x60);
		Keyboard.Process(scan);		
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq2_handler")]
	public static void IRQ2Handler() {
		Debug.Print("! 2 !");
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq3_handler")]
	public static void IRQ3Handler() {
		Debug.Print("! 3 !");
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq4_handler")]
	public static void IRQ4Handler() {
		Debug.Print("! 4 !");
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq5_handler")]
	public static void IRQ5Handler() {
		Debug.Print("! 5 !");
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq6_handler")]
	public static void IRQ6Handler() {
		Debug.Print("! 6 !");
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq7_handler")]
	public static void IRQ7Handler() {
		Debug.Print("! 7 !");
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq8_handler")]
	public static void IRQ8Handler() {
		Debug.Print("! 8 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq9_handler")]
	public static void IRQ9Handler() {
		Debug.Print("! 9 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq10_handler")]
	public static void IRQ10Handler() {
		Debug.Print("! 10 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq11_handler")]
	public static void IRQ11Handler() {
		Debug.Print("! 11 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq12_handler")]
	public static void IRQ12Handler() {
		Debug.Print("! 12 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq13_handler")]
	public static void IRQ13Handler() {
		Debug.Print("! 13 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq14_handler")]
	public static void IRQ14Handler() {
		Debug.Print("! 14 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}

	[RuntimeExport("irq15_handler")]
	public static void IRQ15Handler() {
		Debug.Print("! 15 !");
		Native.outb(0xA0, 0x20);
		Native.outb(0x20, 0x20);
	}
}