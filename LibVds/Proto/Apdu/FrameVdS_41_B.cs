namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Test-Timereinstellung (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_41_B : FrameVdS
    {
        public FrameVdS_41_B(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        /// <summary>
        /// Die Adresserweiterung ist wie folgt aufgeteilt:
        /// 0x50: Zeit der nächsten Testmledung setzen bzw. abfragen
        /// 0x51: Testmeldungsintervall setzen bzw. abfragen
        /// </summary>
        public byte AddressExt
        {
            get { return this.buffer[3]; }
        }

        public byte ValueLow
        {
            get { return this.buffer[4]; }
        }

        public byte ValueHigh
        {
            get { return this.buffer[5]; }
        }
    }
}