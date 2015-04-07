namespace LibVds.Proto
{
    public class FrameVdS_02 : FrameVdS
    {
        public FrameVdS_02(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
        {
        }

        public byte Address { get; set; }
        public byte AddressAdd { get; set; }
        public byte AddressExt { get; set; }
    }
}