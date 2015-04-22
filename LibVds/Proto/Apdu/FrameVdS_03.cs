using LibVds.Utils;

namespace LibVds.Proto.Apdu
{
    /// <summary>
    /// Quittungsrücksendung
    /// </summary>
    /// <remarks>
    /// Mit diesem Satztyp werden Zustandsänderungen und Steuerbefehle quittiert, wenn eine entsprechende Quittung angeforder worden ist (Typ 0x02).
    /// Der Satz unterscheidet sich nur im Satztyp vom Meldungs- oder Steuersatz.
    /// </remarks>
    public class FrameVdS_03 : FrameVdS
    {
        public FrameVdS_03(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
            : base(bytes, start, informationId)
        {
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
    }
}