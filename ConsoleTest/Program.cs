using System.Collections.Concurrent;

namespace ConsoleTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using LibVds.Proto;

    using LibVdsModbus;

    using NLog;

    public class Program
    {
        private static readonly Logger Log = LogManager.GetLogger("Application");

        private static PlcFrame storedPlcFrame;
        private static IPEndPoint vdsEndpoint = new IPEndPoint(IPAddress.Parse("176.94.30.165"), 9000);
        private static string plcHostName = "zintl.selfhost.eu";

        private static SessionVdS clientSession;
        private static ConcurrentQueue<FrameVdS> vdsFrames = new ConcurrentQueue<FrameVdS>(); 

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            while (!cts.IsCancellationRequested)
            {
                var readFrame = Driver.Read(plcHostName);
                if (readFrame == null)
                {
                    Log.Warn("Next retry of PLC readout starts in a few seconds...");
                    cts.Token.WaitHandle.WaitOne(1000);
                    continue;
                }

                if (storedPlcFrame == null)
                {
                    storedPlcFrame = readFrame;
                }

                CreateVdsFrames(readFrame, cts.Token);


                // vds session mgmgt
                while (vdsFrames.Any())
                {
                    if (clientSession == null)
                    {
                        // setup connection to central station
                        clientSession = VdsClient.Run(vdsEndpoint, cts.Token);
                    }

                    FrameVdS vdsFrame;
                    if (vdsFrames.TryDequeue(out vdsFrame))
                    {
                        clientSession.AddMessage(vdsFrame);
                    }

                    // wait for acknowledge
                    if (!clientSession.WaitForAcknowledge(cts.Token, TimeSpan.FromSeconds(30)))
                    {
                        Log.Error("No ACK received - terminating session...");
                        break;
                    }                    
                }

                if (clientSession != null)
                {
                    clientSession.Close();
                    clientSession = null;
                }
                
                cts.Token.WaitHandle.WaitOne(1000);
            }

            Console.ReadLine();
        }

        private static void CreateVdsFrames(PlcFrame readFrame, CancellationToken token)
        {
            // Check and handle changes...
            if (storedPlcFrame.Allg_Meldung != readFrame.Allg_Meldung)
            {
                vdsFrames.Enqueue(FrameVdS.CreateChangeMessageCommon(readFrame.Allg_Meldung));
            }

            if (storedPlcFrame.Stoerung_Batterie != readFrame.Stoerung_Batterie)
            {
                vdsFrames.Enqueue(FrameVdS.CreateChangeMessageBatteryFault(readFrame.Stoerung_Batterie));
            }

            if (storedPlcFrame.Erdschluss != readFrame.Erdschluss)
            {
                vdsFrames.Enqueue(FrameVdS.CreateChangeMessageGroundFault(readFrame.Erdschluss));
            }

            if (storedPlcFrame.Systemstoerung != readFrame.Systemstoerung)
            {
                vdsFrames.Enqueue(FrameVdS.CreateChangeMessageSystemFault());
            }

            if (storedPlcFrame.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB != readFrame.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB)
            {
                vdsFrames.Enqueue(FrameVdS.CreateChangeMessageLS_SwitchedOff());
            }

            if (storedPlcFrame.Firmenspez_Meldung_Stellung_LS != readFrame.Firmenspez_Meldung_Stellung_LS)
            {
                vdsFrames.Enqueue(FrameVdS.CreateChangeMessageState_LS(readFrame.Firmenspez_Meldung_Stellung_LS));
            }

            // Check and handle values
            const double EPSILON = 0.05;
            if (Math.Abs(storedPlcFrame.Spannung - readFrame.Spannung) > EPSILON)
            {
                vdsFrames.Enqueue(FrameVdS.CreateMeasurementMessageVoltage(readFrame.Spannung));
            }

            if (Math.Abs(storedPlcFrame.Strom - readFrame.Strom) > EPSILON)
            {
                vdsFrames.Enqueue(FrameVdS.CreateMeasurementMessageCurrent(readFrame.Strom));
            }

            if (Math.Abs(storedPlcFrame.Leistung - readFrame.Leistung) > EPSILON)
            {
                vdsFrames.Enqueue(FrameVdS.CreateMeasurementMessagePower(readFrame.Leistung));
            }

            storedPlcFrame = readFrame;
        }
    }
}
