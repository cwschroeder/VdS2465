using LibVds.Proto;
using LibVds.Proto.Apdu;

namespace LibVdsModbus
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using Modbus.Device;
    using NLog;

    public static class PlcController
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static IPEndPoint targetEndpoint;
        private static TcpClient tcpClient;
        private static ModbusIpMaster master;

        public static PlcFrame Read(string plcHostName)
        {
            // setup connection to plc
            if (!EnsureConnected(plcHostName))
            {
                return null;
            }

            try
            {
                var result = master.ReadInputRegisters(0, 15);
                var frame = new PlcFrame(result);
                Log.Debug(frame.ToString);

                return frame;
            }
            catch (Exception ex)
            {
                Log.ErrorException("Read PLC failed", ex);
                
                return null;
            }
        }

        private static bool EnsureConnected(string hostname)
        {
            if (tcpClient == null)
            {
                tcpClient = new TcpClient();
            }

            try
            {
                var hosts = Dns.GetHostAddresses(hostname);
                if (hosts.Length == 0)
                {
                    return false;
                }

                var endPoint = new IPEndPoint(hosts[0], 502);
                if (!endPoint.Equals(targetEndpoint))
                {
                    targetEndpoint = endPoint;
                    if (tcpClient.Connected)
                    {
                        tcpClient.Close();
                    }
                }

                if (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.Close();
                    }
                    catch (Exception)
                    {
                    }
                    
                    tcpClient = new TcpClient();

                    tcpClient.Connect(targetEndpoint);
                    master = ModbusIpMaster.CreateIp(tcpClient);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.ErrorException("Connect failed", ex);
                return false;
            }
        }

        public static FrameVdS CreateChangeMessageCommon(bool isOn)
        {
            Log.Debug("Create change message COMMON with value {0}", isOn);
            return FrameVdS_02.Create(0x01, 0x00, 0x00, 0x00, isOn ? (byte)0x00 : (byte)0x80);
        }

        public static FrameVdS CreateChangeMessageBatteryFault(bool hasOccured)
        {
            Log.Debug("Create change message BATTERY FAULT with value {0}", hasOccured);
            return FrameVdS_02.Create(0x01, 0x00, 0x00, 0x00, hasOccured ? (byte)0x33 : (byte)0xB3);
        }

        public static FrameVdS CreateChangeMessageGroundFault(bool hasOccured)
        {
            Log.Debug("Create change message GROUND FAULT with value {0}", hasOccured);
            return FrameVdS_02.Create(0x01, 0x00, 0x00, 0x00, hasOccured ? (byte)0x35 : (byte)0xB5);
        }

        public static FrameVdS CreateChangeMessageSystemFault()
        {
            Log.Debug("Create change message SYSTEM FAULT");
            return FrameVdS_02.Create(0x01, 0x00, 0x00, 0x00, (byte)0x55);
        }

        public static FrameVdS CreateChangeMessageLS_SwitchedOff()
        {
            // Command initiated by NB
            Log.Debug("Create change message LS SWITCHED OFF");
            return FrameVdS_02.Create(0x01, 0x00, 0x00, 0x00, (byte)0x71);
        }

        public static FrameVdS CreateChangeMessageState_LS(bool isOn)
        {
            // Command initiated by NB
            Log.Debug("Create change message LS STATE with value {0}", isOn);
            return FrameVdS_02.Create(0x01, 0x00, 0x00, 0x00, isOn ? (byte)0x72 : (byte)0xF2);
        }

        public static FrameVdS CreateMeasurementMessageVoltage(double voltage)
        {
            Log.Debug("Create change message VOLTAGE with value {0}", voltage);
            //return FrameVdS_30.Create(0x01, 0x00, 0x00, 0x00, 0x21, voltage);
            return FrameVdS_54.Create(string.Format("Spannung geändert, neuer Wert: {0} V"));
        }

        public static FrameVdS CreateMeasurementMessageCurrent(double current)
        {
            Log.Debug("Create change message CURRENT with value {0}", current);
            //return FrameVdS_30.Create(0x01, 0x00, 0x00, 0x00, 0x31, current);
            return FrameVdS_54.Create(string.Format("Strom geändert, neuer Wert: {0} A"));
        }

        public static FrameVdS CreateMeasurementMessagePower(double power)
        {
            Log.Debug("Create change message POWER with value {0}", power);
            //return FrameVdS_30.Create(0x01, 0x00, 0x00, 0x00, 0x48, power);
            return FrameVdS_54.Create(string.Format("Leistung geändert, neuer Wert: {0} kW"));
        }
    }
}