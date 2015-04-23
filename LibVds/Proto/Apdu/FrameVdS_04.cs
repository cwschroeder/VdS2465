namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Meldung Zuständsänderung, Steuerung ohne Quittungsanforderung
    /// Mit diesem Satztyp werden Zustandsänderungen und Steuerbefehle übertragen, die keine Quittung erfordern.
    /// </summary>
    public class FrameVdS_04 : FrameVdS
    {
        public FrameVdS_04(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public byte Device
        {
            get { return this.buffer[2]; }
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

        public byte MessageType
        {
            get { return this.buffer[6]; }
        }

        public static FrameVdS_04 Create(byte device, byte address, byte addressAdd, byte addressExt, byte messageType)
        {
            var buff = new byte[]
                           {
                               0x00, 
                               (byte)VdSType.Meldung_Zustandsaenderung__Steuerung_ohne_Quittungsanforderung, 
                               device,
                               address, 
                               addressAdd, 
                               addressExt, 
                               messageType
                           };
            buff[0] = (byte)(buff.Length - 2);
            return new FrameVdS_04(buff, 0, InformationId.Payload);
        }
    }
}