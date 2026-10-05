namespace Remcoposer64.Core.Timer
{
    /// <summary>
    /// RmTimer が 44100Hz のシーケンス進行を行う際に必要となる
    /// 状態アクセスと操作をまとめたコンテキスト。
    /// 
    /// - RmTimer はこのコンテキストを通して状態を参照する
    /// - 演奏ロジックは IRmTimerHandler に委譲される
    /// - 親コントローラの実装詳細は全てデリゲートで隠蔽される
    /// </summary>
    public class RmTimerContext
    {
        /// <summary>
        /// 1フレーム分の演奏処理を行うハンドラ。
        /// RmTimer は SeqCounter を渡してこのハンドラを呼び出す。
        /// </summary>
        public IRmTimerHandler Handler { get; set; }

        // ------------------------------------------------------------
        // シーケンス進行に必要な状態アクセス
        // ------------------------------------------------------------

        /// <summary>
        /// StepCounter を取得するためのデリゲート。
        /// </summary>
        public Func<int> GetStepCounter { get; set; }

        /// <summary>
        /// StepCounter を更新するためのデリゲート。
        /// </summary>
        public Action<int> SetStepCounter { get; set; }

        /// <summary>
        /// 割り込み状態を取得するためのデリゲート。
        /// true の場合、現在のフレーム処理をスキップする。
        /// </summary>
        public Func<bool> IsInterrupted { get; set; }

        /// <summary>
        /// 現在の演奏モードを取得するためのデリゲート。
        /// SendMode.none の場合、演奏を終了する。
        /// </summary>
        public Func<SendMode> GetCurrentMode { get; set; }

        // ------------------------------------------------------------
        // 音源への送信処理
        // ------------------------------------------------------------

        /// <summary>
        /// 1フレーム分の音源データを送信するためのデリゲート。
        /// </summary>
        public Action SendFrameData { get; set; }

        /// <summary>
        /// 演奏停止時に音源へ停止データを送信するためのデリゲート。
        /// -1 を返す場合は致命的エラーとしてスレッド終了。
        /// </summary>
        public Func<int> SendStopFrame { get; set; }


        // ------------------------------------------------------------
        // その他
        // ------------------------------------------------------------

        /// <summary>
        /// 演奏開始前に外部システムと同期を取るためのデリゲート。
        /// 例：音源初期化、外部デバイスとの同期、MIDIデータ準備など。
        /// </summary>
        public Action WaitSync { get; set; }
    }
}
