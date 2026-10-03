using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDITieCache
    {
        public LinkedListNode<MIDIEvent> Node = null;
        public int GT = -1;

        private MIDITieCache[] CloneTieWork(MIDITieCache[] src)
        {
            var dst = new MIDITieCache[128];
            for (int i = 0; i < 128; i++)
            {
                dst[i] = new MIDITieCache
                {
                    Node = src[i].Node,
                    GT = src[i].GT
                };
            }
            return dst;
        }
    }
}
