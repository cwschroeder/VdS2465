using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibVds.Proto
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class Client
    {
        public static SessionVdS Run(IPEndPoint endPoint, CancellationToken token)
        {
            if (endPoint == null)
            {
                endPoint = new IPEndPoint(IPAddress.Loopback, 8888);
            }

            var client = new TcpClient();

            Console.WriteLine("Connecting to: " + endPoint);

            client.Connect(endPoint);
            if (!client.Connected)
            {
                Console.WriteLine("No TCP connection");
            }

            var stream = client.GetStream();
            var session = new SessionVdS(stream, false, 0);
            session.Run();
            return session;
        }
    }
}
