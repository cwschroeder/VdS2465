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
        private static ModbusIpMaster master;

        public static void Read()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(new IPEndPoint(IPAddress.Parse("192.168.178.5"), 502));
            master = ModbusIpMaster.CreateIp(tcpClient);

            while (true)
            {
                var result = master.ReadInputRegisters(1, 15);
                var frame = new Frame(result);
                Console.WriteLine(frame.ToString());

                master.WriteSingleRegister(1, 20, 1);

                Thread.Sleep(3000);
            }


        }
    }
}