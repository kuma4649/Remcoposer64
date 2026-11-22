
using NAudio.Midi;
using Remcoposer64.Common;

namespace Remcoposer64.Core.Player
{
    public interface IPlayerChannel
    {
        /// <summary>
        /// チャンネルの初期化処理（カーソル準備など）
        /// </summary>
        void Initialize(Setting setting, Setting.midiOutInfo[] midiOut, ThrottledMidiOut[] midiOutDevice, Setting.midiInInfo[] midiIn, MidiIn[] midiInDevice);

        /// <summary>
        /// ベースとなる周波数の割り込みを基に自身のテンポから
        /// ProcessFrameの処理回数となるFireFrameProcessを求める
        /// </summary>
        void UpdateTimer();

        /// <summary>
        /// 現在時刻に応じてイベントを発火する
        /// </summary>
        void ProcessFrame(long seqCounter);

        /// <summary>
        /// 演奏停止指示を受けた時に実施される処理
        /// </summary>
        void Stop();

        /// <summary>
        /// このチャンネルの演奏が終了したかどうか
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// 処理を実施するべきフレーム数
        /// </summary>
        int FireFrameProcess { get; set; }
    }
}
