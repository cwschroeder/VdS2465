using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ConsoleTest
{
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;

    using LibVds.Proto;

    using LibVdsModbus;

    class Program
    {
        private static readonly Logger Log = LogManager.GetLogger("Application");

        private static PlcFrame storedFrame;
        private static IPEndPoint vdsEndpoint = new IPEndPoint(IPAddress.Parse("176.94.30.165"), 9000);
        private static string plcHostName = "zintl.selfhost.eu";

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            
            // setup connection to plc
            var hosts = Dns.GetHostAddresses(plcHostName);
            var plcEndpoint = new IPEndPoint(hosts[0], 502);

            // setup connection to central station
            var clientSession = VdsClient.Run(vdsEndpoint, cts.Token);

            while (!cts.IsCancellationRequested)
            {
                var readFrame = Driver.Read(plcEndpoint);
                if (readFrame == null)
                {
                    Log.Warn("Next retry of PLC readout starts in a few seconds...");
                    cts.Token.WaitHandle.WaitOne(1000);
                    continue;
                }

                if (storedFrame == null)
                {
                    storedFrame = readFrame;    
                }
                

                if (!clientSession.IsConnected)
                {
                    cts.Token.WaitHandle.WaitOne(3000);
                    clientSession = VdsClient.Run(vdsEndpoint, cts.Token);
                }

                CheckAndHandleChanges(readFrame, clientSession);

                cts.Token.WaitHandle.WaitOne(1000);
            }
            
            Console.ReadLine();
        }

        private static void CheckAndHandleChanges(PlcFrame readFrame, SessionVdS clientSession)
        {
            if (clientSession.TransmitQueueLength == 0)
            {
                // Always send deviceno message in advance
                clientSession.AddMessage(FrameVdS.CreateIdentificationNumberMessage());    
            }
            
            // Check and handle changes...
            if (storedFrame.Allg_Meldung != readFrame.Allg_Meldung)
            {
                var frame = FrameVdS.CreateChangeMessageCommon(readFrame.Allg_Meldung);
                clientSession.AddMessage(frame);
            }

            if (storedFrame.Stoerung_Batterie != readFrame.Stoerung_Batterie)
            {
                var frame = FrameVdS.CreateChangeMessageBatteryFault(readFrame.Stoerung_Batterie);
                clientSession.AddMessage(frame);
            }

            if (storedFrame.Erdschluss != readFrame.Erdschluss)
            {
                var frame = FrameVdS.CreateChangeMessageGroundFault(readFrame.Erdschluss);
                clientSession.AddMessage(frame);
            }

            if (storedFrame.Systemstoerung != readFrame.Systemstoerung)
            {
                var frame = FrameVdS.CreateChangeMessageSystemFault();
                clientSession.AddMessage(frame);
            }

            if (storedFrame.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB != readFrame.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB)
            {
                var frame = FrameVdS.CreateChangeMessageLS_SwitchedOff();
                clientSession.AddMessage(frame);
            }

            if (storedFrame.Firmenspez_Meldung_Stellung_LS != readFrame.Firmenspez_Meldung_Stellung_LS)
            {
                var frame = FrameVdS.CreateChangeMessageState_LS(readFrame.Firmenspez_Meldung_Stellung_LS);
                clientSession.AddMessage(frame);
            }

            // Check and handle values
            const double EPSILON = 0.05;
            if (Math.Abs(storedFrame.Spannung - readFrame.Spannung) > EPSILON)
            {
                var frame = FrameVdS.CreateMeasurementMessageVoltage(readFrame.Spannung);
                clientSession.AddMessage(frame);
            }

            if (Math.Abs(storedFrame.Strom - readFrame.Strom) > EPSILON)
            {
                var frame = FrameVdS.CreateMeasurementMessageCurrent(readFrame.Strom);
                clientSession.AddMessage(frame);
            }

            if (Math.Abs(storedFrame.Leistung - readFrame.Leistung) > EPSILON)
            {
                var frame = FrameVdS.CreateMeasurementMessagePower(readFrame.Leistung);
                clientSession.AddMessage(frame);
            }

            storedFrame = readFrame;
        }
    }
}
