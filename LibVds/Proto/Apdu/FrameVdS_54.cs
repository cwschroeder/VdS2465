using System;
using System.Text;

namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// ASCII-Zeichenfolge
    /// Mit diesem Satztyp wird eine beliebige Zeichenfolge in erweiterter ASCII-Codierung übertragen.
    /// </summary>
    public class FrameVdS_54 : FrameVdS
    {
        public FrameVdS_54(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public string Text
        {
            get { return Encoding.ASCII.GetString(this.buffer, 2, this.buffer.Length - 2); }
        }

        public static FrameVdS_54 Create(string text)
        {
            var buff = new byte[text.Length + 2];
            buff[0] = (byte)(buff.Length - 2);
            buff[1] = (byte)VdSType.Ascii_Zeichenfolge;
            var bytes = Encoding.ASCII.GetBytes(text);
            Array.Copy(bytes, 0, buff, 2, bytes.Length);

            return new FrameVdS_54(buff, 0);
        }
    }
}