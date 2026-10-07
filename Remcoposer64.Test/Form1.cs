using NAudio.Midi;
using Remcoposer64.Core;
using Remcoposer64.Core.Timer;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Windows.Forms;
using static Remcoposer64.Common.Setting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Remcoposer64.Test
{
    public partial class Form1 : Form
    {
        private RmTimerContext ctx;   // ★ フィールドにする
        private RmTimer timer;        // ★ これもフィールドにする
        private System.Windows.Forms.Timer formTimer;

        private RingBuffer<CntPackData> ringBuffer = new RingBuffer<CntPackData>(102400);
        private NAudio.Midi.MidiOut midiOut;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //// Stopwatch
            //var sw = new TimeCountByStopWatch();
            //sw.Start();
            //Thread.Sleep(100);
            //lblStopwatch.Text = sw.ElapsedSec().ToString("F6") + " ms";

            //// DateTime
            //var dt = new TimeCountByDateTime();
            //dt.Start();
            //Thread.Sleep(100);
            //lblDateTime.Text = dt.ElapsedSec().ToString("F6") + " ms";

            //// PerformanceCounter
            //var pc = new TimeCountByPerformanceCounter();
            //pc.Start();
            //Thread.Sleep(100);
            //lblPerformanceCounter.Text = pc.ElapsedSec().ToString("F6") + " ms";


            // 利用可能な MIDI 出力デバイス一覧を表示
            cmbDevice.Items.Clear();
            for (int i = 0; i < NAudio.Midi.MidiOut.NumberOfDevices; i++)
            {
                cmbDevice.Items.Add(NAudio.Midi.MidiOut.DeviceInfo(i).ProductName);
            }
            cmbDevice.SelectedIndex = 0;

            formTimer = new System.Windows.Forms.Timer();
            formTimer.Interval = 30; // 15msごとに実行
            formTimer.Tick += FormTimer_Tick;
            formTimer.Start();

        }

        private void Form1_Shown(object sender, EventArgs e)
        {

        }

        private void FormTimer_Tick(object? sender, EventArgs e)
        {
            if (timer == null) return;

            lblRmCounter.Text = $"Seq: {timer.SeqCounter} : {timer.SeqCounter / 44100.0:F6} sec";
        }

        class MockHandler : IRmTimerHandler
        {
            private RmTimerContext ctx;
            private readonly RingBuffer<CntPackData> _ring;
            private readonly NAudio.Midi.MidiOut midiOut;

            public MockHandler(RmTimerContext ctx,RingBuffer<CntPackData> ring, NAudio.Midi.MidiOut midiOut)
            {
                _ring = ring;
                this.midiOut = midiOut;
                this.ctx = ctx;
            }

            long oldCounter = 0;

            public RmTimer.FrameFlowControl OnFrame(long seq)
            {
                while (_ring.Peek(out var p))
                {
                    if (p.Counter > seq)
                        break;

                    // ★ Tempo MetaEvent の検出
                    if (p.pack.Status == 0xFF)
                    {
                        if (p.pack.SysEx[1] == 0x51)
                        {
                            //// FF 51 03 tt tt tt
                            //int tempoUSec =
                            //    (p.pack.SysEx[2] << 16) |
                            //    (p.pack.SysEx[3] << 8) |
                            //    (p.pack.SysEx[4]);

                            //ctx.SetTempo(tempoUSec);

                            //計算時にテンポは考慮済み
                        }

                    }
                    else if (p.pack.SysEx != null)
                    {
                        // SysEx の場合
                        midiOut.SendBuffer(p.pack.SysEx);
                    }
                    else
                    {
                        // 通常ショートメッセージ
                        midiOut.Send(p.pack.RawData);
                        //if (oldCounter > p.Counter)
                        //Debug.WriteLine($"oldCounter:{oldCounter} Counter:{p.Counter}");
                        oldCounter = p.Counter;
                        //Debug.WriteLine($"Counter:{p.Counter} seq:{seq} Sent: {p.pack.Status:X2} {p.pack.Data1:X2} {p.pack.Data2:X2}");
                    }

                    _ring.Advance();
                }
                return RmTimer.FrameFlowControl.Normal;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer != null)
            {
                timer.RequestStop();
                timer.Dispose();
            }
            if(midiOut != null)
            {
                try
                {
                    SendAllSoundOff(midiOut);
                    midiOut.Reset();
                    midiOut.Dispose();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        private void btnRef_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.Filter = "MIDIファイル(*.mid)|*.mid|すべてのファイル(*.*)|*.*";
            DialogResult result = fd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string filePath = fd.FileName;
                txtMIDIFile.Text = filePath;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (btnPlay.Text != "Play")
            {
                btnPlay.Text = "Play";
                timer.RequestStop();
                timer.Dispose();
                midiOut.Reset();
                midiOut.Dispose();
                return;
            }

            // 選択されたデバイスを開く
            int deviceIndex = -1;
            if(cmbDevice.SelectedItem == null)
            {
                MessageBox.Show("MIDI出力デバイスが選択されていません。");
                return;
            }
            string selectedDevice = cmbDevice.SelectedItem.ToString();
            selectedDevice = selectedDevice.Trim();

            for (int i = 0; i < NAudio.Midi.MidiOut.NumberOfDevices; i++)
            {
                string deviceName = NAudio.Midi.MidiOut.DeviceInfo(i).ProductName.Trim();
                if (selectedDevice != deviceName)
                    continue;

                deviceIndex = i;
                break;
            }
            if(deviceIndex == -1)
            {
                MessageBox.Show("MIDI出力デバイスが選択されていません。");
                return;
            }

            btnPlay.Text = "Stop";
            midiOut = new NAudio.Midi.MidiOut(deviceIndex);

            var smfdata = SMFParser.ParseSmf(txtMIDIFile.Text);
            var events = smfdata.Events;
            int ppq = smfdata.PPQ;

            ringBuffer = new RingBuffer<CntPackData>(events.Count);

            // デフォルトテンポ
            int tempoUSec = 500000;

            double currentSeconds = 0.0;
            long lastTick = 0;

            foreach (var ev in events)
            {
                long tick = ev.AbsTick;

                // 前イベントからの差分 Tick を現在テンポで時間に変換
                long deltaTick = tick - lastTick;
                double deltaSec = deltaTick * (tempoUSec / 1_000_000.0) / ppq;
                currentSeconds += deltaSec;

                long counter = (long)(currentSeconds * 44100);
                lastTick = tick;

                //Debug.WriteLine($"Tick: {tick}, Counter: {counter}, Status: {ev.Data[0]:X2}, Data: {BitConverter.ToString(ev.Data)}");

                byte status = ev.Data[0];

                if ((status & 0xF0) != 0xF0)
                {
                    // ショートメッセージ
                    byte d1 = ev.Data.Length > 1 ? ev.Data[1] : (byte)0;
                    byte d2 = ev.Data.Length > 2 ? ev.Data[2] : (byte)0;

                    ringBuffer.Push(new CntPackData
                    {
                        Counter = counter,
                        pack = new PackData
                        {
                            Status = status,
                            Data1 = d1,
                            Data2 = d2
                        }
                    });
                }
                else if (status == 0xF0 || status == 0xF7)
                {
                    // SysEx
                    byte[] full = FixSysEx(ev.Data); // F0/F7 を付ける処理（後述）

                    ringBuffer.Push(new CntPackData
                    {
                        Counter = counter,
                        pack = new PackData
                        {
                            SysEx = full
                        }
                    });
                }
                else if (status == 0xFF)
                {
                    // MetaEvent（テンポ変更など）
                    HandleMetaEvent(counter,ev.Data, ref tempoUSec);
                }
            }

            //long oldTick = 0;
            //int i = 0;
            //foreach (var ev in ringBuffer.buffer)
            //{
            //    if (ev == null) continue;
            //    if (ev.Counter < oldTick)
            //        throw new Exception("Tick order error");
            //    oldTick = ev.Counter;
            //    i++;
            //}

            ctx = new RmTimerContext
            {

                GetStepCounter = () => 0,
                SetStepCounter = v => { },

                IsInterrupted = () => false,
                GetCurrentMode = () => SendMode.RealTime,

                SendFrameData = () => { return; },// Debug.WriteLine("SendFrameData"),
                SendStopFrame = () =>
                {
                    SendAllSoundOff(midiOut); Debug.WriteLine("SendStopFrame"); return 0;
                },

                WaitSync = () => Debug.WriteLine("WaitSync")
            };
            ctx.Handler = new MockHandler(ctx, ringBuffer, midiOut);
            timer = new RmTimer(ctx);
            timer.RequestStart();



        }

        private byte[] FixSysEx(byte[] data)
        {
            // data[0] = F0 or F7
            // data[1] = length or first data byte
            // 君の方式をそのまま使う
            byte[] full = new byte[data.Length + 1];
            full[0] = 0xF0;
            Array.Copy(data, 1, full, 1, data.Length - 1);
            full[full.Length - 1] = 0xF7;
            return full;
        }

        private void HandleMetaEvent(long counter, byte[] data, ref int tempoUSec)
        {
            if (data[1] == 0x51) // SetTempo
            {
                // data[2..4] が 24bit のテンポ値
                tempoUSec = (data[2] << 16) | (data[3] << 8) | data[4];
            }

            ringBuffer.Push(new CntPackData
            {
                Counter = counter,
                pack = new PackData
                {
                    Status=0xff,
                    SysEx = data
                }
            });
        }

        public void SendAllSoundOff(NAudio.Midi.MidiOut midiout)
        {
            try
            {

                for (int ch = 0; ch < 16; ch++)
                {
                    // All Sound Off (CC120)
                    midiOut.Send((0xB0 | ch) | (120 << 8) | (0 << 16));
                    // All Notes Off (CC123)
                    midiOut.Send((0xB0 | ch) | (123 << 8) | (0 << 16));
                    // Hold Pedal Off (CC64)
                    midiOut.Send((0xB0 | ch) | (64 << 8) | (0 << 16));
                    // Reset All Controllers (CC121)
                    midiOut.Send((0xB0 | ch) | (121 << 8) | (0 << 16));
                    // Pitch Bend Center (E0 00 40)
                    midiOut.Send((0xE0 | ch) | (0 << 8) | (64 << 16));
                }
            }
            catch { }
        }
    }
}