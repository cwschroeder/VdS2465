namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Fehler (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_11 : FrameVdS
    {
        public FrameVdS_11(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
        {
        }

        public byte Device
        {
            get { return this.buffer[2]; }
        }

        public byte ErrorCode
        {
            get { return this.buffer[3]; }
        }
    }
}