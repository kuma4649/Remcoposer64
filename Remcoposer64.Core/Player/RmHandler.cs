using Remcoposer64.Common;
using Remcoposer64.Core.Timer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.Core.Player
{
    public class RmHandler : IRmTimerHandler
    {
        private readonly RmTimerContext _context;
        private readonly Setting.MidiOut _midiOut;
        private readonly Setting.MidiIn _midiIn;
        private readonly List<IPlayerChannel> _channels;

        public RmHandler(RmTimerContext context, Setting.MidiOut midiOut, Setting.MidiIn midiIn, List<IPlayerChannel> channels)
        {
            _context = context;
            _midiOut = midiOut;
            _midiIn = midiIn;
            _channels = channels;
        }

        /// <summary>
        /// 1frameごとの処理を行う
        /// </summary>
        /// <param name="seqCounter">44100Hz(デフォルト)のカウンター</param>
        /// <returns></returns>
        public RmTimer.FrameFlowControl OnFrame(long seqCounter)
        {
            foreach (var channel in _channels)
            {
                if (channel.IsFinished) continue;

                channel.UpdateTimer();
                while (channel.FireFrameProcess>0)
                {
                    channel.ProcessFrame(seqCounter);
                    channel.FireFrameProcess--;
                }
            }

            return RmTimer.FrameFlowControl.Normal;
        }

    }

}
