using NAudio.Midi;
using Remcoposer64.Common;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Remcoposer64.Core
{
    public sealed class ThrottledMidiOut : IDisposable
    {
        private class MidiPacket
        {
            public bool IsSysEx;
            public int Message;
            public byte[] SysExData;
            public int Size;
        }

        private readonly MidiOut midiOut;
        public readonly Setting.midiOutInfo midiOutInfo;
        public MIDIParam midiParam;

        private readonly ConcurrentQueue<MidiPacket> queue = new();

        private Thread worker;

        private readonly Stopwatch sw = new();

        private volatile bool running = true;

        // 30byte/ms
        private readonly double bytesPerMs;

        private double tokens;
        private long lastMs;
        private readonly AutoResetEvent wakeup = new(false);
        private volatile bool sending = false;
        private volatile bool skipMode = false;
        private volatile bool quickMode = false;

        private int silentSendCount = 0;

        public int QueueCount => queue.Count;

        public ThrottledMidiOut(MidiOut midiOut,Setting.midiOutInfo midiOutInfo, double bytesPerMs = 30.0)
        {
            this.midiOut = midiOut;
            this.midiOutInfo = midiOutInfo;
            this.bytesPerMs = bytesPerMs;
            tokens = bytesPerMs;

            midiParam = new MIDIParam();
            sw.Start();
            lastMs = sw.ElapsedMilliseconds;

            worker = new Thread(SenderProc)
            {
                IsBackground = true,
                Name = "MidiSender"
            };

            worker.Start();
        }

        public void Init()
        {
            //running = false;
            //worker.Join();

            midiParam = new MIDIParam();
            //sw.Start();
            //lastMs = sw.ElapsedMilliseconds;

            //worker = new Thread(SenderProc)
            //{
            //    IsBackground = true,
            //    Name = "MidiSender"
            //};

            //worker.Start();

            queue.Clear();
        }

        /// <summary>
        /// NoteOn, CC, ProgramChange等
        /// </summary>
        public void Send(int message)
        {
            queue.Enqueue(new MidiPacket()
            {
                IsSysEx = false,
                Message = message,
                Size = 3
            });

            wakeup.Set();
        }

        /// <summary>
        /// SysEx
        /// </summary>
        public void SendSysEx(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            queue.Enqueue(new MidiPacket()
            {
                IsSysEx = true,
                SysExData = data,
                Size = data.Length
            });

            wakeup.Set();
        }

        private void SenderProc()
        {
            while (running)
            {
                if (queue.IsEmpty)
                {
                    wakeup.WaitOne();
                    if (!running) break;
                }

                RefillTokens();

                if (!queue.TryPeek(out MidiPacket packet))
                {
                    continue;
                }

                if (tokens < packet.Size)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (!queue.TryDequeue(out packet))
                    continue;

                sending = true;

                try
                {
                    if (packet.IsSysEx)
                    {
                        if (!skipMode ||(skipMode && !quickMode))
                            midiOut.SendBuffer(packet.SysExData);
                        midiParam.SendBuffer(packet.SysExData);
                        if (skipMode)
                        {
                            silentSendCount += packet.Size;
                        }
                    }
                    else
                    {
                        if(!skipMode) midiOut.Send(packet.Message);
                        midiParam.Send(packet.Message);
                    }

                    tokens -= packet.Size;
                }
                catch
                {
#if DEBUG
                    // 必要ならLog
#endif
                }
                finally
                {
                    sending = false;
                }
            }
        }

        private void RefillTokens()
        {
            long now = sw.ElapsedMilliseconds;

            if (now == lastMs)
                return;

            long elapsed = now - lastMs;

            tokens += elapsed * bytesPerMs;

            // 最大100ms分保持
            double maxTokens = bytesPerMs * 100.0;

            if (tokens > maxTokens)
                tokens = maxTokens;

            lastMs = now;
        }

        /// <summary>
        /// キューが空になるまで待つ
        /// </summary>
        public void Flush()
        {
            while (!queue.IsEmpty || sending)
            {
                Thread.Sleep(0);
            }
            skipMode = false;
            quickMode = false;
        }

        public void Flush2()
        {
            while (!queue.IsEmpty || sending)
            {
                Thread.Sleep(0);
            }
        }

        public int FlushMIDI()
        {
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                FlushMIDICh(i, ref count);
            }

            count += silentSendCount;
            silentSendCount = 0;
            skipMode = false;
            quickMode = false;

            return count;
        }
        public int FlushMIDI(int outChannel)
        {
            int count = 0;
            int i = outChannel;
            FlushMIDICh(i, ref count);

            count += silentSendCount;
            silentSendCount = 0;
            skipMode = false;
            quickMode = false;

            return count;
        }

        public void FlushMIDICh(int i,ref int count)
        {
            int status;
            int msg;

            status = 0xB0 | i; // CC
            for (int j = 0; j < 128; j++)
            {
                if ((j >= 0x62 && j <= 0x65) || j == 0x06 || j == 0x26) continue;

                var val = midiParam.cc[i][j];
                if (val.HasValue)
                {
                    msg = (int)(status | (j << 8) | (val << 16));
                    midiOut.Send(msg);
                    count += 3;
                    Flush2();
                }
            }

            var pc = midiParam.pc[i];
            if (pc.HasValue)
            {
                status = 0xC0 | i; // ProgramChange
                msg = (int)(status | (pc << 8));
                midiOut.Send(msg);
                count += 2;
                Flush2();
            }


            status = 0xb0 | i;

            foreach (var kv in midiParam.dicRPN[i])
            {
                int n = kv.Key;
                ushort val = kv.Value;

                byte msb = (byte)((n >> 8) & 0x7f);
                byte lsb = (byte)(n & 0x7f);
                midiOut.Send(status | (0x65 << 8) | (msb << 16));
                midiOut.Send(status | (0x64 << 8) | (lsb << 16));
                count += 6;

                msb = (byte)((val >> 8) & 0x7f);
                lsb = (byte)(val & 0x7f);
                midiOut.Send(status | (0x06 << 8) | (msb << 16));
                midiOut.Send(status | (0x26 << 8) | (lsb << 16));
                count += 6;
                Flush2();
            }

            foreach (var kv in midiParam.dicNRPN[i])
            {
                int n = kv.Key;
                ushort val = kv.Value;

                byte msb = (byte)((n >> 8) & 0x7f);
                byte lsb = (byte)(n & 0x7f);
                midiOut.Send(status | (0x63 << 8) | (msb << 16));
                midiOut.Send(status | (0x62 << 8) | (lsb << 16));

                midiOut.Send(status | (0x06 << 8) | (val << 16));
                count += 9;
                Flush2();
            }

            var be = midiParam.bend[i];
            if (be.HasValue)
            {
                status = 0xE0 | i; // PitchBend
                int bend = (int)be;
                byte lsb = (byte)(bend & 0x7F);
                byte msb = (byte)((bend >> 7) & 0x7F);
                msg = status | (lsb << 8) | (msb << 16);
                midiOut.Send(msg);
                count += 3;
                Flush2();
            }

            var cp = midiParam.cPress[i];
            if (cp.HasValue)
            {
                status = 0xD0 | i; // Channel Pressure
                msg = (int)(status | (cp << 8));
                midiOut.Send(msg);
                count += 2;
                Flush2();
            }

            status = 0xA0 | i; // Polyphonic Key Pressure
            for (int j = 0; j < 128; j++)
            {
                var val = midiParam.keyPress[i][j];
                if (val.HasValue)
                {
                    msg = (int)(status | (j << 8) | (val << 16));
                    midiOut.Send(msg);
                    count += 3;
                    Flush2();
                }
            }
        }

        public void Dispose()
        {
            Flush();

            running = false;
            
            wakeup.Set();

            worker.Join();

            midiOut?.Dispose();
        }

        internal void SkipModeOn(bool QuickMode)
        {
            skipMode = true;
            quickMode = QuickMode;
            silentSendCount = 0;
        }

        internal void SkipModeOff()
        {
            skipMode = false;
            quickMode = false;
        }
    }
}