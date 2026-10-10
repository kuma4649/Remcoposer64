using NAudio.Midi;
using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Remcoposer64.Core
{
    public class ImportRcp
    {
        //ファイル名保存用
        private string fn = "";

        //設定参照用
        private Setting setting = null;
        private byte[] bs = null;
        private int ptr = 0;
        private MIDIProject prj;
        private List<MIDIRythm> rtm;
        private MIDITrack[] trk;
        private MIDIPart[] prt;


        private bool IsG36 = false;
        private int trkLen = 0;
        private int rcpVer = 0;
        private Encoding enc;

        private int trkTick = 0;
        private int meaTick = 0;
        private int meaInd = 0;
        private bool endTrack = false;
        private Dictionary<byte, byte> taiDic;
        private int stDevNum = 0;
        private int pt = 0;
        private int skipPtr = 4;


        //コンストラクタ
        public ImportRcp(string fn, Setting setting)
        {
            this.fn = fn;
            this.setting = setting;
        }

        public MIDIProject Load()
        {
            prj = new MIDIProject(setting);
            bs = null;

            #region ファイル読込（オンメモリバッファ）

            using (FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                bs = new byte[fs.Length];
                fs.ReadExactly(bs);
                fs.Close();
            }

            #endregion

            Header();
            Rythm();
            UserEx();
            TrackData();

            LinkedListNode<MIDITrack> nTrk = prj.Track.Last;
            while (nTrk!=null)
            {
                if (nTrk.Value.Part.First.Value.Event.Count < 1)
                {
                    prj.Track.RemoveLast();
                    nTrk = prj.Track.Last;
                }
                else
                    break;
            }

            return prj;
        }

        private void Header()
        {
            string str = "";

            enc = Common.Common.GetCode(bs, ptr, 32);
            if (enc == null) enc = Encoding.UTF8;
            str = enc.GetString(bs, ptr, 32);
            if (str != "RCM-PC98V2.0(C)COME ON MUSIC\r\n\0\0")
            {
                if (str != "COME ON MUSIC RECOMPOSER RCP3.0\0")
                {
                    throw new Exception("RCPファイルではないようです");
                }
                else
                {
                    IsG36 = true;
                }
            }
            ptr += 32;

            //タイトル
            enc = Common.Common.GetCode(bs, ptr, 64);
            if (enc == null) enc = Encoding.UTF8;
            str = (enc.GetString(bs, ptr, 64)).Replace("\0", "");
            ptr += 64;
            prj.Information.Title = str;

            if (IsG36)
            {
                Header_G36();
            }
            else
            {
                Header_RCP();
            }
        }

        private void Header_G36()
        {
            string str = "";

            // dummy skip
            ptr += 64;

            // Memo (360byte)
            enc = Common.Common.GetCode(bs, ptr, 360);
            if (enc == null) enc = Encoding.UTF8;
            str = (enc.GetString(bs, ptr, 360)).Replace("\0", "");
            prj.Information.Memo = str;
            ptr += 360;

            // トラック数
            trkLen = bs[ptr++];
            if (trkLen != 18 && trkLen != 36) trkLen = 18;

            // dummy skip
            ptr++;

            // Timebase
            prj.Information.TimeBase = bs[ptr] + (bs[ptr + 1] * 0x100);
            ptr += 2;

            // Tempo
            prj.Information.Tempo = bs[ptr++];
            if (prj.Information.Tempo < 8 || prj.Information.Tempo > 250)
            {
                prj.Information.Tempo = 120;
            }

            // dummy skip
            ptr++;

            // 拍子
            prj.Information.BeatDen = bs[ptr++];
            prj.Information.BeatMol = bs[ptr++];

            // Key
            prj.Information.Key = bs[ptr++];

            // Play BIAS
            prj.Information.PlayBIAS = bs[ptr++];

            // dummy skip
            ptr += 6;
            ptr += 16;
            ptr += 112;

            // .GSD
            enc = Common.Common.GetCode(bs, ptr, 12);
            if (enc == null) enc = Encoding.UTF8;
            prj.Information.ControlFileGSD =
                enc.GetString(bs, ptr, 12).Replace("\0", "");
            ptr += 12;
            ptr += 4;

            // .GSD2
            enc = Common.Common.GetCode(bs, ptr, 12);
            if (enc == null) enc = Encoding.UTF8;
            prj.Information.ControlFileGSD2 =
                enc.GetString(bs, ptr, 12).Replace("\0", "");
            ptr += 12;
            ptr += 4;

            // .CM6
            enc = Common.Common.GetCode(bs, ptr, 12);
            if (enc == null) enc = Encoding.UTF8;
            prj.Information.ControlFileCM6 =
                enc.GetString(bs, ptr, 12).Replace("\0", "");
            ptr += 12;
            ptr += 4;

            // dummy skip
            ptr += 80;
        }

        private void Header_RCP()
        {
            string str = "";

            // Memo (12行 × 28byte)
            str = "";
            for (int i = 0; i < 12; i++)
            {
                str += enc
                    .GetString(bs, ptr + i * 28, 28)
                    .Replace("\0", "") + "\r\n";
            }
            ptr += 336;
            prj.Information.Memo = str;

            // dummy skip
            ptr += 16;

            // Timebase 下位
            prj.Information.TimeBase = bs[ptr++];

            // Tempo
            prj.Information.Tempo = bs[ptr++];

            // 拍子
            prj.Information.BeatDen = bs[ptr++];
            prj.Information.BeatMol = bs[ptr++];

            // Key
            prj.Information.Key = bs[ptr++];

            // Play BIAS
            prj.Information.PlayBIAS = bs[ptr++];

            // .CM6
            enc = Common.Common.GetCode(bs, ptr, 12);
            if (enc == null) enc = Encoding.UTF8;
            prj.Information.ControlFileCM6 =
                enc.GetString(bs, ptr, 12).Replace("\0", "");
            ptr += 12;
            ptr += 4;

            // .GSD
            enc = Common.Common.GetCode(bs, ptr, 12);
            if (enc == null) enc = Encoding.UTF8;
            prj.Information.ControlFileGSD =
                enc.GetString(bs, ptr, 12).Replace("\0", "");
            ptr += 12;
            ptr += 4;

            // トラック数
            trkLen = bs[ptr++];
            switch (trkLen)
            {
                case 0:
                    trkLen = 36;
                    rcpVer = 0;
                    break;
                case 18:
                    rcpVer = 1;
                    break;
                case 36:
                    rcpVer = 2;
                    break;
            }

            // Timebase 上位
            prj.Information.TimeBase += bs[ptr++] * 0x100;

            // TONENAME.TB? 無視

            // リズム定義部まで移動
            ptr = 0x206;
        }

        private void Rythm()
        {
            rtm = new List<MIDIRythm>();
            int n = IsG36 ? 128 : 32;

            for (int i = 0; i < n; i++)
            {
                MIDIRythm r = new MIDIRythm();
                enc = Common.Common.GetCode(bs, ptr, 14);
                if (enc == null) enc = Encoding.UTF8;
                r.Name = enc.GetString(bs, ptr, 14).Replace("\0", "");
                ptr += 14;
                r.Key = bs[ptr++];
                r.Gt = bs[ptr++];
                rtm.Add(r);
            }

            prj.Rythm = rtm;
        }

        private void UserEx()
        {
            prj.UserExclusive = new List<MIDIUserExclusive>();

            for (int i = 0; i < 8; i++)
            {
                MIDIUserExclusive ux = new MIDIUserExclusive();
                enc = Common.Common.GetCode(bs, ptr, 24);
                if (enc == null) enc = Encoding.UTF8;
                ux.Name = enc.GetString(bs, ptr, 24).Replace("\0", "");
                ux.Memo = ux.Name;
                ptr += 24;

                ux.Exclusive = new byte[25];
                ux.Exclusive[0] = 0xF0;

                for (int j = 1; j < 25; j++)
                {
                    ux.Exclusive[j] = bs[ptr++];
                }

                prj.UserExclusive.Add(ux);
            }
        }

        private void TrackData()
        {
            initTrkPrt(); //トラックと小節を準備する

            for (int i = 0; i < trkLen; i++)
            {
                if (ptr >= bs.Length) 
                    continue;

                int bsptr = ptr;
                int trkSize = bs[ptr++] * 0x100 + bs[ptr++];

                //Size dummy(?) skip
                if (IsG36) ptr += 2;

                int trkNumber = 0;

                if (IsG36)
                {
                    trkNumber = i;
                    ptr++;
                }
                else
                {
                    trkNumber = bs[ptr++] - 1;
                    if (trkNumber < 0)
                        trkNumber = i;
                }

                // リズムモード
                trk[trkNumber].RythmMode = (bs[ptr++] == 0x80);

                // 出力デバイス・チャンネル
                int ch = bs[ptr++];

                if (ch != 255)
                {
                    int n = (stDevNum + (ch / 16)) % setting.midiOut.lstMidiOutInfo[setting.midiOut.CurrentDev].Length;

                    trk[trkNumber].OutDevice = n;
                    trk[trkNumber].OutChannel = ch % 16;
                }
                else
                {
                    trk[trkNumber].OutDevice = 0;
                    trk[trkNumber].OutChannel = null;
                }

                // 入力デバイスは常に None
                trk[trkNumber].InDevice = 0;
                trk[trkNumber].InChannel = null;

                // Key
                trk[trkNumber].Key = bs[ptr++];
                if ((trk[trkNumber].Key & 0x80) == 0x80)
                {
                    trk[trkNumber].Key = null;
                }
                else
                {
                    trk[trkNumber].Key =
                        (trk[trkNumber].Key > 63)
                        ? trk[trkNumber].Key - 128
                        : trk[trkNumber].Key;
                }

                // St
                trk[trkNumber].St = bs[ptr++];
                if (rcpVer > 0)
                {
                    trk[trkNumber].St =
                        (trk[trkNumber].St > 127)
                        ? trk[trkNumber].St - 256
                        : trk[trkNumber].St;
                }

                // Mute
                trk[trkNumber].Mute = (bs[ptr++] == 1);

                // トラック名
                enc = Common.Common.GetCode(bs, ptr, 36);
                if (enc == null) enc = Encoding.UTF8;
                trk[trkNumber].Name =
                    enc.GetString(bs, ptr, 36).Replace("\0", "");
                ptr += 36;

                // イベント読み込み準備
                trkTick = 0;
                taiDic = new Dictionary<byte, byte>();
                meaTick = 0;
                meaInd = 0;

                // イベント読み込み
                musData(trk[trkNumber], bs);

                // SameMeasure のリンク
                extractSame(trk[trkNumber]);
            }
        }

        private void initTrkPrt()
        {
            trk = new MIDITrack[trkLen];
            prt = new MIDIPart[trkLen];

            for (int i = 0; i < trkLen; i++)
            {
                // トラック作成
                trk[i] = new MIDITrack();

                // プロジェクトへ登録
                prj.insertTrack(i, trk[i]);   // ← Remcoposer64 にこのメソッドがある前提

                trk[i].Number = i;
                // パート初期化
                trk[i].clearAllPartMemory();  // ← Remcoposer64 に存在するならそのまま

                prt[i] = new MIDIPart();
                prt[i].Name = $"Track {i + 1} Part";

                // トラックにパートを追加
                trk[i].insertPart(0, prt[i]);
            }
        }

        private void musData(MIDITrack trkn, byte[] ebs)
        {
            endTrack = false;
            pt = ptr;

            LinkedListNode<MIDIPart> prt = trkn.Part.First;

            while (!endTrack)
            {
                LinkedListNode<MIDIEvent> pEvt = prt.Value.Event.Last;
                int[] pk = null;

                if (!IsG36)
                {
                    pk = new int[4] { ebs[pt], ebs[pt + 1], ebs[pt + 2], ebs[pt + 3] };
                }
                else
                {
                    pk = new int[4] { ebs[pt], ebs[pt + 2] + ebs[pt + 3] * 0x100, ebs[pt + 4] + ebs[pt + 5] * 0x100, ebs[pt + 1] };
                    skipPtr = 6;
                }

                if (pk[0] < 0x80)
                {
                    onpu(trkn, pk, pEvt);
                }
                else
                {
                    command(trkn, pk, ebs, pEvt);
                }
            }

            ptr = pt;
        }

        /// <summary>
        /// セームメジャー　イベントの割だし
        /// </summary>
        private void extractSame(MIDITrack trk)
        {
            // Part は 1 つだけ
            var part = trk.Part.First;

            LinkedListNode<MIDIEvent> nevt = part.Value.Event.First;

            while (nevt != null)
            {
                MIDIEvent evt = nevt.Value;

                // SameMeasure 判定
                if (evt.Type == MIDIEventType.MetaSequencerSpecific &&
                    evt.MIDIMessage[0] == (byte)MIDISpEventType.SameMeasure)
                {
                    int ofsMea = 0;

                    // --- オフセット計算 ---
                    if (IsG36)
                    {
                        // G36 は 3バイト構造
                        ofsMea = evt.MIDIMessageLst[0][0]
                               + evt.MIDIMessageLst[0][2] * 0x100;
                    }
                    else
                    {
                        // 通常 RCP
                        ofsMea = evt.MIDIMessageLst[0][0]
                               + (evt.MIDIMessageLst[0][1] & 3) * 0x100;
                    }

                    int Mea = 0;
                    int MeaS = 0;

                    LinkedListNode<MIDIEvent> nmEvt = part.Value.Event.First;
                    MIDIEvent mEvt = nmEvt.Value;

                    // --- オフセットが 0 の場合はリンク不要 ---
                    if (ofsMea != 0)
                    {
                        while (mEvt != null)
                        {
                            MeaS = 0;

                            if (mEvt.Type == MIDIEventType.MetaSequencerSpecific)
                            {
                                LinkedListNode< MIDIEvent> nnEvt = nmEvt.Next;
                                MIDIEvent nEvt = nnEvt?.Value;
                                int s = 0;

                                // 次が SameMeasure なら小節進めない
                                if (nEvt != null &&
                                    nEvt.Type == MIDIEventType.MetaSequencerSpecific &&
                                    nEvt.MIDIMessage[0] == (byte)MIDISpEventType.SameMeasure)
                                {
                                    s = 0;
                                }
                                else
                                {
                                    s = 1;
                                }

                                // MeasureEnd の場合
                                if (mEvt.MIDIMessage[0] == (byte)MIDISpEventType.MeasureEnd)
                                {
                                    MeaS = s;
                                }

                                // SameMeasure の場合
                                if (mEvt.MIDIMessage[0] == (byte)MIDISpEventType.SameMeasure)
                                {
                                    Mea++;
                                    if (ofsMea == Mea) break;
                                    MeaS = s;
                                }
                            }

                            nmEvt = nmEvt.Next;
                            mEvt = nmEvt?.Value;
                            Mea += MeaS;

                            if (ofsMea == Mea) break;
                        }
                    }

                    // --- リンク設定 ---
                    if (mEvt != null)
                    {
                        ((MIDISameMeasEvent)evt).RepeatMeas = mEvt.Meas;
                    }
                }

                nevt = nevt.Next;
            }
        }

        private void onpu(MIDITrack trkn, int[] pk, LinkedListNode<MIDIEvent> pEvt)
        {
            // pk[0] = Note
            // pk[1] = Step
            // pk[2] = Gate
            // pk[3] = Velocity

            // Note ON メッセージ
            byte[] msg = new byte[3]
            {
                0x90,           // NoteOn
                (byte)pk[0],    // Note number
                (byte)pk[3]     // Velocity
            };

            // イベント挿入
            trkn.Part.First.Value.insertEventNode(
                pEvt,
                pk[1],              // Step
                MIDIEventType.NoteON,
                msg,
                pk[2]               // GateTime
            );

            // 次のイベントへ進める
            pt += skipPtr;
        }

        private void command(MIDITrack trkn, int[] pk, byte[] ebs, LinkedListNode<MIDIEvent> pEvt)
        {
            List<byte> ex = null;

            switch (pk[0])
            {
                case 0x98: // CH Exclusive
                    pt += skipPtr;
                    ex = new List<byte>();
                    ex.Add(0xF0);

                    while (ebs[pt] == 0xf7)
                    {
                        if (IsG36)
                        {
                            pt++;
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                        }
                        else
                        {
                            pt += 2;
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                        }
                    }

                    ex.Add((byte)(pk[2] & 0xff));
                    ex.Add((byte)(pk[3] & 0xff));

                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        pk[1],
                        MIDISpEventType.ChExclusive,
                        new byte[1][] { ex.ToArray() }
                    );
                    break;

                case 0x90: // User Exclusive 1
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0xc0: // DX7 Function
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                case 0xdc:
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xdd:
                case 0xde:
                case 0xdf:
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        pk[1],
                        (MIDISpEventType)pk[0],
                        new byte[1][] { new byte[2] { (byte)pk[2], (byte)pk[3] } }
                    );
                    pt += skipPtr;
                    break;

                case 0xe2: // Bank & Program
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        pk[1],
                        MIDISpEventType.BankProgram,
                        new byte[2][] {
                    new byte[2]{ (byte)MIDIEventType.ProgramChange, (byte)pk[2] },
                    new byte[3]{ (byte)MIDIEventType.ControlChange, 0x00, (byte)pk[3] }
                        }
                    );
                    pt += skipPtr;
                    break;

                case 0xe5: // KeyScan
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        pk[1],
                        MIDISpEventType.KeyScan,
                        new byte[1][] { new byte[1] { (byte)pk[2] } }
                    );
                    pt += skipPtr;
                    break;

                case 0xe6: // MIDI CH
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        pk[1],
                        MIDISpEventType.MIDICh,
                        new byte[1][] { new byte[2] { (byte)pk[2], (byte)pk[3] } }
                    );
                    pt += skipPtr;
                    break;

                case 0xe7: // TempoChange
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        pk[1],
                        MIDISpEventType.TempoChange,
                        new byte[1][] { new byte[2] { (byte)pk[2], (byte)pk[3] } }
                    );
                    pt += skipPtr;
                    break;

                case 0xea: // Channel AfterTouch
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        pk[1],
                        MIDIEventType.ChannelAfterTouch,
                        new byte[2] { (byte)MIDIEventType.ChannelAfterTouch, (byte)pk[2] },
                        0
                    );
                    pt += skipPtr;
                    break;

                case 0xeb: // ControlChange
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        pk[1],
                        MIDIEventType.ControlChange,
                        new byte[3] { (byte)MIDIEventType.ControlChange, (byte)pk[2], (byte)pk[3] },
                        0
                    );
                    pt += skipPtr;
                    break;

                case 0xec: // Program Change
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        pk[1],
                        MIDIEventType.ProgramChange,
                        new byte[2] { (byte)MIDIEventType.ProgramChange, (byte)pk[2] },
                        0
                    );
                    pt += skipPtr;
                    break;

                case 0xed: // Key AfterTouch
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        pk[1],
                        MIDIEventType.KeyAfterTouch,
                        new byte[3] { (byte)MIDIEventType.KeyAfterTouch, (byte)pk[2], (byte)pk[3] },
                        0
                    );
                    pt += skipPtr;
                    break;

                case 0xee: // Pitch Bend
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        pk[1],
                        MIDIEventType.PitchBend,
                        new byte[3] { (byte)MIDIEventType.PitchBend, (byte)pk[2], (byte)pk[3] },
                        0
                    );
                    pt += skipPtr;
                    break;

                case 0xf5: // Key Change
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        0,
                        MIDISpEventType.KeyChange,
                        new byte[1][] { new byte[1] { (byte)pk[0] } }
                    );
                    pt += skipPtr;
                    break;

                case 0xf6: // Comment
                    pt += skipPtr;
                    ex = new List<byte>();

                    if (IsG36)
                    {
                        ex.Add((byte)(pk[3] & 0xff));
                        ex.Add((byte)(pk[1] & 0xff));
                        ex.Add((byte)(pk[1] / 0x100));
                        ex.Add((byte)(pk[2] & 0xff));
                        ex.Add((byte)(pk[2] / 0x100));

                        while (ebs[pt] == 0xf7)
                        {
                            pt++;
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                        }

                        trkn.Part.First.Value.insertEventNode(
                            pEvt,
                            0,
                            MIDIEventType.Memo,
                            ex.ToArray()
                        );
                    }
                    else
                    {
                        ex.Add((byte)pk[2]);
                        ex.Add((byte)pk[3]);

                        while (ebs[pt] == 0xf7)
                        {
                            pt += 2;
                            ex.Add(ebs[pt++]);
                            ex.Add(ebs[pt++]);
                        }

                        trkn.Part.First.Value.insertEventNode(
                            pEvt,
                            pk[1],
                            MIDIEventType.Memo,
                            ex.ToArray() 
                        );
                    }
                    break;

                case 0xf8: // Loop End
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        0,
                        MIDISpEventType.LoopEnd,
                        new byte[1][] { new byte[1] { (byte)pk[1] } }
                    );
                    pt += skipPtr;
                    break;

                case 0xf9: // Loop Start
                    trkn.Part.First.Value.insertSpEventNode(
                        pEvt,
                        0,
                        MIDISpEventType.LoopStart,
                        new byte[1][] { new byte[1] { 0 } }
                    );
                    pt += skipPtr;
                    break;

                case 0xFC: // Same Measure
                    if (IsG36)
                    {
                        trkn.Part.First.Value.insertEventNode(
                            pEvt,
                            0,
                            MIDIEventType.SameMeas,
                            new byte[3] {
                                (byte)pk[1],
                                (byte)(pk[3] & 0xff),
                                (byte)(pk[3] / 0x100)
                            }
                        );
                    }
                    else
                    {
                        trkn.Part.First.Value.insertEventNode(
                            pEvt,
                            0,
                            MIDIEventType.SameMeas,
                            new byte[3] {
                                (byte)pk[1],
                                (byte)pk[2],
                                (byte)pk[3]
                            }
                        );
                    }
                    pt += skipPtr;
                    break;

                case 0xFD: // Measure End
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        0,
                        MIDIEventType.BarLine,
                        null
                    );
                    pt += skipPtr;
                    break;

                case 0xFE: // End of Track
                    trkn.Part.First.Value.insertEventNode(
                        pEvt,
                        0,
                        MIDIEventType.MetaEndOfTrack,
                        null
                    );
                    pt += skipPtr;
                    endTrack = true;
                    break;

                default:
                    pt += skipPtr;
                    break;
            }
        }


    }
}
