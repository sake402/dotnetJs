
using dotnetJs;
using System.Runtime.CompilerServices;

namespace System
{
    public partial class Object
    {
        public extern object? this[string name]
        {
            [dotnetJs.External]
            get;
            [dotnetJs.External]
            set;
        }


        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        public virtual extern string ToLocaleString();

        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        public virtual extern object? ValueOf();

        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        public virtual extern bool HasOwnProperty(object v);

        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        public virtual extern bool IsPrototypeOf(object v);

        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        public virtual extern bool PropertyIsEnumerable(object v);

        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        [dotnetJs.Template("Object.getOwnPropertyNames({obj})")]
        [dotnetJs.Unbox(true)]
        public static extern string[] GetOwnPropertyNames(object obj);

        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        [dotnetJs.Template("{T}.prototype")]
        public static extern TypePrototype GetPrototype<T>();
        [dotnetJs.Convention(dotnetJs.Notation.CamelCase)]
        [dotnetJs.Template("Object.getPrototypeOf({value})")]
        public static extern TypePrototype GetPrototypeOf(object value);


        [dotnetJs.Template("{global.}$clone({this:!super})")]
        [dotnetJs.MemberReplace(nameof(MemberwiseClone))]
        protected extern object IntrisicMemberwiseClone();

        [dotnetJs.MemberReplace(nameof(GetType))]
        [dotnetJs.StaticCallConvention]
        public Type IntrisicGetType()
        {
            var value = this;
            if (value == null)
                throw new NullReferenceException();
            var prototype = Object.GetPrototypeOf(value);
            if (prototype.Type != null)
                return prototype.Type;
            prototype = dotnetJs.Script.Write<TypePrototype>("value.constructor");
            if (prototype?.Type != null)
                return prototype.Type;
            var jsType = dotnetJs.Script.TypeOf(value);
            switch (jsType)
            {
                case "number":
                    return typeof(double);
                case "string":
                    return typeof(string);
                case "boolean":
                    return typeof(bool);
            }
            return typeof(object);
        }
        [dotnetJs.MemberReplace(nameof(ToString))]
        //[dotnetJs.StaticCallConvention]
        [dotnetJs.Template("{global.}" + dotnetJs.Constants.ToStringName + "({this:!super}, \"\")")] //make sure we dont pass super keyword in here. JS doesnt support it
        public virtual string ToStringImpl()
        {
            return GetType().ToString();
        }
        [dotnetJs.MemberReplace(nameof(GetHashCode))]
        [dotnetJs.Template("{global.}" + dotnetJs.Constants.GetHashCodeName + "({this:!super})")] //make sure we dont pass super keyword in here. JS doesnt support it
        public virtual int GetHashCodeImpl()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        const bool FieldLayoutByByte = false;
        
        #region ObjectFieldAccess

        [Name(Constants.StructFieldsLayoutName)]
        object[] _fields;
        //public extern object? this[int offset]
        //{
        //    [dotnetJs.External]
        //    get;
        //    [dotnetJs.External]
        //    set;
        //}
        byte GetByte(int offset) => _fields[offset].As<byte>();
        void SetByte(int offset, byte value) => _fields[offset] = value;

        sbyte GetSByte(int offset) => _fields[offset].As<sbyte>();
        void SetSByte(int offset, sbyte value) => _fields[offset] = value;


        ushort GetUShort(int offset)
        {
            if (FieldLayoutByByte)
                return (_fields[offset].As<int>() | (_fields[offset + 1].As<int>() << 8)).As<ushort>();
            else
                return _fields[offset].As<ushort>();
        }

        void SetUShort(int offset, ushort value)
        {
            if (FieldLayoutByByte)
            {
                _fields[offset] = value & 0xFF;
                _fields[offset + 1] = (value >> 8) & 0xFF;
            }
            else
            {
                _fields[offset] = value;
            }
        }


        short GetShort(int offset)
        {
            if (FieldLayoutByByte)
                return (_fields[offset].As<int>() | (_fields[offset + 1].As<int>() << 8)).As<short>();
            else
                return _fields[offset].As<short>();
        }

        void SetShort(int offset, short value)
        {
            if (FieldLayoutByByte)
            {
                _fields[offset] = value & 0xFF;
                _fields[offset + 1] = (value >> 8) & 0xFF;
            }
            else
            {
                _fields[offset] = value;
            }
        }


        uint GetUInt(int offset)
        {
            if (FieldLayoutByByte)
            {
                return (
                _fields[offset].As<uint>() |
                (_fields[offset + 1].As<uint>() << 8) |
                (_fields[offset + 2].As<uint>() << 16) |
                (_fields[offset + 3].As<uint>() << 24)
                ).As<uint>();
            }
            else
            {
                return _fields[offset].As<uint>();
            }
        }

        void SetUInt(int offset, uint value)
        {
            if (FieldLayoutByByte)
            {
                _fields[offset] = value & 0xFF;
                _fields[offset + 1] = (value >> 8) & 0xFF;
                _fields[offset + 1] = (value >> 16) & 0xFF;
                _fields[offset + 1] = (value >> 24) & 0xFF;
            }
            else
            {
                _fields[offset] = value;
            }
        }

        int GetInt(int offset)
        {
            if (FieldLayoutByByte)
            {
                return (
                _fields[offset].As<int>() |
                (_fields[offset + 1].As<int>() << 8) |
                (_fields[offset + 2].As<int>() << 16) |
                (_fields[offset + 3].As<int>() << 24)
                ).As<int>();
            }
            else
            {
                return _fields[offset].As<int>();
            }
        }

        void SetInt(int offset, int value)
        {
            if (FieldLayoutByByte)
            {
                _fields[offset] = value & 0xFF;
                _fields[offset + 1] = (value >> 8) & 0xFF;
                _fields[offset + 2] = (value >> 16) & 0xFF;
                _fields[offset + 3] = (value >> 24) & 0xFF;
            }
            else
            {
                _fields[offset] = value;
            }
        }

        ulong GetULong(int offset)
        {
            if (FieldLayoutByByte)
            {
                return (
                _fields[offset].As<ulong>() |
                (_fields[offset + 1].As<ulong>() << 8) |
                (_fields[offset + 2].As<ulong>() << 16) |
                (_fields[offset + 3].As<ulong>() << 24) |
                (_fields[offset + 4].As<ulong>() << 32) |
                (_fields[offset + 5].As<ulong>() << 40) |
                (_fields[offset + 6].As<ulong>() << 48) |
                (_fields[offset + 7].As<ulong>() << 56)
                ).As<ulong>();
            }
            else
            {
                return _fields[offset].As<ulong>();
            }
        }

        void SetULong(int offset, ulong value)
        {
            if (FieldLayoutByByte)
            {
                _fields[offset] = value & 0xFF;
                _fields[offset + 1] = (value >> 8) & 0xFF;
                _fields[offset + 2] = (value >> 16) & 0xFF;
                _fields[offset + 3] = (value >> 24) & 0xFF;
                _fields[offset + 4] = (value >> 32) & 0xFF;
                _fields[offset + 5] = (value >> 40) & 0xFF;
                _fields[offset + 6] = (value >> 48) & 0xFF;
                _fields[offset + 7] = (value >> 56) & 0xFF;
            }
            else
            {
                _fields[offset] = value;
            }
        }

        long GetLong(int offset)
        {
            if (FieldLayoutByByte)
            {
                return (
                _fields[offset].As<long>() |
                (_fields[offset + 1].As<long>() << 8) |
                (_fields[offset + 2].As<long>() << 16) |
                (_fields[offset + 3].As<long>() << 24) |
                (_fields[offset + 4].As<long>() << 32) |
                (_fields[offset + 5].As<long>() << 40) |
                (_fields[offset + 6].As<long>() << 48) |
                (_fields[offset + 7].As<long>() << 56)
                ).As<long>();
            }
            else
            {
                return _fields[offset].As<long>();
            }
        }

        void SetLong(int offset, ulong value)
        {
            if (FieldLayoutByByte)
            {
                _fields[offset] = value & 0xFF;
                _fields[offset + 1] = (value >> 8) & 0xFF;
                _fields[offset + 2] = (value >> 16) & 0xFF;
                _fields[offset + 3] = (value >> 24) & 0xFF;
                _fields[offset + 4] = (value >> 32) & 0xFF;
                _fields[offset + 5] = (value >> 40) & 0xFF;
                _fields[offset + 6] = (value >> 48) & 0xFF;
                _fields[offset + 7] = (value >> 56) & 0xFF;
            }
            else
            {
                _fields[offset] = value;
            }
        }

        object GetField(int offset) => _fields[offset].As<object>();
        void SetField(int offset, object value) => _fields[offset] = value;
        #endregion
        //[dotnetJs.MemberReplace(nameof(ToString), dotnetJs.MemberReplaceType.Attributes)]
        //[dotnetJs.StaticCallConvention]
        //Make .ToString static so it can be called with instance of native types that doesn't have ToString by default
        //public virtual extern string? OverrideToString();
    }
}
