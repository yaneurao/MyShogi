using System;
using System.IO;

namespace SoundPlayer.Sound
{
    public class WaveFile
    {
        public WaveFile(string path)
        {
            var data = readFile(path);
            var header = new RiffHeader(data);
            if (!header.Valid())
            {
                throw new IOException("Invalid format: Seems not wav format");
            }

            var fmt = new FormatChunk(data, header.Offset);
            if (!fmt.Valid())
            {
                throw new IOException("Invalid format: unsupported wav format");
            }

            var wav = new DataChunk(data, header.Offset + fmt.Offset);
            if (!wav.Valid())
            {
                throw new IOException("Invalid format: broken file");
            }

            WaveData = wav.Data;
            SamplingRate = fmt.SamplingRate;
            NumChannels = fmt.NumChannels;
        }

        private static byte[] readFile(string path)
        {
            byte[] data;
            using (var fp = new FileStream(path, FileMode.Open))
            {
                if (fp.Length > 256 * 1024 * 1024)
                {
                    throw new IOException("File size is too large");
                }

                data = new byte[fp.Length];
                fp.Read(data, 0, data.Length);
            }

            return data;
        }

        public short [] WaveData;
        public uint SamplingRate;
        public ushort NumChannels;
    }

    /*
     *
     * Wavファイルフォーマットの実装
     * 公式の仕様が見つからなかったので下記サイトを参考に総合的に判断して実装
     * http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html
     * http://www.graffiti.jp/pc/p030506a.htm
     * http://sky.geocities.jp/kmaedam/directx9/waveform.html
     * http://www.web-sky.org/program/other/wave.php
     * https://www.youfit.co.jp/archives/1418
     *
     */
    class ParseUtil
    {
        /*
         * リトルエンディアン表記されたバイト列から数字を復元する
         */

        public static uint buildUInt(byte v1, byte v2, byte v3, byte v4)
        {
            uint ret = v4;
            ret = (ret << 8) + v3;
            ret = (ret << 8) + v2;
            ret = (ret << 8) + v1;
            return ret;
        }

        public static ushort buildUShort(byte v1, byte v2)
        {
            ushort ret = (ushort) (v2 << 8);
            ret += v1;
            return ret;
        }

    }
    class RiffHeader
    {
        public RiffHeader(byte[] data)
        {
            RiffCheck = new[]
            {
                (char) data[0],
                (char) data[1],
                (char) data[2],
                (char) data[3]
            };

            Size = ParseUtil.buildUInt(data[4], data[5], data[6], data[7]);

            FileType = new[]
            {
                (char) data[8],
                (char) data[9],
                (char) data[10],
                (char) data[11]
            };
        }

        public bool Valid()
        {
            if (new string(RiffCheck) != "RIFF")
            {
                return false;
            }

            if (new string(FileType) != "WAVE")
            {
                return false;
            }

            return true;
        }

        public uint Offset => 12;

        private uint ChunkSize;
        private char[] RiffCheck;
        private uint Size;
        private char[] FileType;
    }

    class FormatChunk
    {
        public FormatChunk(byte[] data, uint p)
        {
            FormatCheck = new[]
            {
                (char) data[p],
                (char) data[p+1],
                (char) data[p+2],
                (char) data[p+3]
            };

            ChunkSize = ParseUtil.buildUInt(data[p + 4], data[p + 5], data[p + 6], data[p + 7]);
            FormatCode = ParseUtil.buildUShort(data[p + 8], data[p + 9]);
            NumChannels = ParseUtil.buildUShort(data[p + 10], data[p + 11]);
            SamplingRate = ParseUtil.buildUInt(data[p + 12], data[p + 13], data[p + 14], data[p + 15]);
            BytesPerSec = ParseUtil.buildUInt(data[p + 16], data[p + 17], data[p + 18], data[p + 19]);
            BlockAlign = ParseUtil.buildUShort(data[p + 20], data[p + 21]);
            BitsPerSample = ParseUtil.buildUShort(data[p + 22], data[p + 23]);
        }

        public bool Valid()
        {
            if (new string(FormatCheck) != "fmt ")
            {
                return false;
            }

            if (FormatCode != 0x1)
            {
                return false;
            }

            if (BitsPerSample != 0x10)
            {
                return false;
            }

            return true;
        }

        public uint Offset => ChunkSize + 8;

        private char[] FormatCheck;
        private uint ChunkSize;

        // 0x1 (Linear PCM)のはず…
        private ushort FormatCode;

        public ushort NumChannels { get; }
        public uint SamplingRate { get; }
        private uint BytesPerSec;
        private ushort BlockAlign;

        // 0x10 = (16bit 前提)
        private ushort BitsPerSample;

        // FormatCode == 0x1のとき以下の2フィールドは存在すらしないってなにそれ。
        // private ushort ExtendedSize;
        // private byte[] Extended;
    }

    class DataChunk
    {
        public DataChunk(byte[] data, uint p)
        {
            FormatCheck = new[]
            {
                (char) data[p],
                (char) data[p+1],
                (char) data[p+2],
                (char) data[p+3]
            };

            ChunkSize = ParseUtil.buildUInt(data[p + 4], data[p + 5], data[p + 6], data[p + 7]);

            Data = new short[ChunkSize / 2];

            // (注意) リトルエンディアンを仮定しているのでビッグエンディアンのCPUで動かすと多分バグります。
            Buffer.BlockCopy(data, (int) p + 8, Data, 0, (int) ChunkSize);
        }

        public bool Valid()
        {
            if (new string(FormatCheck) != "data")
            {
                return false;
            }

            return true;
        }

        private char[] FormatCheck;
        private uint ChunkSize;

        // FormatChunkのBitsPerSampleが16のとき符号付きshort型
        public short[] Data { get; }
    }

}
