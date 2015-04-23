namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Verbindung wird nicht mehr benötigt (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_FF : FrameVdS
    {
        public FrameVdS_FF(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
        {
        }

        public static FrameVdS_FF Create()
        {
            var buff = new byte[2];
            buff[0] = (byte)(buff.Length - 2);
            buff[1] = (byte)VdSType.Verbindung_wird_nicht_mehr_benoetigt;

            return new FrameVdS_FF(buff, 0);
        }
    }
}