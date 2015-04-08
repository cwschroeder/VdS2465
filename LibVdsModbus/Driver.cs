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

        public static PlcFrame Read(IPEndPoint endPoint)
        {
            EnsureConnected(endPoint);

            var result = master.ReadInputRegisters(0, 15);
            var frame = new PlcFrame(result);
            Log.Debug(frame.ToString);
            
            return frame;
        }

        private static bool EnsureConnected(IPEndPoint endPoint)
        {
            if (tcpClient == null)
            {
                tcpClient = new TcpClient();
            }

            try
            {
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