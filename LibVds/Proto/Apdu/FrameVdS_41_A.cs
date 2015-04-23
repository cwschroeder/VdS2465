namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Testmeldungsquittung (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_41_A : FrameVdS
    {
        public FrameVdS_41_A(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
        {
        }
    }
}