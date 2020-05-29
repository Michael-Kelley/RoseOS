![RoseOS Icon](./rose_os_icon.svg)
# RoseOS
## A UEFI loader and OS kernel written in C# and compiled to native code with CoreRT.

*RoseOS is a hobby OS and currently a very early WIP. As such, many features that you might expect from an OS are missing, and some existing ones may be completely broken. Future commits will likely change the structure and inner workings of the OS drastically.*

---

### **Requirements**
* Visual Studio 2019
* .Net Core 3.1 SDK

### **First-time build**
1. Build NativeLib with Visual Studio using the Debug configuration.
2. Publish the kernel: `dotnet publish -r win-x64 -c debug kernel`
3. Publish the loader: `dotnet publish -r win-x64 -c debug loader`

*NOTE: If you later make modifications to NativeLib, make sure to re-publish both the kernel and the loader, as both projects utilise the NativeLib library.*

### **Running the loader**
After a successful publish of the loader project, QEMU will automatically run and boot straight in to the loader. The loader will prompt the user to select a graphics mode, and then load the kernel. When prompted to select a graphics mode, use a 0-based index into the list that the loader displays (ie. to select the first mode, type 0, then press enter)

*NOTE: QEMU expects there to be an `os_drive` directory in the root directory, alongside `build` and the RoseOS solution file. It is not currently used for anything, but will later be used as a virtual drive to store the OS drivers, applications and user data.*