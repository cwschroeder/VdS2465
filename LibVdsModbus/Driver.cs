using System.Threading.Tasks;
using NLog;

namespace LibVdsModbus
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using Modbus.Device;

    public static class Driver
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
    }
}