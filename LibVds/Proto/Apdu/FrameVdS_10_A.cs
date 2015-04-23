namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Abfrage (Muss-Bedingung)
    /// Dieser Satztyp dient zur Abfrage eines anderen Satztypen. Der gewünschte Satztyp ist im Feld Abfragetyp agegeben. 
    /// Die Gegenstation antwortet mit dem entsprechenden Satztyp.
    /// Es gibt eine erweiterte Variante für die abfrage einer Statusmeldung.
    /// </summary>
    public class FrameVdS_10_A : FrameVdS
    {
        public FrameVdS_10_A(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
        }

        public byte Device
        {
            get { return this.buffer[2]; }
        }

        public byte RequestType
        {
            get { return this.buffer[3]; }
        }
    }
}