
using Remcoposer64.Common;
using Remcoposer64.Core.Timer;
using NAudio.Midi;

namespace Remcoposer64.Core.Player
{
    public interface IPlayerChannel
    {
        /// <summary>
        /// チャンネルの初期化処理（カーソル準備など）
        /// </summary>
        void Initialize(Setting.midiOutInfo[] midiOut, MidiOut[] midiOutDevice, Setting.midiInInfo[] midiIn, MidiIn[] midiInDevice);

        /// <summary>
        /// 現在時刻に応じてイベントを発火する
        /// </summary>
        void ProcessFrame(long seqCounter);
        void UpdateTimer();
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
