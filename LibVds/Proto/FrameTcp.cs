using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibVds.Proto
{
    using System.Diagnostics;
    using System.IO;
    using System.Net.Mime;
    using System.Security.Cryptography;

    public class FrameTcp
    {
        private readonly byte[] buffer;

        private const int payloadStartIndex = 17;

        public FrameTcp(ushort keyNumber, ushort length, byte[] input)
        {
            if (keyNumber == 0x0000)
            {
                // unsecured transmission
                this.buffer = new byte[2 + 2 + length];
                Array.Copy(input, 4, this.buffer, 4, length);
            }
            else
            {
                // secured transmission, AES is used -> decryption needed
                var tmpLen = length / 16.0;
                var aesLength = (int)(Math.Ceiling(tmpLen) * 16);
                this.buffer = new byte[2 + 2 + aesLength];
                Array.Copy(input, 4, this.buffer, 4, length);

                // decrypt buffer
                if (!SessionVdS.AesKeyList.ContainsKey(keyNumber))
                {
                    throw new Exception("Unknown KeyNumber -> Return corresponding error frame");
                }

                this.DecryptAes(SessionVdS.AesKeyList[keyNumber]);
            }

            this.KeyNumber = keyNumber;
            this.FrameLength = length;
        }

        /// <summary>
        /// For creating outgoing frames
        /// </summary>
        /// <param name="sendCounter"></param>
        /// <param name="receiveCounter"></param>
        /// <param name="payload"></param>
        public FrameTcp(uint sendCounter, uint receiveCounter, ushort keyNumber, InformationId informationId, params FrameVdS[] payload)
        {
            // AES frames must be blocks of 16 bytes
            //var tmpLen = (payload.Sum(p => p.GetByteCount()) + 13) / 16.0;
            //var aesLength = (int)(Math.Ceiling(tmpLen) * 16);
            var aesLength = 160;
            this.FillByteCount = aesLength - payload.Sum(p => p.GetByteCount() + 13);

            // K + SL + aesBuffer
            this.buffer = new byte[2 + 2 + aesLength];

            // copy payload into buffer
            var payloadBytes = payload.SelectMany(p => p.Serialize()).ToArray();
            Array.Copy(payloadBytes, 0, this.buffer, 0 + 2 + 2 + 4 + 2 + 4 + 1 + 1 + 1, payloadBytes.Length);


            this.KeyNumber = keyNumber;
            this.FrameLength = (ushort)(this.buffer.Length - 4);

            this.SendCounter = sendCounter;
            this.ReceiveCounter = receiveCounter;
            this.InformationId = informationId;
            this.ProtocolId = ProtocolId.VdS2465;
            this.PayloadLength = (byte)payloadBytes.Length;

            // set CRC
            this.Checksum = this.CalculateCrc();
        }

        public ushort KeyNumber
        {
            get
            {
                return BitConverter.ToUInt16(
                    this.buffer
                    .Take(2)
                    .Reverse()
                    .ToArray(),
                    0);
            }

            private set
            {
                Array.Copy(
                    BitConverter.GetBytes(value)
                        .Reverse()
                        .ToArray(),
                        0,
                        this.buffer,
                        0,
                        2);
            }
        }


        public ushort FrameLength
        {
            get
            {
                return BitConverter.ToUInt16(
                    this.buffer
                    .Skip(2)
                    .Take(2)
                    .Reverse()
                    .ToArray(),
                    0);
            }

            private set
            {
                Array.Copy(
                    BitConverter.GetBytes(value)
                    .Reverse()
                    .ToArray(),
                    0,
                    this.buffer,
                    2,
                    2);
            }
        }

        public uint SendCounter
        {
            get
            {
                return BitConverter.ToUInt32(
                  this.buffer
                  .Skip(2 + 2)
                  .Take(4)
                  .Reverse()
                  .ToArray(),
                  0);
            }

            private set
            {
                Array.Copy(
                    BitConverter.GetBytes(value)
                    .Reverse()
                    .ToArray(),
                    0,
                    this.buffer,
                    2 + 2,
                    4);
            }
        }

        public ushort Checksum
        {
            get
            {
                return BitConverter.ToUInt16(
                  this.buffer
                  .Skip(2 + 2 + 4)
                  .Take(2)
                  .Reverse()
                  .ToArray(),
                  0);
            }

            private set
            {
                Array.Copy(
                    BitConverter.GetBytes(value)
                    .Reverse()
                    .ToArray(),
                    0,
                    this.buffer,
                    2 + 2 + 4,
                    2);
            }
        }

        public uint ReceiveCounter
        {
            get
            {
                return BitConverter.ToUInt32(
                  this.buffer
                  .Skip(2 + 2 + 4 + 2)
                  .Take(4)
                  .Reverse()
                  .ToArray(),
                  0);
            }

            private set
            {
                Array.Copy(
                    BitConverter.GetBytes(value)
                    .Reverse()
                    .ToArray(),
                    0,
                    this.buffer,
                    2 + 2 + 4 + 2,
                    4);
            }
        }

        public InformationId InformationId
        {
            get
            {
                return (InformationId)this.buffer[2 + 2 + 4 + 2 + 4];
            }

            private set
            {
                this.buffer[2 + 2 + 4 + 2 + 4] = (byte)value;
            }
        }

        public ProtocolId ProtocolId
        {
            get
            {
                return (ProtocolId)this.buffer[2 + 2 + 4 + 2 + 4 + 1];
            }

            private set
            {
                this.buffer[2 + 2 + 4 + 2 + 4 + 1] = (byte)value;
            }
        }

        public byte PayloadLength
        {
            get
            {
                return this.buffer[2 + 2 + 4 + 2 + 4 + 1 + 1];
            }

            private set
            {
                this.buffer[2 + 2 + 4 + 2 + 4 + 1 + 1] = value;
            }
        }

        public FrameVdS[] Payload
        {
            get
            {
                var payloadFrames = new List<FrameVdS>();
                var totalPayloadCnt = 0;

                while (totalPayloadCnt < this.PayloadLength)
                {
                    var vds = new FrameVdS(this.buffer, payloadStartIndex + totalPayloadCnt, this.InformationId);
                    totalPayloadCnt += vds.GetByteCount();
                    payloadFrames.Add(vds);
                }

                return payloadFrames.ToArray();
            }

            //set
            //{
            //    var payloadBuffer = value.SelectMany(p => p.Serialize()).ToArray();
            //    Array.Copy(payloadBuffer, 0, this.buffer, payloadStartIndex, payloadBuffer.Length);
            //}
        }

        private int FillByteCount { get; set; }

        public static FrameTcp CreateSyncRequest(uint sendCounter, uint receiveCounter)
        {
            var frame = new FrameTcp(sendCounter, receiveCounter, 0, InformationId.SyncReq, FrameVdS.CreateSyncRequestResponse(InformationId.SyncReq))
                            {
                                InformationId = InformationId.SyncReq,
                                SendCounter = sendCounter,
                                ReceiveCounter = receiveCounter
                            };

            return frame;
        }

        public byte[] Serialize()
        {
            if (this.KeyNumber != 0)
            {
                this.EncryptAes(SessionVdS.AesKeyList[this.KeyNumber]);
            }

            return this.buffer.ToArray();
        }

        private void EncryptAes(byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                aes.Key = key;
                aes.IV = new byte[16];

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var input = this.buffer.Skip(4).Take(this.buffer.Length - 4).ToArray();
                    Trace.WriteLine("Encrypting bytes: " + BitConverter.ToString(input));
                    cs.Write(input, 0, input.Length);
                    cs.FlushFinalBlock();

                    // overwrite buffer with encrypted data
                    var encrypted = ms.ToArray();
                    Trace.WriteLine("ENC: " + BitConverter.ToString(encrypted));
                    Array.Copy(encrypted, 0, this.buffer, 4, encrypted.Length);
                }
            }
        }

        private void DecryptAes(byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                aes.Key = key;
                aes.IV = new byte[16];

                var encrypted = this.buffer.Skip(4).ToArray();
                Trace.WriteLine("Decrypting bytes: " + BitConverter.ToString(encrypted));

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encrypted, 0, encrypted.Length);
                    cs.FlushFinalBlock();
                    var decrypted = ms.ToArray();
                    Trace.WriteLine("Decrypted bytes: " + BitConverter.ToString(decrypted));

                    // overwrite buffer with encrypted data
                    Array.Clear(this.buffer, 4, this.buffer.Length - 4);
                    Array.Copy(decrypted, 0, this.buffer, 4, decrypted.Length);
                }
            }
        }

        private ushort CalculateCrc()
        {
            // RFC1071
            byte[] input = this.buffer.Skip(4).Take(this.buffer.Length - 4 - this.FillByteCount).ToArray();
            int length = input.Length;
            int i = 0;
            uint sum = 0;
            uint data = 0;
            while (length > 1)
            {
                data = (uint)(
                ((uint)(input[i]) << 8)
                |
                ((uint)(input[i + 1]) & 0xFF));

                sum += data;
                if ((sum & 0xFFFF0000) > 0)
                {
                    sum = sum & 0xFFFF;
                    sum += 1;
                }

                i += 2;
                length -= 2;
            }

            if (length > 0)
            {
                sum += (uint)(input[i] << 8);
                //sum += (UInt32)(buffer[i]);
                if ((sum & 0xFFFF0000) > 0)
                {
                    sum = sum & 0xFFFF;
                    sum += 1;
                }
            }
            sum = ~sum;
            sum = sum & 0xFFFF;
            return (ushort)sum;
        }

        public override string ToString()
        {
            return string.Format(
                "K: {0}, SL: {1}, TC: {2}, CRC: {3}, RC: {4}, IK: {5}, PK: {6}, L: {7}, DATA: {8}",
                this.KeyNumber,
                this.FrameLength,
                this.SendCounter,
                this.Checksum,
                this.ReceiveCounter,
                this.InformationId,
                this.ProtocolId,
                this.PayloadLength,
                BitConverter.ToString(this.Payload.SelectMany(p => p.Serialize()).ToArray()));
        }
    }
}
