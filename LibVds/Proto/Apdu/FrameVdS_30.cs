using System;
using LibVds.Utils;

namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Mess-/Zähl- und Stellwerte
    /// </summary>
    public class FrameVdS_30 : FrameVdS
    {
        public FrameVdS_30(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public byte Address
        {
            get { return this.buffer[3]; }
        }

        public byte AddressAdd
        {
            get { return this.buffer[4]; }
        }

        public byte AddressExt
        {
            get { return this.buffer[5]; }
        }

        public byte Value_Low
        {
            get
            {
                return this.buffer[6];
            }
        }

        public byte Value_High
        {
            get
            {
                return this.buffer[7];
            }
        }

        public static FrameVdS_30 Create(byte device, byte address, byte addressAdd, byte addressExt, byte valueKind, double value)
        {
            var half = new Half(value);
            var halfBuff = BitConverter.GetBytes(half);
            var buff = new byte[]
                           {
                               0x00, 
                               (byte)VdSType.Mess_Zaehl_Stellwerte, 
                               device,
                               address, 
                               addressAdd, 
                               addressExt, 
                               valueKind,
                               halfBuff[3],
                               halfBuff[2]
                           };
            buff[0] = (byte)(buff.Length - 2);
            return new FrameVdS_30(buff, 0);
        }
    }
}