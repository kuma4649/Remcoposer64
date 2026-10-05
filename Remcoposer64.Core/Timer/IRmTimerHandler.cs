using System;
using System.Collections.Generic;
using System.Text;
using static Remcoposer64.Core.Timer.RmTimer;

namespace Remcoposer64.Core.Timer
{
    public interface IRmTimerHandler
    {
        FrameFlowControl OnFrame(long seqCounter);
    }
}
