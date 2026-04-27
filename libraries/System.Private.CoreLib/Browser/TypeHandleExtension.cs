using NetJs;

namespace System
{
    [NetJs.Reflectable(false)]
    public static class TypeHandleExtension
    {
        [Template("{i} & 0xFFFF")]
        public extern static int TypeHandle(this uint i);
        [Template("{i} >> 16")]
        public extern static int AssemblyHandle(this uint i);
        [Template("({value} & {flag}) != 0")]
        public extern static bool TypeHasFlag(this Enum value, Enum flag);
        public static bool IsIntegerNumeric(this KnownTypeHandle value)
        {
            return value switch
            {
                KnownTypeHandle.SystemByte => true,
                KnownTypeHandle.SystemSByte => true,
                KnownTypeHandle.SystemChar => true,
                KnownTypeHandle.SystemInt16 => true,
                KnownTypeHandle.SystemUInt16 => true,
                KnownTypeHandle.SystemInt32 => true,
                KnownTypeHandle.SystemUint32 => true,
                KnownTypeHandle.SystemInt64 => true,
                KnownTypeHandle.SystemUint64 => true,
                _ => false
            };
        }
    }
}
