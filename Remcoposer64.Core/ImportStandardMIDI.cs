using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.Core
{
    //参考　：　http://www2s.biglobe.ne.jp/~yyagi/material/smfspec.html#TrackData

    public class ImportStandardMIDI
    {
        //ファイル名保存用
        private string fn = "";
        //SMF読込設定参照用
        private Setting setting = null;
        //ランニングステータス展開用
        private byte Bcmd = 0;
        //ランニングステータス展開用
        private int Bch = 0;
        //既にタイトルを格納済みかどうかフラグ
        private bool titleSW = false;

        //コンストラクタ
        public ImportStandardMIDI(string fn, Setting setting)
        {
            this.fn = fn;
            this.setting = setting;
        }

        //パブリックメソッド

        //読み込み！
        public MIDIProject Load()
        {
            MIDIProject prj = new MIDIProject();
            MIDITrack[] trk = null;
            MIDIPart[] prtAry = null;
            int[] trkStep = null;
            byte[] bs = null;
            int trkPtr = 14;
            int trkDatLen = 0;
            int trkTick = 0;
            int stDevNum = 0;//このデバイスから順にトラック（或いはチャンネル）が振られる(暫定)

            #region ファイル読込（オンメモリバッファ）

            using (FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                bs = new byte[fs.Length];
                fs.ReadExactly(bs);
                fs.Close();
            }

            #endregion

            #region ヘッダーのチェック

            if (!(bs[0] == 0x4D && bs[1] == 0x54 && bs[2] == 0x68 && bs[3] == 0x64 &&
                bs[4] == 0x00 && bs[5] == 0x00 && bs[6] == 0x00 && bs[7] == 0x06))
            {
                //不正なヘッダーであることを検知しました。
                throw new Exceptions.RcFailedOpenSMFHeaderException();
            }

            int format = bs[8] * 256 + bs[9];
            int trkLen = bs[10] * 256 + bs[11];
            int delta = bs[12] * 256 + bs[13];

            if (format == 0 && trkLen > 1)
            {
                //不正なトラック数であることを検知しました。
                throw new Exceptions.RcFailedOpenSMFTrackNumberException();
            }

            if (format == 2)
            {
                //現在、Format2には対応していません。
                throw new Exceptions.RcFailedOpenSMFNotSupportSMF2Exception();
            }

            #endregion

            #region プロジェクトの初期設定

            prj.Information.FileName = Path.GetFileNameWithoutExtension(fn);
            prj.Information.Title = Common.cntNoTitle;
            prj.Information.Tempo = 120;
            prj.Information.TimeBase = delta;
            prj.Information.BeatDen = 4;
            prj.Information.BeatMol = 4;
            prj.clearAllTrackMemory();

            //トラックを準備
            if (format == 0)
            {
                trk = new MIDITrack[16];
                trkStep = new int[16];
                prtAry = new MIDIPart[16];
                for (int i = 0; i < 16; i++)
                {
                    trk[i] = new MIDITrack();
                    trk[i].Name = string.Format(Common.cntSMF0DefaultTrackName, i + 1);
                    trk[i].OutDeviceName = setting.MIDIOutDeviceList[stDevNum].DevName;
                    trk[i].OutDeviceNumber = setting.MIDIOutDeviceList[stDevNum].DevNumber;
                    trk[i].OutUserDeviceName = setting.MIDIOutDeviceList[stDevNum].UsrName;
                    trk[i].OutUserDeviceNumber = setting.MIDIOutDeviceList[stDevNum].UsrNumber;
                    trk[i].OutChannel = i;
                    trk[i].InDeviceName = Common.cntNullDevice;
                    trk[i].InDeviceNumber = null;
                    trk[i].InUserDeviceNumber = null;
                    trk[i].InUserDeviceName = Common.cntNullDevice;
                    trk[i].InChannel = null;
                    trkStep[i] = 0;
                    prj.insertTrack(i, trk[i]);
                    trk[i].clearAllPartMemory();
                    prtAry[i] = new MIDIPart();
                    prtAry[i].Name = string.Format(Common.cntSMF0DefaultPartName, i + 1);
                    trk[i].insertPart(0, prtAry[i]);
                    prtAry[i].insertEventNode(null, 0, MIDIEventType.NoteOff, new byte[3] { 0x80, 60, 0 }, 100);
                }
            }
            else
            {
                trk = new MIDITrack[trkLen];
                prtAry = new MIDIPart[trkLen];
                trkStep = new int[trkLen];
                for (int i = 0; i < trkLen; i++)
                {
                    trk[i] = new MIDITrack();
                    trk[i].Name = (i == 0)
                        ? Common.cntSMF1ConductorTrackName
                        : string.Format(Common.cntSMF1DefaultTrackName, i + 1);
                    trk[i].OutDeviceName = setting.MIDIOutDeviceList[stDevNum].DevName;
                    trk[i].OutDeviceNumber = setting.MIDIOutDeviceList[stDevNum].DevNumber;
                    trk[i].OutUserDeviceNumber = setting.MIDIOutDeviceList[stDevNum].UsrNumber;
                    trk[i].OutUserDeviceName = setting.MIDIOutDeviceList[stDevNum].UsrName;
                    trk[i].OutChannel = i % 16;
                    trk[i].InDeviceName = Common.cntNullDevice;
                    trk[i].InDeviceNumber = null;
                    trk[i].InUserDeviceNumber = null;
                    trk[i].InUserDeviceName = Common.cntNullDevice;
                    trk[i].InChannel = null;
                    trkStep[i] = 0;
                    prj.insertTrack(i, trk[i]);
                    trk[i].clearAllPartMemory();
                    prtAry[i] = new MIDIPart();
                    prtAry[i].Name = string.Format(Common.cntSMF1DefaultPartName, i + 1);
                    trk[i].insertPart(0, prtAry[i]);
                    prtAry[i].insertEventNode(null, 0, MIDIEventType.NoteOff, new byte[3] { 0x80, 60, 0 }, 100);
                }
            }

            #endregion

            #region SMF解析メインループ

            for (int trkNum = 0; trkNum < trkLen; trkNum++)
            {
                trkTick = 0;
                for (int i = 0; i < trkStep.Length; i++)
                {
                    trkStep[i] = 0;
                }

                // ヘッダーのチェック
                if (!(bs[trkPtr] == 0x4D && bs[trkPtr + 1] == 0x54 && bs[trkPtr + 2] == 0x72 && bs[trkPtr + 3] == 0x6B))
                {
                    //不正なヘッダーを検知しました
                    throw new Exceptions.RcFailedOpenSMFHeaderException();
                }
                trkPtr += 4;
                //トラックの大きさをゲット
                trkDatLen = bs[trkPtr] * 0x1000000 + bs[trkPtr + 1] * 0x10000 + bs[trkPtr + 2] * 0x100 + bs[trkPtr + 3] + trkPtr + 4;
                trkPtr += 4;

                bool TrkEnd = false;
                do
                {
                    //デルタ取得
                    delta = getDelta(ref trkPtr, bs, null);
                    for (int i = 0; i < trkStep.Length; i++)
                    {
                        trkStep[i] += delta;
                    }
                    trkTick = delta;
                    int ch = 0;
                    //コマンド取得
                    byte cmd = bs[trkPtr];
                    //MIDIメッセージ取得
                    byte[] msg = null;
                    switch (cmd)
                    {
                        case 0xF0:
                            trkPtr++;
                            msg = SysExEvent(0xF0, ref trkPtr, bs);
                            break;
                        case 0xF7:
                            trkPtr++;
                            msg = SysExEvent(0xF7, ref trkPtr, bs);
                            break;
                        case 0xFF:
                            trkPtr++;
                            cmd = bs[trkPtr];
                            if (cmd == 0x2f)
                                TrkEnd = true;
                            msg = MetaEvent(prj, trk[(format == 0) ? 0 : trkNum], format, ref trkPtr, stDevNum, bs);
                            break;
                        default:
                            msg = setMIDIEvent(ref cmd, ref ch, ref trkPtr, bs);
                            if (format == 1)
                            {
                                trk[trkNum].OutChannel = ch;
                            }
                            break;
                    }

                    int num = (format != 0) ? trkNum : ch;
                    LinkedListNode<MIDIEvent> evt = prtAry[num].getEndEventNode();
                    if (evt != null) evt.Value.ST = trkStep[num];

                    if (!TrkEnd)
                    {
                        prtAry[num].insertEventNode(evt, 0, (MIDIEventType)cmd, msg, 100);
                    }
                    else
                    {
                        //トラックが終わりになったとき
                        if (format != 0)
                        {
                            //Format1の時は、現在のトラックのみにEndOfTrackを挿入する
                            prtAry[num].insertSpEventNode(evt, 0, MIDISpEventType.EndOfTrack, null);
                        }
                        else
                        {
                            //Format0の時は全トラックにEndOfTrackを挿入する
                            for (int i = 0; i < prtAry.Length; i++)
                            {
                                LinkedListNode<MIDIEvent> ev = prtAry[i].getEndEventNode();
                                if (ev != null) ev.Value.ST = trkStep[i];
                                prtAry[i].insertSpEventNode(ev, 0, MIDISpEventType.EndOfTrack, null);
                            }
                        }
                    }
                    trkStep[num] = 0;

                } while (trkPtr < trkDatLen && !TrkEnd);
            }

            #endregion

            #region gatetime調整
            //RemcoposerはRCP形式の為、NoteONとNoteOFFを同一のイベントで表現する
            //一方、SMFはNoteONとOFFが別イベントとして記録されている為、それをまとめる必要がある

            LinkedListNode<MIDITrack> track = prj.getStartTrackNode();
            while (track != null)
            {
                try
                {
                    LinkedListNode<MIDIPart> prt = track.Value.getStartPartNode();
                    if (prt == null) break;
                    LinkedListNode<MIDIEvent> evt = prt.Value.getStartEventNode();
                    int gt = 0;
                    while (evt != null)
                    {
                        //NoteONイベントを探す
                        if (evt.Value.Type != MIDIEventType.NoteON)
                        {
                            evt = prt.Value.getNextEventNode(evt);
                            continue;
                        }
                        //NoteONイベントが見つかったのでNoteOFFイベントを探す
                        gt = evt.Value.ST;
                        LinkedListNode<MIDIEvent> evt2 = prt.Value.getNextEventNode(evt);
                        while (evt2 != null)
                        {
                            if (evt2.Value.Type == MIDIEventType.NoteOff || (evt2.Value.Type == MIDIEventType.NoteON && evt2.Value.MIDIMessage[2] == 0))
                            {
                                if (evt.Value.MIDIMessage[1] == evt2.Value.MIDIMessage[1])
                                {
                                    //見つけた
                                    break;
                                }
                            }
                            //NoteOFFが見つかるまでのイベントのGateTimeを加算し続ける
                            gt += evt2.Value.ST;
                            evt2 = prt.Value.getNextEventNode(evt2);
                        }
                        //ループを抜けた時にOFFイベントを見つけていなければとりあえずGateTimeを1にしてしまう
                        if (evt2 == null)
                        {
                            gt = 1;
                        }
                        else
                        {
                            //OFFイベントを見つけていればそれをクリア
                            prt.Value.clearEventNode(evt2);
                        }
                        //NoteONのGateTimeを更新
                        ((MIDINoteEvent)evt.Value).GT = gt;
                        //次のNoteONイベント探しの旅へ
                        evt = prt.Value.getNextEventNode(evt);
                    }
                }
                catch (Exception ex)
                {
                    Log.ForcedWrite(ex);
                    throw;
                }

                track = prj.getNextTrackNode(track);
            }

            #endregion


            #region イベント調整

            track = prj.getStartTrackNode();
            while (track != null)
            {
                try
                {
                    LinkedListNode<MIDIPart> prt = track.Value.getStartPartNode();
                    while (prt != null)
                    {
                        LinkedListNode<MIDIEvent> eventNode = prt.Value.getStartEventNode();
                        while (eventNode != null)
                        {
                            MIDIEvent ev = eventNode.Value;
                            switch (ev.Type)
                            {
                                case MIDIEventType.NoteON:
                                    ((MIDINoteEvent)ev).KeyNumber = ev.MIDIMessage[1];
                                    ((MIDINoteEvent)ev).Vel = ev.MIDIMessage[2];
                                    ev.MIDIMessage = null;
                                    break;
                                case MIDIEventType.ControlChange:
                                    ((MIDIControlChangeEvent)ev).ControllerNumber = ev.MIDIMessage[1];
                                    ((MIDIControlChangeEvent)ev).ControllerValue = ev.MIDIMessage[2];
                                    ev.MIDIMessage = null;
                                    break;
                                case MIDIEventType.ProgramChange:
                                    ((MIDIProgramChangeEvent)ev).ProgramNumber = ev.MIDIMessage[1];
                                    ev.MIDIMessage = null;
                                    break;
                                case MIDIEventType.PitchBend:
                                    ((MIDIPitchBendEvent)ev).PitchValue = (short)((ev.MIDIMessage[2] << 7) | ev.MIDIMessage[1]);
                                    ev.MIDIMessage = null;
                                    break;
                            }
                            eventNode = prt.Value.getNextEventNode(eventNode);
                        }
                        prt.Value.insertEventNode(null, 0, MIDIEventType.MetaEndOfTrack , null);
                        prt = track.Value.getNextPartNode(prt);
                    }
                }
                catch (Exception ex)
                {
                    Log.ForcedWrite(ex);
                    throw;
                }

                track = prj.getNextTrackNode(track);
            }


            #endregion

            #region 小節線の追加



            track = prj.getStartTrackNode();
            while (track != null)
            {
                int beatDen = prj.Information.BeatDen;//分母
                int beatMol = prj.Information.BeatMol;//分子

                try
                {
                    LinkedListNode<MIDIPart> prt = track.Value.getStartPartNode();
                    while (prt != null)
                    {
                        int sum = 0;
                        LinkedListNode<MIDIEvent> eventNode = prt.Value.getStartEventNode();
                        while (eventNode != null)
                        {
                            MIDIEvent ev = eventNode.Value;

                            sum += ev.ST;
                            if(sum >= prj.Information.TimeBase * 4 / beatDen * beatMol)
                            {
                                //小節線を挿入
                                LinkedListNode<MIDIEvent> barLineNode = prt.Value.insertEventNode(eventNode, 0, MIDIEventType.BarLine, null);
                                MIDIBarLineEvent barLineEvent = (MIDIBarLineEvent)barLineNode.Value;
                                barLineEvent.MeasureTotalST = sum;
                                sum -= prj.Information.TimeBase * 4 / beatDen * beatMol;
                            }

                            eventNode = prt.Value.getNextEventNode(eventNode);
                        }
                        prt = track.Value.getNextPartNode(prt);
                    }
                }
                catch (Exception ex)
                {
                    Log.ForcedWrite(ex);
                    throw;
                }

                track = prj.getNextTrackNode(track);
            }

            #endregion

            return prj;
        }


        //プライベートメソッド

        private int getDelta(ref int trkPtr, byte[] bs, List<byte> msg)
        {
            int delta = 0;
            bool flg = true;
            do
            {
                if (msg != null) msg.Add(bs[trkPtr]);
                delta = delta * 0x80 + (bs[trkPtr] & 0x7f);
                if ((bs[trkPtr] & 0x80) == 0)
                {
                    flg = false;
                }
                trkPtr++;
            } while (flg);

            return delta;
        }

        private byte[] SysExEvent(byte cmd, ref int trkPtr, byte[] bs)
        {
            List<byte> msg = new List<byte>();
            int len = (int)bs[trkPtr++];
            msg.Add(cmd);
            for (int i = 0; i < len; i++)
            {
                msg.Add(bs[trkPtr++]);
            }

            return msg.ToArray();
        }

        private byte[] setMIDIEvent(ref byte cmd, ref int ch, ref int trkPtr, byte[] bs)
        {
            List<byte> msg = new List<byte>();
            if ((cmd & 0x80) != 0)
            {
                trkPtr++;
                Bcmd = (byte)(cmd & 0xf0);
                Bch = (cmd & 0x0f);
                cmd = Bcmd;
                ch = Bch;
            }
            else
            {
                //ランニングステータス発動
                cmd = Bcmd;
                ch = Bch;
            }
            msg.Add(cmd);
            msg.Add(bs[trkPtr++]);
            if (cmd != 0xC0 && cmd != 0xD0)
            {
                msg.Add(bs[trkPtr++]);
            }

            return msg.ToArray();
        }

        private byte[] MetaEvent(MIDIProject prj, MIDITrack trk, int format, ref int trkPtr, int stDevNum, byte[] bs)
        {
            List<byte> msg = new List<byte>();

            byte cmd = bs[trkPtr++];
            msg.Add(cmd);
            int len = 0;
            byte[] nam = null;
            string strFromByte = "";
            switch (cmd)
            {
                case 0x01://テキスト
                    len = getDelta(ref trkPtr, bs, msg);
                    nam = new byte[len];
                    for (int i = 0; i < len; i++, trkPtr++)
                    {
                        nam[i] = bs[trkPtr];
                        msg.Add(nam[i]);
                    }
                    strFromByte = Encoding.GetEncoding("Shift_JIS").GetString(nam).Replace("\0", "");
                    break;
                case 0x02://著作権表示
                    len = getDelta(ref trkPtr, bs, msg);
                    nam = new byte[len];
                    for (int i = 0; i < len; i++, trkPtr++)
                    {
                        nam[i] = bs[trkPtr];
                        msg.Add(nam[i]);
                    }
                    strFromByte = Encoding.GetEncoding("Shift_JIS").GetString(nam).Replace("\0", "");
                    prj.Information.Copyright = strFromByte;
                    break;
                case 0x03://曲名或いはトラック名
                    len = getDelta(ref trkPtr, bs, msg);
                    nam = new byte[len];
                    for (int i = 0; i < len; i++, trkPtr++)
                    {
                        nam[i] = bs[trkPtr];
                        msg.Add(nam[i]);
                    }
                    strFromByte = Encoding.GetEncoding("Shift_JIS").GetString(nam).Replace("\0", "");
                    if ((format == 0 || (format == 1 && trk.Number == 0)) && !titleSW)
                    {
                        prj.Information.Title = strFromByte;
                        titleSW = true;
                    }
                    else
                    {
                        trk.Name = strFromByte;
                    }
                    break;
                case 0x04://楽器名
                    len = getDelta(ref trkPtr, bs, msg);
                    nam = new byte[len];
                    for (int i = 0; i < len; i++, trkPtr++)
                    {
                        nam[i] = bs[trkPtr];
                        msg.Add(nam[i]);
                    }
                    strFromByte = Encoding.GetEncoding("Shift_JIS").GetString(nam).Replace("\0", "");
                    trk.Name = strFromByte;
                    break;
                case 0x05://歌詞
                case 0x06://マーカー
                case 0x07://キューポイント 
                case 0x08://プログラム名 (音色名) 
                case 0x09://デバイス名 (音源名) 
                    len = getDelta(ref trkPtr, bs, msg);
                    nam = new byte[len];
                    for (int i = 0; i < len; i++, trkPtr++)
                    {
                        nam[i] = bs[trkPtr];
                        msg.Add(nam[i]);
                    }
                    strFromByte = Encoding.GetEncoding("Shift_JIS").GetString(nam).Replace("\0", "");
                    break;
                case 0x20://MIDIチャンネルプリフィックス
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    break;
                case 0x21://出力ポート指定 (現在のSMFでは未定義) 
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    if (format == 1)
                    {
                        int dev = (stDevNum + bs[trkPtr]) % setting.MIDIOutDeviceList.Count;
                        trk.OutUserDeviceNumber = setting.MIDIOutDeviceList[dev].UsrNumber;
                        trk.OutUserDeviceName = setting.MIDIOutDeviceList[dev].UsrName;
                    }
                    msg.Add(bs[trkPtr++]);
                    break;
                case 0x2f://トラックの終端
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    break;
                case 0x51://テンポ
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    prj.Information.Tempo = 60000000 / (bs[trkPtr] * 0x10000 + bs[trkPtr + 1] * 0x100 + bs[trkPtr + 2]);
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    break;
                case 0x54://SMPTE オフセット
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);
                    break;
                case 0x58://拍子
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    prj.Information.BeatDen = (int)bs[trkPtr]; //分子
                    msg.Add(bs[trkPtr++]);
                    prj.Information.BeatMol = (int)Math.Pow(2.0, (double)bs[trkPtr]); //分母
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]);//1拍あたりのMIDIクロック数(無視)
                    msg.Add(bs[trkPtr++]);//MIDI4分音符(24MIDIクロック)の中に入る32分音符の数(無視)
                    break;
                case 0x59://調号
                    len = bs[trkPtr];
                    msg.Add(bs[trkPtr++]);
                    msg.Add(bs[trkPtr++]); //♯や♭の数が入る。正数なら♯の数、負数なら♭の数を表す。(無視)
                    msg.Add(bs[trkPtr++]); //長調か短調かを表すフラグで、0なら長調、1なら短調。(無視)
                    break;
                case 0x7F://シーケンサー特定メタイベント
                    len = getDelta(ref trkPtr, bs, msg);
                    for (int i = 0; i < len; i++, trkPtr++)
                    {
                        msg.Add(bs[trkPtr]);
                    }
                    break;
                default:
                    break;
            }

            return msg.ToArray();
        }

    }
}
