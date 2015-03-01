namespace LibVds.Proto
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Security.Cryptography;


    
    public class Session
    {
        // Current send counter, must be incremented with each new outgoing frame
        public uint MySendCounter { get; private set; }

        // The last received TC of the remote peer
        public uint OtherSendCounter { get; private set; }

        // indicates whether a key (AES/CHIASMUS) is used or not
        public bool IsSecured { get; set; }

        // AES/CHIASMUS key or 0 in case of unsecured communication
        public ushort KeyNumber { get; set; }

        public static Dictionary<ushort, byte[]> AesKeyList = new Dictionary<ushort, byte[]>();

        static Session()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                for (ushort i = 0; i < 10; i++)
                {
                    aes.GenerateKey();
                    AesKeyList[i] = new byte[]{0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
                }
            }
        }

        /// <summary>
        /// Must be called for each outgoing frame
        /// </summary>
        public void IncrementMySendCounter()
        {
            if (this.MySendCounter == uint.MaxValue - 1)
            {
                this.MySendCounter = 0;
                return;
            }

            this.MySendCounter++;
        }

        /// <summary>
        /// Must be called for each incoming frame
        /// </summary>
        public void IncrementOtherSendCounter()
        {
            if (this.OtherSendCounter == uint.MaxValue - 1)
            {
                this.OtherSendCounter = 0;
                return;
            }

            this.OtherSendCounter++;
        }
    }
}