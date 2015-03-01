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

        public FrameTcp(ushort key, ushort length, byte[] input)
        {
            if (key == 0x0000)
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
                if (!Session.AesKeyList.ContainsKey(key))
                {
                    throw new Exception("Unknown key -> Return corresponding error frame");
                }

                this.DecryptAes(Session.AesKeyList[key]);
            }

            this.Key = key;
            this.FrameLength = length;
        }

        /// <summary>
        /// For creating outgoing frames
        /// </summary>
        /// <param name="sendCounter"></param>
        /// <param name="receiveCounter"></param>
        /// <param name="payload"></param>
        public FrameTcp(uint sendCounter, uint receiveCounter, ushort key, params FrameVdS[] payload)
        {
            // AES frames must be blocks of 16 bytes
            var tmpLen = (payload.Sum(p => p.GetByteCount()) + 13) / 16.0;
            var aesLength = (int)(Math.Ceiling(tmpLen) * 16);
            this.FillByteCount = aesLength - payload.Sum(p => p.GetByteCount() + 13);

            // K + SL + aesBuffer
            this.buffer = new byte[2 + 2 + aesLength];

            // copy payload into buffer
            var payloadBytes = payload.SelectMany(p => p.Serialize()).ToArray();
            Array.Copy(payloadBytes, 0, this.buffer, 0 + 2 + 2 + 4 + 2 + 4 + 1 + 1 + 1, payloadBytes.Length);

            this.Key = key;
            this.FrameLength = (ushort)(this.buffer.Length - 4);

            this.SendCounter = sendCounter;
            this.ReceiveCounter = receiveCounter;
            this.InformationId = InformationId.Payload;
            this.ProtocolId = ProtocolId.VdS2465;
            this.PayloadLength = (byte)payloadBytes.Length;

            // set CRC
            this.Checksum = this.CalculateCrc();
        }

        public ushort Key
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
                var frame = new FrameVdS(this.buffer, 17, this.PayloadLength);
                return new [] { frame };
            }
        }

        private int FillByteCount { get; set; }

        public byte[] Serialize()
        {
            if (this.Key != 0)
            {
                this.EncryptAes(Session.AesKeyList[this.Key]);
            }

            return this.buffer.ToArray();
        }

        private void EncryptAes(byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;

                aes.Key = key;
                aes.IV = new byte[16];

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var input = this.buffer.Skip(4).Take(this.buffer.Length - 4 - this.FillByteCount).ToArray();
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

    }
}
