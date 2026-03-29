using NetJs;
using System.Runtime.CompilerServices;

namespace System
{
    public struct RefOrPointer<T>
    {
        internal T[]? _array;
        internal Func<int?, T> _getter;
        internal Action<T, int?> _setter;
        public int? ArrayOffset { get; set; }

        //If we cast a primitive pointer type like byte* to int*,
        //this holds the number of items to read(4) from the underlying byte array and return as the result
        //When a case from int* to byte*, this becomes -4
        internal int _primitiveWindowItems;
        internal ulong _primitiveWindowItemMask => _primitiveWindowItems switch
        {
            1 or -1 => 0xFF,
            2 or -2 => 0xFFFF,
            4 or -4 => 0xFFFFFFFF,
            8 or -8 => 0xFFFFFFFFFFFFFFFF,
            _ => 0
        };
        public RefOrPointer(Func<int?, T> getter, Action<T, int?> setter)
        {
            this._getter = getter;
            this._setter = setter;
        }

        [Name(Constants.RefValueName)]
        public T Value
        {
            get
            {
                if (_primitiveWindowItems > 0) //eg getting int from underlying byte[]
                {
                    ulong result = 0;
                    for (int i = 0; i < _primitiveWindowItems; i++)
                    {
                        result |= _getter(ArrayOffset + i).As<ulong>() << (i * 8);
                    }
                    return result.As<T>();
                }
                else if (_primitiveWindowItems < 0)  //eg getting byte from underlying int[]
                {
                    ulong result = (_getter(ArrayOffset).As<ulong>() >> ((Math.Abs(_primitiveWindowItems) - 1) * 8)) & _primitiveWindowItemMask;
                    return result.As<T>();
                }
                return _getter(ArrayOffset);
            }
            set
            {
                if (_primitiveWindowItems > 0) //eg setting int to underlying byte[]
                {
                    ulong val = value.As<ulong>();
                    for (int i = 0; i < _primitiveWindowItems; i++)
                    {
                        _setter((val >> (i * 8)).As<T>(), ArrayOffset + i);
                    }
                    return;
                }
                else if (_primitiveWindowItems < 0)  //eg setting byte to underlying int[]
                {
                    var currentValue = _getter(ArrayOffset).As<ulong>();
                    var off = (ArrayOffset ?? 0) % Math.Abs(_primitiveWindowItems);
                    var mask = _primitiveWindowItemMask << off;
                    currentValue &= ~mask;
                    currentValue |= value.As<ulong>() << off;
                    _setter(currentValue.As<T>(), ArrayOffset);
                    return;
                }
                _setter(value, ArrayOffset);
            }
        }

        public T[] ToArray(int length = -1)
        {
            if (_array == null)
                throw new InvalidOperationException("Not based on an array");
            if ((ArrayOffset == null || ArrayOffset == 0) && length < 0)
                return _array;
            int start = 0;
            if (ArrayOffset != null)
            {
                start = ArrayOffset.Value;
            }
            if (length < 0)
                length = _array.Length - start;
            var newArray = new T[length];
            Array.Copy(_array, start, newArray, 0, length);
            return newArray;
        }

        public RefOrPointer<T> Add(int offset)
        {
            return this with { ArrayOffset = (ArrayOffset ?? 0) + offset };
        }

        public RefOrPointer<T> this[int offset]
        {
            get
            {
                return this with { ArrayOffset = (ArrayOffset ?? 0) + offset };
            }
        }

        //private T v
        //{
        //    get
        //    {
        //        return Value;
        //    }
        //    set
        //    {
        //        Value = value;
        //    }
        //}

        public void CopyTo(RefOrPointer<T> dst, int count)
        {
            if (!Array.Is(Value) || !Array.Is(dst.Value))
            {
                throw new InvalidOperationException("Both ref must be an array");
            }
            for (int i = 0; i < count; i++)
            {
                dst.Value.As<Array>()[(dst.ArrayOffset ?? 0) + i] = Value.As<Array>()[(ArrayOffset ?? 0) + i];
            }
        }

        //public static implicit operator T(Ref<T> reference)
        //{
        //    return reference.Value;
        //}

        //public static implicit operator =(Ref<T> reference, T value)
        //{
        //    return reference.Value;
        //}

        public override string ToString()
        {
            return Value?.ToString() ?? base.ToString();
        }

        //public override object? ValueOf()
        //{
        //    return Value;
        //}

        [NetJs.Template("{0}")]
        [NetJs.Unbox(true)]
        public static extern unsafe ref T As<T>(void* obj);

        [NetJs.Template("{0}")]
        public static extern unsafe ref T FromPointer(void* pointer);

        [NetJs.Template("{0}")]
        public static extern unsafe T* ToPointer(ref T valueRef);
    }
}
