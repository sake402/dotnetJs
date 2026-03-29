using NetJs;

namespace System
{
    [External]
    public static class TypeHandleExtension
    {
        [Template("{i} & 0xFFFF")]
        public extern static int TypeHandle(this uint i);
        [Template("{i} >> 16")]
        public extern static int AssemblyHandle(this uint i);
        [Template("({value} & {flag}) != 0")]
        public extern static bool TypeHasFlag(this Enum value, Enum flag);
    }
}
