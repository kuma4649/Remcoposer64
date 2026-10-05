using Remcoposer64.Common;

namespace Remcoposer64.Core.Timer
{
    public class RmTimer : IDisposable
    {
        // ============================================================
        //  フィールド
        // ============================================================

        private bool disposed = false;
        private Thread workerThread;

        private readonly object lockObj = new object();

        // 割り込み周波数（44100Hz）
        private readonly int Frq = Common.Common.DATA_SEQUENCE_FREQUENCE;

        // シーケンスカウンタ初期値（負値からスタートするのは音源初期化のため）
        private const long Def_SeqCounter = -1500;
        public long SeqCounter = Def_SeqCounter;

        // シーケンス進行速度（テンポ変化などに使用）
        private double SeqSpeed = 0.0;
        private double SeqSpeedDelta = 1.0;

        // スレッド制御フラグ
        protected volatile bool Start = false;
        protected volatile bool isRunning = false;
        public bool unmount = false;
        public bool procExit = false;

        // 使用するタイマー方式
        private MusicInterruptTimer musicInterruptTimer = MusicInterruptTimer.StopWatch;

        // デバッグ用（処理落ち検出）
        private long processOverFlowCounter = 0;
        private double process1_Lap;
        private double process2_Lap;
        private int skipframe = 1;

        // 同期リセット要求
        private bool reqResetSync = false;

        private RmTimerContext context;       



        // ============================================================
        //  コンストラクタ
        // ============================================================
        public RmTimer(RmTimerContext ctx)
        {
            this.context = ctx;
            StartThread();
        }

        // ============================================================
        //  スレッド起動
        // ============================================================
        public void StartThread()
        {
            workerThread = new Thread(Main);
            workerThread.IsBackground = true;
            workerThread.Priority = ThreadPriority.AboveNormal;
            workerThread.Start();
        }

        // ============================================================
        //  メインスレッド処理
        // ============================================================
        private void Main()
        {
            try
            {
                Thread.CurrentThread.Name = "RmTimer";

                while (true)
                {
                    // 演奏開始待ち（低優先度）
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                    while (!GetStart())
                    {
                        if (unmount) return;
                        Thread.Sleep(100);
                    }

                    // 演奏開始 → 高優先度へ
                    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

                    // 外部同期（必要な場合）
                    context.WaitSync?.Invoke();

                    lock (lockObj) isRunning = true;

                    // ★ 44100Hz → 1フレームのミリ秒へ変換
                    //    44100Hz = 44100回/秒 → 1回あたり 0.0226757ms
                    double stepMs = 1.0 / (double)Frq;

                    // シーケンス初期化
                    SeqCounter = Def_SeqCounter;
                    processOverFlowCounter = 0;

                    // 使用するタイマーを選択&動作開始
                    ISpeedCount spdc = SelectTimer(musicInterruptTimer);
                    spdc.Start();

                    // 基準時刻（ミリ秒）
                    double o = spdc.ElapsedSec();

                    // ★ メイン演奏ループ（44100Hz）
                    while (true)
                    {
                        if (!GetStart())
                            break;
                        if (unmount) return;

                        // ★ CPU負荷軽減のため Sleep(0) → Sleep(1)
                        //    Sleep(0) は「即復帰」するため CPU が張り付く
                        //    Sleep(1) は 1ms 休むため CPU 使用率が大幅に下がる
                        Thread.Sleep(0);

                        //// 仮想送信モードなら待機
                        //if (context.isVirtualOnlySend)
                        //{
                        //    Thread.Sleep(100);
                        //    continue;
                        //}

                        // 現在時刻（ミリ秒）
                        double el1 = spdc.ElapsedSec();

                        // まだ1フレーム分経過していない
                        if (el1 - o < stepMs) continue;

                        // ★ 遅れすぎた場合の補正（処理落ち検出）
                        if (el1 - o >= stepMs * Frq)
                        {
                            processOverFlowCounter++;
                            o = el1 - stepMs; // 遅延補正
                        }

                        // ★ 遅れた分だけフレーム補完（最大500）
                        skipframe = Math.Max(Math.Min((int)((el1 - o) / stepMs), 500), 1);

                        // ★ フレーム補完ループ
                        for (int skipf = 0; skipf < skipframe; skipf++)
                        {
                            // 次のフレームへ進める
                            o += stepMs;

                            double lapPtr = spdc.ElapsedSec();

                            // 割り込み要求があればスキップ
                            FrameFlowControl flow = OneFrameProcMain();
                            switch (flow)
                            {
                                case FrameFlowControl.Break:
                                    break;

                                case FrameFlowControl.Continue:
                                    continue;

                                case FrameFlowControl.FatalExit:
                                    return;

                                case FrameFlowControl.Normal:
                                    // SendFrameData() へ進む
                                    break;
                            }

                            // デバッグ用：処理時間計測
                            process1_Lap = spdc.ElapsedSec() - lapPtr;
                            lapPtr = spdc.ElapsedSec();

                            // ★ データ送信（音源へ）
                            context.SendFrameData();

                            process2_Lap = spdc.ElapsedSec() - lapPtr;

                            if (reqResetSync) break;
                        }

                        // モードなし → 演奏終了
                        if (context.GetCurrentMode() == SendMode.none)
                        {
                            break;
                        }

                        // 同期リセット要求
                        if (reqResetSync)
                        {
                            reqResetSync = false;
                            o = spdc.ElapsedSec();
                        }
                    }

                    // 演奏停止データ送信
                    if (context.SendStopFrame() == -1) return;

                    lock (lockObj)
                    {
                        isRunning = false;
                        Start = false;
                    }

                    //// 音源側へ停止通知
                    //context.RequestStopAtEmuChipSender();
                    //context.RequestStopAtRealChipSender();
                }
            }
            catch (Exception ex)
            {
                Log.ForcedWrite(ex);
                lock (lockObj)
                {
                    isRunning = false;
                    Start = false;
                }
            }
            finally
            {
                procExit = true;
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
        }

        // ============================================================
        //  1フレーム処理
        // ============================================================
        public enum FrameFlowControl
        {
            Break,      // 0: 演奏終了
            Continue,   // 1: スキップ
            FatalExit,  // 2: スレッド終了
            Normal      // 3: 正常処理
        }

        private FrameFlowControl OneFrameProcMain()
        {
            if (context.IsInterrupted())
            {
                return FrameFlowControl.Continue;
            }

            // シーケンス進行速度の調整
            SeqSpeed += SeqSpeedDelta;

            // StepCounter がある場合の補正
            if (SeqSpeedDelta == 0 && context.GetStepCounter() > 0)
            {
                SeqSpeed++;
                int step = context.GetStepCounter();
                context.SetStepCounter(step - 1);
            }

            // シーケンス進行（1.0を超えたら次のステップへ）
            while (SeqSpeed >= 1.0)
            {
                SeqCounter++;
                SeqSpeed -= 1.0;
            }

            if (SeqCounter < 0) return FrameFlowControl.Continue;

            // ★ 演奏ロジックは handler に委譲
            return context.Handler.OnFrame(SeqCounter);
        }

        // ============================================================
        //  タイマー選択
        // ============================================================
        private ISpeedCount SelectTimer(MusicInterruptTimer musicInterruptTimer)
        {
            ISpeedCount spdc;
            switch (musicInterruptTimer)
            {
                case MusicInterruptTimer.StopWatch:
                default:
                    spdc = new TimeCountByStopWatch();
                    break;
                case MusicInterruptTimer.DateTime:
                    spdc = new TimeCountByDateTime();
                    break;
                case MusicInterruptTimer.QueryPerformanceCounter:
                    spdc = new TimeCountByPerformanceCounter();
                    break;
            }

            return spdc;
        }

        // ============================================================
        //  Start / Stop / Dispose
        // ============================================================
        public void RequestStart()
        {
            lock (lockObj)
            {
                Start = true;
            }
        }

        public void RequestStop()
        {
            lock (lockObj)
            {
                Start = false;
            }
        }

        public void RequestDispose()
        {
            lock (lockObj)
            {
                unmount = true;     // メインループを抜ける
                Start = false;      // 待機ループも抜ける
            }
        }

        protected bool GetStart()
        {
            lock (lockObj)
            {
                return Start;
            }
        }

        public bool IsRunning()
        {
            lock (lockObj)
            {
                return isRunning;
            }
        }

        // ============================================================
        //  Dispose
        // ============================================================
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            unmount = true;
            Start = false;

            if (workerThread != null && workerThread.IsAlive)
            {
                workerThread.Join();
            }

            GC.SuppressFinalize(this);
        }

        ~RmTimer()
        {
            Dispose();
        }


    }
}
