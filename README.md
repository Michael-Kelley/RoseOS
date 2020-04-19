# SeeSharpOS
## A UEFI loader and OS kernel written in C# and compiled to native code with CoreRT.
---

### **First-time build**
1. Build NativeLib with Visual Studio 2019 using the Debug configuration.
2. Publish the kernel: `dotnet publish -r win-x64 -c debug kernel`
3. Publish the loader: `dotnet publish -r win-x64 -c debug loader`

*NOTE: If you later make modifications to NativeLib, make sure to re-publish both the kernel and the loader, as both projects utilise the NativeLib library.*

### **Running the loader**
After a successful publish of the loader project, QEMU will automatically run and boot straight in to the loader. The loader will prompt the user to select a graphics mode, and then load the kernel.

*NOTE: QEMU expects there to be an `os_drive` directory in the root directory, alongside `build` and the SeeSharOS solution file. It is not currently used for anything, but will later be used as a virtual drive to store the OS drivers, applications and user data.*

### **Note for João**
STOP TRYING TO BUILD THE LOADER AND KERNEL IN VISUAL STUDIO. THIS WILL ONLY BUILD THE .NET VERSIONS, WHICH IS NO GOOD. THEY NEED TO BE PUBLISHED SO THAT CORERT CAN COMPILE THEM TO NATIVE BINARIES. PUBLISHING THEM ALSO BUILDS THEM.