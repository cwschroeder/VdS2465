namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Abfrage (Muss-Bedingung)
    /// </summary>
    public class FrameVdS_10_B : FrameVdS
    {
        public FrameVdS_10_B(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
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

        /// <summary>
        /// Für die Abfrage einer Statusmeldung 0x20 und einer Blockstatusmeldung 0x24 ist die Adresserweiterung wie folgt aufgeteilt:
        /// 0x01: Meldeeingänge/Messwerte
        /// 0x02: Schaltuausgänge/Stellwerte
        /// 0x10: Störung
        /// </summary>
        public byte AddressExt
        {
            get { return this.buffer[5]; }
        }

        /// <summary>
        /// Für die Abfrage der Test-Timereinstellung 0x41 ist die Adresserweiterung wie folgt aufgeteilt:
        /// 0x50: Zeit bis zur nächsten Testmeldung
        /// 0x51: Testmeldungsintervall
        /// </summary>
        public byte RequestType
        {
            get { return this.buffer[6]; }
        }
    }
}