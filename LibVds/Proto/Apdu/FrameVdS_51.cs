using System;
using System.Text;

namespace LibVds.Proto.Apdu
{
    public class FrameVdS_51 : FrameVdS
    {
        public FrameVdS_51(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public string HerstellerId
        {
            get { return Encoding.ASCII.GetString(this.buffer, 2, this.buffer.Length - 2); }
        }

        public static FrameVdS_51 Create(string herstellerId)
        {
            var data = Encoding.ASCII.GetBytes(herstellerId);
            var buff = new byte[data.Length + 2];
            buff[0] = (byte)data.Length;
            buff[1] = (byte)VdSType.HerstellerId;
            Array.Copy(data, 0, buff, 2, data.Length);

            return new FrameVdS_51(buff, 0);
        }
    }
}