using Remcoposer64.Core.Timer;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Remcoposer64.Test
{
    public partial class Form1 : Form
    {
        private RmTimerContext ctx;   // ★ フィールドにする
        private RmTimer timer;        // ★ これもフィールドにする
        private System.Windows.Forms.Timer formTimer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Stopwatch
            var sw = new TimeCountByStopWatch();
            sw.Start();
            Thread.Sleep(100);
            lblStopwatch.Text = sw.ElapsedSec().ToString("F6") + " ms";

            // DateTime
            var dt = new TimeCountByDateTime();
            dt.Start();
            Thread.Sleep(100);
            lblDateTime.Text = dt.ElapsedSec().ToString("F6") + " ms";

            // PerformanceCounter
            var pc = new TimeCountByPerformanceCounter();
            pc.Start();
            Thread.Sleep(100);
            lblPerformanceCounter.Text = pc.ElapsedSec().ToString("F6") + " ms";

            ctx = new RmTimerContext
            {
                Handler = new MockHandler(lblRmCounter),

                GetStepCounter = () => 0,
                SetStepCounter = v => { },

                IsInterrupted = () => false,
                GetCurrentMode = () => SendMode.RealTime,

                SendFrameData = () => Console.WriteLine("SendFrameData"),
                SendStopFrame = () => { Console.WriteLine("SendStopFrame"); return 0; },

                WaitSync = () => Console.WriteLine("WaitSync")
            };

            formTimer = new System.Windows.Forms.Timer();
            formTimer.Interval = 30; // 15msごとに実行
            formTimer.Tick += FormTimer_Tick;
            formTimer.Start();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            timer = new RmTimer(ctx);

            timer.RequestStart();

        }

        private void FormTimer_Tick(object? sender, EventArgs e)
        {
            lblRmCounter.Text = $"Seq: {timer.SeqCounter} : {timer.SeqCounter / 44100.0:F6} sec";
        }

        class MockHandler : IRmTimerHandler
        {
            private readonly Label _label;

            public MockHandler(Label label)
            {
                _label = label;
            }

            public RmTimer.FrameFlowControl OnFrame(long seq)
            {

                return RmTimer.FrameFlowControl.Normal;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer.RequestStop();
            timer.Dispose();
        }
    }
}
