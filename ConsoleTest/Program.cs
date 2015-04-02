using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        static void Main(string[] args)
        {

            Console.WriteLine("Start");
            Driver.Read();


            Console.ReadLine();
            var syncReq = FrameTcp.CreateSyncRequest(2107159040, 0);
            var syncRes = new FrameTcp(493594511, 2107159041, 12345, InformationId.SyncRes, FrameVdS.CreateSyncRequestResponse(InformationId.SyncRes));

            var pollReq = new FrameTcp(
                2107159041,
                493594512,
                12345,
                InformationId.PollReqRes,
                FrameVdS.CreateEmpty(InformationId.PollReqRes));

            Trace.WriteLine("POLLREQ: " + BitConverter.ToString(pollReq.Serialize()));

            var cts = new CancellationTokenSource();
            //Server.Run(null, cts.Token);
            //Thread.Sleep(200);

            var bavaria = new IPEndPoint(IPAddress.Parse("176.94.30.165"), 9000);
            var clientSession = Client.Run(bavaria, cts.Token);

            while (true)
            {
                //clientSession.AddMessage(FrameVdS.CreatePayloadType02(0x14, 0x03, 0x02, 0x01, 0x21));
                clientSession.AddMessage(FrameVdS.CreatePayloadType56(new byte[] { 0x99, 0x99, 0x99 }));
                
                Thread.Sleep(3000);
            }


            Console.ReadLine();
        }
    }
}
