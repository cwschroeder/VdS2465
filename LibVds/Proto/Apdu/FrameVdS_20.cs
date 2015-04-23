namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Status (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_20 : FrameVdS
    {
        public FrameVdS_20(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
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

        /// <summary>
        /// Die Adresserweiterung ist wie folgt aufgeteilt:
        /// 0x01: Meldeeingänge/Messwerte
        /// 0x02: Schaltuausgänge/Stellwerte
        /// 0x10: Störung
        /// </summary>
        public byte AddressExt
        {
            get { return this.buffer[5]; }
        }
    }
}