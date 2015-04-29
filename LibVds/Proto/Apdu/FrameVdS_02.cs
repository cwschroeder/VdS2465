namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Meldung Zustandsänderung, Steuerung mit Quittungsanforderung
    /// </summary>
    /// <remarks>
    /// Mit diesem Satztyp werden Zustandsänderungen und Steuerbefehle übertragen, die von der Empfängerseite eine Telegrammquittung (Typ 0x03) anfordern.
    /// </remarks>
    public class FrameVdS_02 : FrameVdS
    {
        public FrameVdS_02(byte[] bytes, int start, InformationId informationId = InformationId.Payload) : base(bytes, start, informationId)
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

        /// <summary>
        /// Zustandsänderungen und Steuerbefehle die vom Empfänger eine Telegrammquittung Typ 0x03 anfordern.
        /// </summary>
        /// <param name="device">ÜG hat Gerätenummer 0, erste angeschlossenen Zentrale hat Gerätenummer 1.</param>
        /// <param name="address">0x00 bedeutet gesamte Zentrale.</param>
        /// <param name="addressAdd">0x00 bedeutet Zustandsänderung bezieht sich auf gesamte Adresse.</param>
        /// <param name="addressExt">0x01: Meldeingänge/Meßwerte.
        ///                          0x02: Schaltausgänge/Stellwerte
        ///                          Das höherwertige Half-Byte kann als Adress-Offset benutzt werden.</param>
        /// <param name="messageType">Bit 7: Ein/Aus, 1->Ruhe, Aus, zurückgenommen, unscharf
        ///                                           0->Alarm, Ein, ausgelöst, scharf
        ///                           Bit6-4: Meldungsblock
        ///                                   0x01: Allgemein: Meldung/Schaltzustand/Steuern
        ///                                   0x02: Überfall/Einbruch
        ///                                   0x03: Störungsmeldung
        ///                                   0x04: Sonstige Meldung
        ///                                   0x05: Gerätemeldungen
        ///                                   0x06: Zustandsmeldungen
        ///                                   0x07: Firmenspezifische Meldungen
        ///                                   --> weitere Details: S.31
        ///                           Bit3-0: Meldungskennung</param>
        /// <returns>A VDS frame</returns>
        public static FrameVdS_02 Create(byte device, byte address, byte addressAdd, byte addressExt, byte messageType)
        {
            var buff = new byte[]
                           {
                               0x00, 
                               (byte)VdSType.Meldung_Zustandsaenderung__Steuerung_mit_Quittungsanforderung, 
                               device,
                               address, 
                               addressAdd, 
                               addressExt, 
                               messageType
                           };
            buff[0] = (byte)(buff.Length - 2);
            return new FrameVdS_02(buff, 0, InformationId.Payload);
        }
    }
}