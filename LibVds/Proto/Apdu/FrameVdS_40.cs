namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Testmeldung (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_40 : FrameVdS
    {
        public FrameVdS_40(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
        {
        }

        public static FrameVdS_40 Create()
        {
            var buff = new byte[]
            {
                0x00, 
                (byte)VdSType.Testmeldung
            };
            buff[0] = (byte)(buff.Length - 2);
            return new FrameVdS_40(buff, 0);
        }
    }
}