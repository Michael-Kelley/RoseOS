A UEFI application whose sole purpose is to prepare the system and load the kernel. It expects the kernel to be a 64-bit PE DLL stored as `kernel.bin` in the root of the boot partition (the same partition that contains this loader).