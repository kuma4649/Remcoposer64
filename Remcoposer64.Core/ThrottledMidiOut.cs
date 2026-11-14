using NAudio.Midi;
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
        public MIDIParam midiParam;

        private readonly ConcurrentQueue<MidiPacket> queue = new();

        private Thread worker;

        private readonly Stopwatch sw = new();

        private volatile bool running = true;

        // 30byte/ms
        private readonly double bytesPerMs;

        private double tokens;
        private long lastMs;
        private bool skipMode = false;

        public int QueueCount => queue.Count;

        public ThrottledMidiOut(MidiOut midiOut, double bytesPerMs = 30.0)
        {
            this.midiOut = midiOut;
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
        }

        private void SenderProc()
        {
            while (running)
            {
                RefillTokens();

                if (!queue.TryPeek(out MidiPacket packet))
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (tokens < packet.Size)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (!queue.TryDequeue(out packet))
                    continue;

                try
                {
                    if (packet.IsSysEx)
                    {
                        midiOut.SendBuffer(packet.SysExData);
                        midiParam.SendBuffer(packet.SysExData);
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
                    // 必要ならLog
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
            while (queue.Count > 0)
            {
                Thread.Sleep(1);
            }
            skipMode = false;
        }

        public void Flush2()
        {
            while (queue.Count > 0)
            {
                Thread.Sleep(1);
            }
        }

        public void FlushMIDI()
        {
            int status;
            int msg;
            for (int i = 0; i < 16; i++)
            {
                status = 0xA0 | i; // Polyphonic Key Pressure
                for (int j = 0; j < 128; j++)
                {
                    if (midiParam.keyPress[i][j] != null)
                    {
                        msg = (int)(status + (j << 8) + (midiParam.keyPress[i][j] << 16));
                        midiOut.Send(msg);
                    }
                }
                while (queue.Count > 0)
                {
                    Thread.Sleep(1);
                }

                status = 0xB0 | i; // CC
                for (int j = 0; j < 128; j++)
                {
                    if (midiParam.cc[i][j] != null)
                    {
                        msg = (int)(status + (j << 8) + (midiParam.cc[i][j] << 16));
                        midiOut.Send(msg);
                    }
                }
                while (queue.Count > 0)
                {
                    Thread.Sleep(1);
                }

                if (midiParam.pc[i] != null)
                {
                    status = 0xC0 | i; // ProgramChange
                    msg = (int)(status + (midiParam.pc[i] << 8));
                    midiOut.Send(msg);
                }
                while (queue.Count > 0)
                {
                    Thread.Sleep(1);
                }

                status = 0xD0 | i; // Polyphonic Key Pressure
                if (midiParam.cPress[i] != null)
                {
                    msg = (int)(status + (midiParam.cPress[i] << 8));
                    midiOut.Send(msg);
                }
                while (queue.Count > 0)
                {
                    Thread.Sleep(1);
                }

                status = 0xE0 | i; // PitchBend
                if (midiParam.bend[i] != null)
                {
                    int bend = (int)midiParam.bend[i];
                    byte lsb = (byte)(bend & 0x7F);
                    byte msb = (byte)((bend >> 7) & 0x7F);
                    msg = status | (lsb << 8) | (msb << 16);
                    midiOut.Send(msg);
                }
                while (queue.Count > 0)
                {
                    Thread.Sleep(1);
                }
            }
            skipMode = false;
        }

        public void Dispose()
        {
            Flush();

            running = false;

            worker.Join();

            midiOut?.Dispose();
        }

        internal void SkipModeOn()
        {
            skipMode = true;
        }
        internal void SkipModeOff()
        {
            skipMode = false;
        }
    }
}