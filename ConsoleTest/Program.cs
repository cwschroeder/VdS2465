using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    using System.Diagnostics;

    using LibVds.Proto;

    class Program
    {
        static void Main(string[] args)
        {
            var s = new Session();

            var vdsBuff = new byte[] { 0x05, 0x03, 0x01, 0x02, 0x03, 0x04 };
            var vds = new FrameVdS(vdsBuff, 0, InformationId.Payload);
            var frameTcp = new FrameTcp(1, 1, 1, vds);
            foreach (var pl in frameTcp.Payload)
            {
                Trace.WriteLine("Payload: + " + BitConverter.ToString(pl.Serialize()));
            }
            var dataOut = frameTcp.Serialize();

            var receivedFrameTcp = new FrameTcp(frameTcp.Key, frameTcp.FrameLength, dataOut);


            Console.ReadLine();
        }
    }
}
