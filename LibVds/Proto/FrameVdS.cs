namespace LibVds.Proto
{
    using System;
    using System.Linq;

    public class FrameVdS
    {
        private readonly byte[] buffer;

        public FrameVdS(byte[] bytes, int start, InformationId informationId)
        {
            this.InformationId = informationId;
            if (this.InformationId == InformationId.Payload)
            {
                var vdsLength = bytes[start];
                this.buffer = new byte[vdsLength + 1];
                Array.Copy(bytes, start, this.buffer, 0, this.buffer.Length);
            }
            else if (informationId == InformationId.SyncReq || informationId == InformationId.SyncRes)
            {
                this.buffer = new byte[1];
                this.buffer[0] = bytes[start];
            }
            else
            {
                this.buffer = new byte[0];
            }
        }

        public InformationId InformationId { get; set; }

        public byte VdsLength
        {
            get
            {
                if (this.InformationId != InformationId.Payload)
                {
                    throw new InvalidOperationException("VdsLength is only defined for payload frames");
                }

                return (byte)this.buffer.Length;
            }
        }

        public VdSType VdsType
        {
            get
            {
                if (this.InformationId != InformationId.Payload)
                {
                    throw new InvalidOperationException("VdsType is only defined for payload frames");
                }

                return (VdSType)this.buffer[1];
            }

            private set
            {
                if (this.InformationId != InformationId.Payload)
                {
                    throw new InvalidOperationException("VdSType is only defined for payload frames");
                }

                this.buffer[1] = (byte)value;
            }
        }

        public byte[] Serialize()
        {
            return this.buffer.ToArray();
        }

        public int GetByteCount()
        {
            return this.buffer.Length;
        }

        public static FrameVdS CreateSyncRequestResponse(InformationId informationId)
        {
            if (informationId != InformationId.SyncReq && informationId != InformationId.SyncRes)
            {
                throw new ArgumentException("informationId");
            }

            // Window size
            var buff = new byte[] { 0x01 };
            return new FrameVdS(buff, 0, informationId);
        }

        public static FrameVdS CreateEmpty(InformationId informationId)
        {
            if (informationId != Proto.InformationId.PollReqRes)
            {
                throw new ArgumentException("informationId");
            }

            var buff = new byte[0];
            return new FrameVdS(buff, 0, informationId);
        }
    }
}