using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.Test
{
    public class SMFParser
    {
        public class MidiEventEx
        {
            public int Track;
            public long AbsTick;
            public byte[] Data; // ステータス含む生MIDIメッセージ
        }

        public static List<MidiEventEx> ParseSmf(string path)
        {
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);

            // --- MThd ---
            if (new string(br.ReadChars(4)) != "MThd")
                throw new Exception("MThd not found");

            int hdrLen = ReadBE32(br);
            int format = ReadBE16(br);
            int trackCount = ReadBE16(br);
            int division = ReadBE16(br); // PPQ

            if (hdrLen > 6)
                br.ReadBytes(hdrLen - 6);

            var trackBuffers = new List<List<MidiEventEx>>();

            // --- 各Trackを読み込む ---
            for (int ti = 0; ti < trackCount; ti++)
            {
                if (new string(br.ReadChars(4)) != "MTrk")
                    throw new Exception("MTrk not found");

                int trkLen = ReadBE32(br);
                long endPos = fs.Position + trkLen;

                long absTick = 0;
                int runningStatus = 0;

                var list = new List<MidiEventEx>();

                while (fs.Position < endPos)
                {
                    long delta = ReadVarLen(br);
                    absTick += delta;

                    byte status = br.ReadByte();

                    if (status < 0x80)
                    {
                        // Running Status
                        if (runningStatus == 0)
                            throw new Exception("Running status error");

                        byte data1 = status;
                        byte[] msg = ReadChannelMessage(br, (byte)runningStatus, data1);

                        list.Add(new MidiEventEx { Track = ti, AbsTick = absTick, Data = msg });
                    }
                    else if (status == 0xFF)
                    {
                        runningStatus = 0; // ★ Running Status リセット
                        // Meta Event
                        byte metaType = br.ReadByte();
                        long len = ReadVarLen(br);
                        byte[] data = br.ReadBytes((int)len);

                        var buf = new byte[2 + len];
                        buf[0] = 0xFF;
                        buf[1] = metaType;
                        Array.Copy(data, 0, buf, 2, len);

                        list.Add(new MidiEventEx { Track = ti, AbsTick = absTick, Data = buf });

                        if (metaType == 0x2F) // End of Track
                            break;
                    }
                    else if (status == 0xF0 || status == 0xF7)
                    {
                        runningStatus = 0; // ★ Running Status リセット
                        // SysEx
                        long len = ReadVarLen(br);
                        byte[] body = br.ReadBytes((int)len);

                        var buf = new byte[1 + len];
                        buf[0] = status;
                        Array.Copy(body, 0, buf, 1, len);

                        list.Add(new MidiEventEx { Track = ti, AbsTick = absTick, Data = buf });
                    }
                    else
                    {
                        // Channel Message
                        runningStatus = status;
                        byte[] msg = ReadChannelMessage(br, status);

                        list.Add(new MidiEventEx { Track = ti, AbsTick = absTick, Data = msg });
                    }
                }

                trackBuffers.Add(list);
            }

            // --- Trackバッファをマージ ---
            List<MidiEventEx> ret = MergeTracks(trackBuffers);
            //long oldTick = 0;
            //foreach (var ev in ret)
            //{
            //    if(ev.AbsTick < oldTick)
            //        throw new Exception("Tick order error");
            //    oldTick = ev.AbsTick;
            //}
            return ret;
        }

        private static List<MidiEventEx> MergeTracks(List<List<MidiEventEx>> tracks)
        {
            var result = new List<MidiEventEx>();

            int trackCount = tracks.Count;
            int[] index = new int[trackCount];

            while (true)
            {
                long minTick = long.MaxValue;
                int minTrack = -1;

                for (int t = 0; t < trackCount; t++)
                {
                    if (index[t] >= tracks[t].Count)
                        continue;

                    var ev = tracks[t][index[t]];

                    if (ev.AbsTick < minTick)
                    {
                        minTick = ev.AbsTick;
                        minTrack = t;
                    }
                    else if (ev.AbsTick == minTick)
                    {
                        // 同Tickならトラック番号の若い方を優先
                        if (t < minTrack)
                            minTrack = t;
                    }
                }

                if (minTrack == -1)
                    break;

                result.Add(tracks[minTrack][index[minTrack]]);
                index[minTrack]++;
            }

            return result;
        }

        private static int ReadBE16(BinaryReader br)
        {
            byte b1 = br.ReadByte();
            byte b2 = br.ReadByte();
            return (b1 << 8) | b2;
        }

        private static int ReadBE32(BinaryReader br)
        {
            byte b1 = br.ReadByte();
            byte b2 = br.ReadByte();
            byte b3 = br.ReadByte();
            byte b4 = br.ReadByte();
            return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
        }

        private static long ReadVarLen(BinaryReader br)
        {
            long value = 0;
            while (true)
            {
                byte b = br.ReadByte();
                value = (long)(value << 7) | ((long)(b & 0x7F));
                if ((b & 0x80) == 0) break;
            }
            return value;
        }

        private static byte[] ReadChannelMessage(BinaryReader br, byte status, byte? firstData = null)
        {
            int type = status & 0xF0;
            byte ch = (byte)(status & 0x0F);

            byte d1, d2 = 0;

            if (firstData.HasValue)
            {
                d1 = firstData.Value;
            }
            else
            {
                d1 = br.ReadByte();
            }

            bool hasD2 =
                type != 0xC0 && // Program Change
                type != 0xD0;   // Channel Aftertouch

            if (hasD2)
                d2 = br.ReadByte();

            if (hasD2)
                return new[] { status, d1, d2 };
            else
                return new[] { status, d1 };
        }

    }
}
