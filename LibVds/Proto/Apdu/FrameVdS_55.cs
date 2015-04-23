namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Aktuell unterstützte Satztypen (Muss-Bedingung).
    /// Mit diesem Satztyp werden auf Anfrage die vom Gerät aktuell unterstützten Satztypen des gesamten implementierten Vorrats übertragen. 
    /// </summary>
    /// TODO: check usage and meaning
    public class FrameVdS_55 : FrameVdS
    {
        public FrameVdS_55(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public byte Device
        {
            get { return this.buffer[2]; }
        }

        public byte Type_A
        {
            get { return this.buffer[3]; }
        }

        public byte Type_B
        {
            get { return this.buffer[4]; }
        }

        public byte Type_C
        {
            get { return this.buffer[5]; }
        }

        public byte Type_D
        {
            get { return this.buffer[6]; }
        }

        public byte Type_E
        {
            get { return this.buffer[7]; }
        }

        public byte Type_F
        {
            get { return this.buffer[8]; }
        }

        public byte Type_G
        {
            get { return this.buffer[9]; }
        }

        public static FrameVdS_55 Create(byte device, byte typeA, byte typeB, byte typeC, byte typeD, byte typeE, byte typeF, byte typeG)
        {
            var buff = new byte[10];
            buff[0] = (byte)(buff.Length - 2);
            buff[1] = (byte)VdSType.Aktuell_unterstuetzte_Satztypen;
            buff[2] = device;
            buff[3] = typeA;
            buff[4] = typeB;
            buff[5] = typeC;
            buff[6] = typeD;
            buff[7] = typeE;
            buff[8] = typeF;
            buff[9] = typeG;

            return new FrameVdS_55(buff, 0);
        }
    }
}