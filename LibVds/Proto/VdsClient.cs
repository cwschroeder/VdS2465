using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace LibVds.Proto
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class VdsClient
    {
        private static readonly Logger Log = LogManager.GetLogger("VdsClient");

        public static SessionVdS Run(IPEndPoint endPoint, CancellationToken token)
        {
            if (endPoint == null)
            {
                Log.Warn("No endpoint given, using default...");
                endPoint = new IPEndPoint(IPAddress.Loopback, 8888);
            }

            var client = new TcpClient();

            try
            {
                Log.Info("Connecting to: " + endPoint);
                client.Connect(endPoint);   
            }
            catch (Exception ex)
            {
                Log.ErrorException(string.Format("Connecting to:  {0} failed", endPoint), ex);
                return null;
            }
            
            var stream = client.GetStream();
            //var session = new SessionVdS(stream, isServer: false, keyNumber: 3);
            var session = new SessionVdS(stream, isServer: false, keyNumber: 0);
            session.Run();
            return session;
        }
    }
}
