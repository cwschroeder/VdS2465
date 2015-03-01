namespace LibVds.Proto
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class Server
    {
        public static void Run(IPEndPoint endPoint, CancellationToken token)
        {
            if (endPoint == null)
            {
                endPoint = new IPEndPoint(IPAddress.Any, 8888);
            }

            Task.Run(
                () =>
                    {
                        var listener = new TcpListener(endPoint);
                        listener.Start();

                        Console.WriteLine("Listening at: " + endPoint);
                        while (!token.IsCancellationRequested)
                        {
                            var client = listener.AcceptTcpClient();
                            if (!client.Connected)
                            {
                                Console.WriteLine("Error accepting TCP socket");
                                continue;
                            }

                            var stream = client.GetStream();
                            var session = new SessionVdS(stream, true, 0);
                            session.Run();

                            Thread.Sleep(100);

                            // create sync request
                            session.SendRequest(InformationId.SyncReq);
                            
                            // create poll requests
                            while (!token.IsCancellationRequested)
                            {
                                session.SendRequest(InformationId.PollReqRes);
                                Thread.Sleep(3000); 
                            }
                        }
                    });


        }
    }
}