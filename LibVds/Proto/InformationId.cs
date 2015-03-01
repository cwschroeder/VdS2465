namespace LibVds.Proto
{
    public enum InformationId : byte
    {
        SyncReq = 0x01,
        SyncRes = 0x02,
        PollReqRes = 0x03,
        Payload = 0x04,
        ErrorInformationIdUnknown = 0x05,
        ErrorProtocolIdUnknown = 0x06
    }
}