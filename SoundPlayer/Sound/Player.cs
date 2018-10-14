using System.IO;
using System.Threading;
using SoundIOSharp;

/*
 * 実装に当たってはexternal/libsoundio-sharp/example/sio-sine/Program.csを全面的に参考し、
 * 一部そのまま利用しています。
 *
 * 元のコードのライセンスを以下に張っておきます。
 *
 *  The MIT License
 *
 *   Copyright (c) 2017 Atsushi Eno
 *
 *   Permission is hereby granted, free of charge, to any person obtaining a copy
 *   of this software and associated documentation files (the "Software"), to deal
 *   in the Software without restriction, including without limitation the rights
 *   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *   copies of the Software, and to permit persons to whom the Software is
 *   furnished to do so, subject to the following conditions:
 *
 *   The above copyright notice and this permission notice shall be included in
 *   all copies or substantial portions of the Software.
 *
 *   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *   THE SOFTWARE.
 */

namespace SoundPlayer.Sound
{
    public class Player
    {
        /// <summary>
        /// サウンドを再生する。
        /// </summary>
        public void Play(WaveFile file)
        {
            var api = new SoundIO();
            api.Connect();
            api.FlushEvents();
            var device = api.GetOutputDevice(api.DefaultOutputDeviceIndex);
            if (device == null || device.ProbeError != 0)
            {
                throw new IOException("device not found");
            }

            var outStream = device.CreateOutStream();
            if (outStream == null)
            {
                throw new IOException("failed to create out stream");
            }
            outStream.SoftwareLatency = 0.0;
            outStream.SampleRate = (int) file.SamplingRate;
            outStream.Layout = SoundIOChannelLayout.GetDefault(file.NumChannels);
            outStream.Format = SoundIODevice.Float32NE;

            var count = 0;
            var finish = false;
            outStream.WriteCallback = (_, max) => WriteCallback(
                outStream, max, ref count, ref finish, file.WaveData);

            outStream.Open();
            outStream.Start();

            for (; ;)
            {
                api.FlushEvents();
                Thread.Sleep(1);
                if (finish)
                {
                    break;
                }
            }

            outStream.Dispose();
            device.RemoveReference();
            api.Dispose();
        }

        private static unsafe void WriteCallback(
            SoundIOOutStream outStream,
            int frameCountMax,
            ref int count, ref bool finish, short[] data)
        {
            var framesLeft = frameCountMax;

            for (; !finish; )
            {
                var frameCount = framesLeft;
                var results = outStream.BeginWrite(ref frameCount);

                if (frameCount == 0)
                    break;

                SoundIOChannelLayout layout = outStream.Layout;

                for (var frame = 0; frame < frameCount; frame += 1)
                {
                    var sample = (double)(data[count]) / short.MaxValue;
                    count++;
                    for (var channel = 0; channel < layout.ChannelCount; channel += 1)
                    {
                        var area = results.GetArea(channel);
                        var buf = (float*) area.Pointer;
                        *buf = (float) sample;
                        area.Pointer += area.Step;
                    }
                    if (count >= data.Length)
                    {
                        finish = true;
                        break;
                    }

                }

                outStream.EndWrite();

                framesLeft -= frameCount;
                if (framesLeft <= 0 || finish)
                    break;
            }
        }
    }
}
