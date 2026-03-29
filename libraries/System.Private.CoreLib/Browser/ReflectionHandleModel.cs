using System.Text.Json.Serialization;

namespace System
{
    public struct ReflectionHandleModel
    {
        public const int AssemblyShift = 0;
        public const ulong AssemblyMask = 0xFFFF;
        public const int TypeShift = 16;
        public const ulong TypeMask = 0xFFFF0000;
        public const int MemberShift = 32;
        public const ulong MemberMask = 0xFFFF00000000;
        [NetJs.Name("v")]
        [JsonPropertyName("v")]
        public ulong Value { get; set; }
        [JsonIgnore]
        public int Assembly
        {
            [NetJs.Template("{this}.v & 0xFFFF")]
            get
            {
                return (int)(Value & AssemblyMask);
            }
        }

        [JsonIgnore]
        public int Type
        {
            [NetJs.Template("({this}.v & 0xFFFF0000) >> 16")]
            get
            {
                return (int)((Value & TypeMask) >> TypeShift);
            }
        }

        [JsonIgnore]
        public int Member
        {
            [NetJs.Template("({this}.v & 0xFFFF00000000) >> 32")]
            get
            {
                return (int)((Value & MemberMask) >> MemberShift);
            }
        }
        
        [JsonIgnore]
        public int AssemblyAndType
        {
            [NetJs.Template("({this}.v & (0xFFFF | 0xFFFF0000))")]
            get
            {
                return (int)(Value & (AssemblyMask | TypeMask));
            }
        }
    }

}