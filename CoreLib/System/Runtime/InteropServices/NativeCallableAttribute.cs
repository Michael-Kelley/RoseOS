
namespace System.Runtime.InteropServices {
    public sealed class NativeCallableAttribute : Attribute {
        public string EntryPoint;
        public CallingConvention CallingConvention;

        public NativeCallableAttribute() { }
    }
}