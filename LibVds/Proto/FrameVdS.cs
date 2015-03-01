namespace LibVds.Proto
{
    using System;
    using System.Linq;

    public class FrameVdS
    {
        private readonly byte[] buffer;

        public FrameVdS(byte[] bytes, int start, int length)
        {
            this.buffer = new byte[length];
            Array.Copy(bytes, start, this.buffer, 0, length);

        }

        public byte Length
        {
            get
            {
                return this.buffer[0];
            }
        }

        public VdSType Type
        {
            get
            {
                return (VdSType)this.buffer[1];
            }

            private set
            {
                this.buffer[1] = (byte)value;
            }
        }

        public byte[] Serialize()
        {
            return this.buffer.ToArray();
        }

        public int GetByteCount()
        {
            // add Length and Type
            return this.Length + 2;
        }
    }
}