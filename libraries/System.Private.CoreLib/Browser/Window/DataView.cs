using System;
using System.Runtime.CompilerServices;

namespace Window
{
    [NetJs.External]
    public class DataView
    {
        public extern DataView(ArrayBuffer buffer, int byteOffset = 0, int byteLength = -1);
        public extern ArrayBuffer buffer { get; }
        public extern int byteLength { get; }
        public extern int byteOffset { get; }

        public extern sbyte getInt8(int byteOffset);
        public extern byte getUint8(int byteOffset);
        public extern short getInt16(int byteOffset, bool littleEndian = false);
        public extern ushort getUint16(int byteOffset, bool littleEndian = false);
        public extern int getInt32(int byteOffset, bool littleEndian = false);
        public extern uint getUint32(int byteOffset, bool littleEndian = false);
        public extern float getFloat32(int byteOffset, bool littleEndian = false);
        public extern double getFloat64(int byteOffset, bool littleEndian = false);
        public extern long getBigInt64(int byteOffset, bool littleEndian = false);
        public extern ulong getBigUint64(int byteOffset, bool littleEndian = false);


        public extern void setInt8(int byteOffset, sbyte value);
        public extern void setUint8(int byteOffset, byte value);
        public extern void setInt16(int byteOffset, short value, bool littleEndian = false);
        public extern void setUint16(int byteOffset, ushort value, bool littleEndian = false);
        public extern void setInt32(int byteOffset, int value, bool littleEndian = false);
        public extern void setUint32(int byteOffset, uint value, bool littleEndian = false);
        public extern void setFloat32(int byteOffset, float value, bool littleEndian = false);
        public extern void setFloat64(int byteOffset, double value, bool littleEndian = false);
        public extern void setBigInt64(int byteOffset, long value, bool littleEndian = false);
        public extern void setBigUint64(int byteOffset, ulong value, bool littleEndian = false);
    }
}