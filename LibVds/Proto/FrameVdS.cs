using LibVds.Proto.Apdu;
using NLog;
using NLog.Fluent;

namespace LibVds.Proto
{
    using System;
    using System.Linq;

    public class FrameVdS
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected readonly byte[] buffer;

        public FrameVdS(byte[] bytes, int start, InformationId informationId = InformationId.Payload)
        {
            this.InformationId = informationId;
            if (this.InformationId == InformationId.Payload)
            {
                var vdsLength = bytes[start];
                this.buffer = new byte[vdsLength + 1 + 1];
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

                return (byte)this.buffer[0];
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
            if (informationId != InformationId.PollReqRes)
            {
                throw new ArgumentException("informationId");
            }

            var buff = new byte[0];
            return new FrameVdS(buff, 0, informationId);
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
        public static FrameVdS_02 CreatePayloadType02(byte device, byte address, byte addressAdd, byte addressExt, byte messageType)
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

        /// <summary>
        /// Mess-/Zähl- und Stellwerte
        /// </summary>
        public static FrameVdS CreatePayloadType30(byte device, byte address, byte addressAdd, byte addressExt, byte valueKind, double value)
        {
            var half = new Half(value);
            var halfBuff = BitConverter.GetBytes(half);
            var buff = new byte[]
                           {
                               0x00, 
                               (byte)VdSType.Mess_Zaehl_Stellwerte, 
                               device,
                               address, 
                               addressAdd, 
                               addressExt, 
                               valueKind,
                               halfBuff[3],
                               halfBuff[2]
                           };
            buff[0] = (byte)(buff.Length - 2);
            return new FrameVdS(buff, 0, InformationId.Payload);
        }

        public static FrameVdS CreatePayloadType56(byte[] identBytes)
        {
            var buff = new byte[identBytes.Length + 2];
            buff[0] = (byte)identBytes.Length;
            buff[1] = (byte)VdSType.Identifikations_Nummer;
            Array.Copy(identBytes, 0, buff, 2, identBytes.Length);

            return new FrameVdS(buff, 0, InformationId.Payload);
        }

        public static FrameVdS CreateChangeMessageCommon(bool isOn)
        {
            Log.Debug("Create change message COMMON with value {0}", isOn);
            return CreatePayloadType02(0x01, 0x00, 0x00, 0x00, isOn ? (byte)0x00 : (byte)0x80);    
        }

        public static FrameVdS CreateChangeMessageBatteryFault(bool hasOccured)
        {
            Log.Debug("Create change message BATTERY FAULT with value {0}", hasOccured);
            return CreatePayloadType02(0x01, 0x00, 0x00, 0x00, hasOccured ? (byte)0x33 : (byte)0xB3);
        }

        public static FrameVdS CreateChangeMessageGroundFault(bool hasOccured)
        {
            Log.Debug("Create change message GROUND FAULT with value {0}", hasOccured);
            return CreatePayloadType02(0x01, 0x00, 0x00, 0x00, hasOccured ? (byte)0x35 : (byte)0xB5);
        }

        public static FrameVdS CreateChangeMessageSystemFault()
        {
            Log.Debug("Create change message SYSTEM FAULT");
            return CreatePayloadType02(0x01, 0x00, 0x00, 0x00, (byte)0x55);
        }

        public static FrameVdS CreateChangeMessageLS_SwitchedOff()
        {
            // Command initiated by NB
            Log.Debug("Create change message LS SWITCHED OFF");
            return CreatePayloadType02(0x01, 0x00, 0x00, 0x00, (byte)0x71);
        }

        public static FrameVdS CreateChangeMessageState_LS(bool isOn)
        {
            // Command initiated by NB
            Log.Debug("Create change message LS STATE with value {0}", isOn);
            return CreatePayloadType02(0x01, 0x00, 0x00, 0x00, isOn ? (byte)0x72 : (byte)0xF2);
        }

        public static FrameVdS CreateMeasurementMessageVoltage(double voltage)
        {
            Log.Debug("Create change message VOLTAGE with value {0}", voltage);
            return CreatePayloadType30(0x01,0x00, 0x00, 0x00, 0x21, voltage);
        }

        public static FrameVdS CreateMeasurementMessageCurrent(double current)
        {
            Log.Debug("Create change message CURRENT with value {0}", current);
            return CreatePayloadType30(0x01, 0x00, 0x00, 0x00, 0x31, current);
        }

        public static FrameVdS CreateMeasurementMessagePower(double power)
        {
            Log.Debug("Create change message POWER with value {0}", power);
            return CreatePayloadType30(0x01, 0x00, 0x00, 0x00, 0x48, power);
        }

        public static FrameVdS CreateIdentificationNumberMessage()
        {
            return FrameVdS.CreatePayloadType56(new byte[] {0x99, 0x99, 0x99});
        }
    }
}