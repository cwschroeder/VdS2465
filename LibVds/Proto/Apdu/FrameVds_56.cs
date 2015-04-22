using System;

namespace LibVds.Proto.Apdu
{
    public class FrameVdS_56 : FrameVdS
    {
        public FrameVdS_56(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public byte Device
        {
            get { return this.buffer[2]; }
        }

        public byte Type_A
        {
            get { return this.buffer[3]; }
        }

        public byte Type_B
        {
            get { return this.buffer[4]; }
        }

        public byte Type_C
        {
            get { return this.buffer[5]; }
        }

        public byte Type_D
        {
            get { return this.buffer[6]; }
        }

        public byte Type_E
        {
            get { return this.buffer[7]; }
        }

        public byte Type_F
        {
            get { return this.buffer[8]; }
        }

        public byte Type_G
        {
            get { return this.buffer[3]; }
        }

        public static FrameVdS_56 Create(byte[] identBytes)
        {
            var buff = new byte[identBytes.Length + 2];
            buff[0] = (byte)identBytes.Length;
            buff[1] = (byte)VdSType.Identifikations_Nummer;
            Array.Copy(identBytes, 0, buff, 2, identBytes.Length);

            return new FrameVdS_56(buff, 0);
        }
    }
}