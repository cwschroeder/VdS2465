using System;
using System.Security.Cryptography;
using System.Text;

namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Identifikations-Nummer (Muss-Bedingung).
    /// Dieser Satz dient zur eindeutigen Identifizierung der Informationsquelle.
    /// </summary>
    public class FrameVdS_56 : FrameVdS
    {
        public FrameVdS_56(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public long IdentNo
        {
            get
            {
                var sb = new StringBuilder();
                for (int i = 2; i < this.buffer.Length; i++)
                {
                    sb.Append(this.buffer[i] & 0x0F);

                    var highNibble = this.buffer[i] >> 4;
                    if (highNibble == 0x0F)
                    {
                        break;
                    }

                    sb.Append(highNibble);
                }

                return long.Parse(sb.ToString());
            }
        }

        public static FrameVdS_56 Create(long identNo)
        {
            var chars = identNo.ToString().ToCharArray();
            var buffLen = chars.Length;
            if (buffLen % 2 != 0)
            {
                buffLen++;
            }
            buffLen = 2 + buffLen/2;

            var buff = new byte[buffLen];
            buff[0] = (byte)(buff.Length - 2);
            buff[1] = (byte)VdSType.Identifikations_Nummer;
            for (int i = 0, k = 2; i < chars.Length; i++)
            {
                byte low = (byte)(chars[i] - 0x30);
                byte high = 0xF0;
                if (++i < chars.Length)
                {
                    high = (byte)((chars[i] - 0x30) << 4);
                }

                buff[k++] = (byte)(high | low);
            }


            return new FrameVdS_56(buff, 0);
        }

        public static FrameVdS Create(byte[] identNo)
        {
            var buff = new byte[identNo.Length + 2];
            buff[0] = (byte)(buff.Length - 2);
            buff[1] = (byte)VdSType.Identifikations_Nummer;
            Array.Copy(identNo, 0, buff, 2, identNo.Length);

            return new FrameVdS_56(buff, 0);
        }
    }
}