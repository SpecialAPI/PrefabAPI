using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PrefabAPI
{
    public enum EndianType
    {
        LittleEndian,
        BigEndian
    }

    public class EndianBinaryWriter : BinaryWriter
    {
        public EndianType endian;

        public EndianBinaryWriter(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
        {
            this.endian = endian;
        }

        public long Position
        {
            get => BaseStream.Position;
        }

        public void SetPosition(long val)
        {
            BaseStream.Position = val;
        }

        public override void Write(short val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 2);
                return;
            }
            base.Write(val);
        }

        public override void Write(int val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 4);
                return;
            }
            base.Write(val);
        }

        public override void Write(long val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 8);
                return;
            }
            base.Write(val);
        }

        public override void Write(ushort val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 2);
                return;
            }
            base.Write(val);
        }

        public override void Write(uint val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 4);
                return;
            }
            base.Write(val);
        }

        public override void Write(ulong val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 8);
                return;
            }
            base.Write(val);
        }

        public override void Write(float val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 4);
                return;
            }
            base.Write(val);
        }

        public override void Write(double val)
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                base.Write(buff, 0, 8);
                return;
            }
            base.Write(val);
        }
    }
}
