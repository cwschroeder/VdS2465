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

        private static PlcFrame storedFrame;
        private static IPEndPoint vdsEndpoint = new IPEndPoint(IPAddress.Parse("176.94.30.165"), 9000);
        private static string plcHostName = "zintl.selfhost.eu";

        private static SessionVdS clientSession;

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

                if (storedFrame == null)
                {
                    storedFrame = readFrame;
                }

                CheckAndHandleChanges(readFrame, cts.Token);

                cts.Token.WaitHandle.WaitOne(8000);
            }

            Console.ReadLine();
        }

        private static void CheckAndHandleChanges(PlcFrame readFrame, CancellationToken token)
        {
            var frames = new List<FrameVdS>();

            // Check and handle changes...
            if (storedFrame.Allg_Meldung != readFrame.Allg_Meldung)
            {
                frames.Add(FrameVdS.CreateChangeMessageCommon(readFrame.Allg_Meldung));
            }

            if (storedFrame.Stoerung_Batterie != readFrame.Stoerung_Batterie)
            {
                frames.Add(FrameVdS.CreateChangeMessageBatteryFault(readFrame.Stoerung_Batterie));
            }

            if (storedFrame.Erdschluss != readFrame.Erdschluss)
            {
                frames.Add(FrameVdS.CreateChangeMessageGroundFault(readFrame.Erdschluss));
            }

            if (storedFrame.Systemstoerung != readFrame.Systemstoerung)
            {
                frames.Add(FrameVdS.CreateChangeMessageSystemFault());
            }

            if (storedFrame.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB != readFrame.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB)
            {
                frames.Add(FrameVdS.CreateChangeMessageLS_SwitchedOff());
            }

            if (storedFrame.Firmenspez_Meldung_Stellung_LS != readFrame.Firmenspez_Meldung_Stellung_LS)
            {
                frames.Add(FrameVdS.CreateChangeMessageState_LS(readFrame.Firmenspez_Meldung_Stellung_LS));
            }

            // Check and handle values
            const double EPSILON = 0.05;
            if (Math.Abs(storedFrame.Spannung - readFrame.Spannung) > EPSILON)
            {
                frames.Add(FrameVdS.CreateMeasurementMessageVoltage(readFrame.Spannung));
            }

            if (Math.Abs(storedFrame.Strom - readFrame.Strom) > EPSILON)
            {
                frames.Add(FrameVdS.CreateMeasurementMessageCurrent(readFrame.Strom));
            }

            if (Math.Abs(storedFrame.Leistung - readFrame.Leistung) > EPSILON)
            {
                frames.Add(FrameVdS.CreateMeasurementMessagePower(readFrame.Leistung));
            }

            storedFrame = readFrame;

            if (!frames.Any())
            {
                return;
            }

            if (clientSession == null)
            {
                // setup connection to central station
                clientSession = VdsClient.Run(vdsEndpoint, token);                
            }

            foreach (var frame in frames)
            {
                clientSession.AddMessage(frame);
            }

            // wait for acknowledge
            while (!token.IsCancellationRequested && !clientSession.IsAcked)
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
            }

            clientSession.Close();
        }
    }
}
