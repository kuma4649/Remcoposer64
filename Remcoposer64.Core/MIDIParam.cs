using NAudio.SoundFont;
using System.Runtime.CompilerServices;

namespace Remcoposer64.Core
{
    public class MIDIParam
    {
        public int MIDIModule = 0;//0:GMIDI 1:XG 2:GS
        public byte[][] note = null;
        public string[] notes = null;
        public byte?[][] keyPress = null;
        public byte?[][] cc = null;
        public byte?[] pc = null;
        public byte?[] cPress = null;
        public short?[] bend = null;
        public int[][] level = null;
        public byte?[] nrpnVibRate = null;
        public byte?[] nrpnVibDepth = null;
        public byte?[] nrpnVibDelay = null;
        public byte?[] nrpnLPF = null;
        public byte?[] nrpnLPFRsn = null;
        public byte?[] nrpnHPF = null;
        public byte?[] nrpnEQBaseGain = null;
        public byte?[] nrpnEQTrebleGain = null;
        public byte?[] nrpnEQBaseFrq = null;
        public byte?[] nrpnEQTrebleFrq = null;
        public byte?[] nrpnEGAttack = null;
        public byte?[] nrpnEGDecay = null;
        public byte?[] nrpnEGRls = null;
        public byte?[] LCDDisplay = null;
        public int LCDDisplayTime = 0;
        public byte[] LCD8850Display = null;
        public int LCD8850DisplayTime = 0;
        public int LCDDisplayTimeXG = 0;
        public byte[] LCDDisplayLetter = null;
        public int LCDDisplayLetterTime = 0;
        public int LCDDisplayLetterTimeXG = 0;
        public int LCDDisplayLetterLen = 0;
        public string Lyric = "";

        public byte MasterVolume = 0;

        public int ReverbXG = 1; //HALL1
        public int ChorusXG = 1; //CHORUS1
        public int VariationXG = 15; //DELAY LCR
        public int Insertion1XG = 56;// DISTORTION 
        public int Insertion2XG = 56;//DISTORTION 
        public int Insertion3XG = 56;// DISTORTION 
        public int Insertion4XG = 56;//DISTORTION 

        public int ReverbGS = 4; // Room1(default)
        public int ChorusGS = 2; // Chorus3(default)
        public int DelayGS = 0; // Delay1(default)
        public int EFXGS = 0; // Thru(default)

        public int RevType_MSB = 0;
        public int RevType_LSB = 0;
        public int ChoType_MSB = 0;
        public int ChoType_LSB = 0;
        public int VarType_MSB = 0;
        public int VarType_LSB = 0;
        public int Ins1Type_MSB = 0;
        public int Ins1Type_LSB = 0;
        public int Ins2Type_MSB = 0;
        public int Ins2Type_LSB = 0;
        public int Ins3Type_MSB = 0;
        public int Ins3Type_LSB = 0;
        public int Ins4Type_MSB = 0;
        public int Ins4Type_LSB = 0;
        public int EFXType_MSB = 0;
        public int EFXType_LSB = 0;

        private readonly byte[] msg = null;
        private int msgInd = 0;
        private bool NowSystemMsg = false;

        private static readonly int[] tblRevTypeXG = new int[] {
            0x0000
            ,0x0100,0x0101,0x0106,0x0107
            ,0x0200,0x0201,0x0202,0x0205,0x0206,0x0207
            ,0x0300,0x0301
            ,0x0400,0x0407
            ,0x1000
            ,0x1100
            ,0x1200
            ,0x1300
        };

        private static readonly int[] tblChoTypeXG = new int[] {
            0x0000
            ,0x4100,0x4101,0x4102,0x4103,0x4104,0x4105,0x4106,0x4107,0x4108
            ,0x4200,0x4201,0x4202,0x4208
            ,0x4300,0x4301,0x4307,0x4308
            ,0x4400
            ,0x4800
            ,0x5700
        };

        private static readonly int[] tblVarInsTypeXG = new int[] {
0x0000
,0x0100,0x0101,0x0106,0x0107
,0x0200,0x0201,0x0202,0x0205,0x0206,0x0207
,0x0300,0x0301
,0x0400,0x0407
,0x0500
,0x0600
,0x0700
,0x0800
,0x0900,0x0901
,0x0a00
,0x0b00
,0x1000
,0x1100
,0x1200
,0x1300
,0x1400,0x1401,0x1402
,0x4100,0x4101,0x4102,0x4103,0x4104,0x4105,0x4106,0x4107,0x4108
,0x4200,0x4201,0x4202,0x4208
,0x4300,0x4301,0x4307,0x4308
,0x4400
,0x4500,0x4501,0x4502,0x4503
,0x4600
,0x4700
,0x4800,0x4808
,0x4900,0x4901,0x4908
,0x4a00,0x4a08
,0x4b00,0x4b08
,0x4c00
,0x4d00
,0x4e00,0x4e01,0x4e02
,0x5000,0x5001
,0x5100
,0x5200,0x5201,0x5202,0x5208
,0x5300
,0x5400
,0x5500
,0x5600,0x5601,0x5602,0x5603
,0x5700
,0x5800
,0x5d00
,0x5e00
,0x5f00,0x5f01
,0x6000,0x6001
,0x6100,0x6101
,0x6200,0x6201,0x6202,0x6203
,0x6300,0x6301
        };

        private static readonly int[] tblIns1TypeFromEFX = new int[] {
            0x0000,0x0100,0x0101,0x0102,0x0103,0x0110,0x0111,0x0120
            ,0x0121,0x0122,0x0123,0x0124,0x0125,0x0126,0x0130,0x0131
            ,0x0140,0x0141,0x0142,0x0143,0x0144,0x0150,0x0151,0x0152
            ,0x0153,0x0154,0x0155,0x0156,0x0157,0x0160,0x0161,0x0170
            ,0x0171,0x0172,0x0173,0x0200,0x0201,0x0202,0x0203,0x0204
            ,0x0205,0x0206,0x0207,0x0208,0x0209,0x020a,0x020b,0x0300
            ,0x0400,0x0401,0x0402,0x0403,0x0404,0x0405,0x0406,0x0500
            ,0x1100,0x1101,0x1102,0x1103,0x1104,0x1105,0x1106,0x1107,0x1108
        };

        private readonly bool[] isRPN = new bool[16];
        public Dictionary<ushort, ushort>[] dicRPN = new Dictionary<ushort, ushort>[16];
        public Dictionary<ushort, byte>[] dicNRPN = new Dictionary<ushort, byte>[16];

        private static readonly Dictionary<int, int> revTypeXGMap
            = CreateMap(tblRevTypeXG);

        private static readonly Dictionary<int, int> choTypeXGMap
            = CreateMap(tblChoTypeXG);

        private static readonly Dictionary<int, int> varTypeXGMap
            = CreateMap(tblVarInsTypeXG);

        private static readonly Dictionary<int, int> ins1TypeFromEFXMap
            = CreateMap(tblIns1TypeFromEFX);

        private static Dictionary<int, int> CreateMap(int[] table)
        {
            var dic = new Dictionary<int, int>(table.Length);

            for (int i = 0; i < table.Length; i++)
                dic[table[i]] = i;

            return dic;
        }

        //static MIDIParam()
        //{
        //    revTypeXGMap = new Dictionary<int, int>(tblRevTypeXG.Length);
        //    for (int i = 0; i < tblRevTypeXG.Length; i++)
        //    {
        //        revTypeXGMap[tblRevTypeXG[i]] = i;
        //    }
        //}

        public MIDIParam()
        {

            note = [new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128], new byte[128]];
            notes = new string[16];
            keyPress = [new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128]];
            cc = new byte?[16][] { new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128], new byte?[128] };
            pc = new byte?[16];
            cPress = new byte?[16];
            bend = new short?[16];
            level = new int[16][] { new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5], new int[5] };
            nrpnVibRate = new byte?[16];
            nrpnVibDepth = new byte?[16];
            nrpnVibDelay = new byte?[16];
            nrpnLPF = new byte?[16];
            nrpnLPFRsn = new byte?[16];
            nrpnHPF = new byte?[16];
            nrpnEQBaseGain = new byte?[16];
            nrpnEQTrebleGain = new byte?[16];
            nrpnEQBaseFrq = new byte?[16];
            nrpnEQTrebleFrq = new byte?[16];
            nrpnEGAttack = new byte?[16];
            nrpnEGDecay = new byte?[16];
            nrpnEGRls = new byte?[16];
            LCDDisplay = new byte?[64];
            LCD8850Display = new byte[27 * 4 * 16];// 160*8];
            LCDDisplayTime = 0;
            LCD8850DisplayTime = 0;
            LCDDisplayTimeXG = 0;

            LCDDisplayLetter = new byte[32];
            LCDDisplayLetterTime = 0;
            LCDDisplayLetterTimeXG = 0;

            msg = new byte[256];
            msgInd = 0;
            NowSystemMsg = false;

            for (int ch = 0; ch < 16; ch++)
            {
                //for (int n = 0; n < 256; n++)
                //{
                //    note[ch][n] = 0;
                //}
                cc[ch][7] = 110;
                cc[ch][10] = 64;
                cc[ch][11] = 110;
                bend[ch] = 8192;
                notes[ch] = "          ";

                //nrpnVibRate[ch] = 64;
                //nrpnVibDepth[ch] = 64;
                //nrpnVibDelay[ch] = 64;
                //nrpnLPF[ch] = 64;
                //nrpnLPFRsn[ch] = 64;
                //nrpnHPF[ch] = 64;
                //nrpnEQBaseGain[ch] = 64;
                //nrpnEQTrebleGain[ch] = 64;
                //nrpnEQBaseFrq[ch] = 64;
                //nrpnEQTrebleFrq[ch] = 64;
                //nrpnEGAttack[ch] = 64;
                //nrpnEGDecay[ch] = 64;
                //nrpnEGRls[ch] = 64;

            }
            Lyric = "";
            latestCmd = 0;

            //isRPN = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];
            dicRPN = new Dictionary<ushort, ushort>[16];
            dicNRPN = new Dictionary<ushort, byte>[16];
            for (int i = 0; i < 16; i++) { dicRPN[i] = new(); dicNRPN[i] = new(); }
        }

        private byte latestCmd = 0;

        public void SendBuffer(byte[] dat)
        {
            foreach (byte d in dat)
            {
                bool IsStatusByte = (d & 0x80) != 0;

                if (IsStatusByte)
                {
                    latestCmd = d;
                    //Debug.Write(string.Format("[{0:X02}]:", d));
                    NowSystemMsg = ((d & 0xf0) == 0xf0);
                    if (d == 0xf7 && NowSystemMsg)
                    {
                        if (msgInd < msg.Length) msg[msgInd] = 0xf7;
                        AnalyzeSystemMsg();
                        NowSystemMsg = false;
                    }
                    msgInd = 0;
                }
                //else
                //{
                //    Debug.Write(string.Format("{0:X02}:", d));
                //}

                if (msgInd < msg.Length)
                {
                    msg[msgInd] = d;
                    //Console.Write("{0:X2}:", msg[msgInd]);
                    msgInd++;
                }

                Analyze(d);

            }
        }

        public void Send(int msg)
        {
            byte status = (byte)msg;
            byte d1 = (byte)(msg >> 8);
            byte d2 = (byte)(msg >> 16);

            AnalyzeShortMessageFast(status, d1, d2);
        }

        private void AnalyzeShortMessageFast(byte status, byte d1, byte d2)
        {
            byte cmd = (byte)(status & 0xF0);
            byte ch = (byte)(status & 0x0F);

            switch (cmd)
            {
                case 0x80:
                    note[ch][d1] = 0;
                    break;

                case 0x90:
                    note[ch][d1] = d2;
                    UpdateLevel(ch, d2);
                    break;

                case 0xA0:
                    keyPress[ch][d1] = d2;
                    break;

                case 0xB0:
                    cc[ch][d1] = d2;
                    AnalyzeControlChangeFast(ch, d1);
                    break;

                case 0xC0:
                    pc[ch] = d1;
                    break;

                case 0xD0:
                    cPress[ch] = d1;
                    break;

                case 0xE0:
                    bend[ch] = (short)((d2 << 7) | d1);
                    break;
            }
        }

        private void UpdateLevel(byte ch, byte d2)
        {
            if (d2 != 0)//NOTE OFF の代用の場合は液晶パラメータの更新を行わない
            {
                int? lv = cc[ch][7] * cc[ch][11] * d2 >> (7 + 7);
                int? lvl = (lv * (cc[ch][10] > 64 ? ((127 - cc[ch][10]) * 2) : 127)) >> (7); //65->124 127->0
                int? lvr = (lv * (cc[ch][10] < 64 ? (cc[ch][10] * 2) : 127)) >> (7); // 63->126 0->0
                if (lvl != null) level[ch][0] = (byte)lvl;
                if (lv != null) level[ch][1] = (byte)lv;
                if (lvr != null) level[ch][2] = (byte)lvr;
                if (level[ch][3] < lv)
                {
                    level[ch][3] = (byte)lv;
                    level[ch][4] = 100;
                }
            }
        }

        //private readonly byte[,] ccb = new byte[16, 128];

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void AnalyzeControlChangeFast(    byte status,    byte controller,    byte value)
        //{
        //    ccb[status & 0x0f, controller] = value;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AnalyzeControlChangeFast(byte ch, byte controller)
        {
            switch (controller)
            {
                case 0x06: // Data Entry MSB
                    AnalyzeDataEntryMSBFast(ch);

                    if (isRPN[ch])
                        AnalyzeRPN(ch);
                    else
                        AnalyzeNRPN(ch);

                    break;

                case 0x26: // Data Entry LSB
                    if (isRPN[ch])
                        AnalyzeRPN(ch);

                    break;

                case 0x65:
                case 0x64:
                    isRPN[ch] = true;
                    break;

                case 0x63:
                case 0x62:
                    isRPN[ch] = false;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AnalyzeDataEntryMSBFast(byte ch)
        {
            var ccCh = cc[ch];

            byte nrpnMsb = ccCh[0x63] ?? 0;
            byte nrpnLsb = ccCh[0x62] ?? 0;
            byte dataMsb = ccCh[0x06] ?? 0;

            if (nrpnMsb != 0x01)
                return;

            switch (nrpnLsb)
            {
                case 0x08: nrpnVibRate[ch] = dataMsb; break;
                case 0x09: nrpnVibDepth[ch] = dataMsb; break;
                case 0x0A: nrpnVibDelay[ch] = dataMsb; break;

                case 0x20: nrpnLPF[ch] = dataMsb; break;
                case 0x21: nrpnLPFRsn[ch] = dataMsb; break;
                case 0x24: nrpnHPF[ch] = dataMsb; break;

                case 0x30: nrpnEQBaseGain[ch] = dataMsb; break;
                case 0x31: nrpnEQTrebleGain[ch] = dataMsb; break;
                case 0x34: nrpnEQBaseFrq[ch] = dataMsb; break;
                case 0x35: nrpnEQTrebleFrq[ch] = dataMsb; break;

                case 0x63: nrpnEGAttack[ch] = dataMsb; break;
                case 0x64: nrpnEGDecay[ch] = dataMsb; break;
                case 0x66: nrpnEGRls[ch] = dataMsb; break;
            }
        }


        public void AnalyzeShortMessage(byte cmd, byte v1, byte v2)
        {
            switch (cmd & 0xF0)
            {
                case 0xC0:
                case 0xD0:
                    AnalyzeOneByte(cmd);
                    AnalyzeOneByte(v1);
                    break;

                default:
                    AnalyzeOneByte(cmd);
                    AnalyzeOneByte(v1);
                    AnalyzeOneByte(v2);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AnalyzeOneByte(byte d)
        {
            bool isStatusByte = (d & 0x80) != 0;

            if (isStatusByte)
            {
                latestCmd = d;

                NowSystemMsg = (d & 0xf0) == 0xf0;

                if (d == 0xf7 && NowSystemMsg)
                {
                    if (msgInd < msg.Length)
                        msg[msgInd] = 0xf7;

                    AnalyzeSystemMsg();
                    NowSystemMsg = false;
                }

                msgInd = 0;
            }

            if (msgInd < msg.Length)
            {
                msg[msgInd] = d;
                msgInd++;
            }

            Analyze(d);
        }


        private void Analyze(byte d)
        {
            bool IsStatusByte = (d & 0x80) != 0;
            if (IsStatusByte) return;

            byte cmd = (byte)(latestCmd & 0xf0);
            byte ch = (byte)(latestCmd & 0xf);
            bool IsStatusByte0 = (msg[0] & 0x80) != 0;

            switch (msgInd)
            {
                case 1:
                    if (!IsStatusByte0) AnalyzeData1(cmd, ch, 0);
                    break;
                case 2:
                    if (IsStatusByte0) AnalyzeData1(cmd, ch, 1);
                    else AnalyzeData2(cmd, ch, 0);
                    break;
                case 3:
                    if (IsStatusByte0) AnalyzeData2(cmd, ch, 1);
                    break;
            }
        }

        private void AnalyzeData1(byte cmd, byte ch, int i)
        {
            switch (cmd)
            {
                case 0xc0://Program Change
                    pc[ch] = msg[i + 0];
                    msgInd = 0;
                    break;
                case 0xd0://Ch Press
                    cPress[ch] = msg[i + 0];
                    msgInd = 0;
                    break;
            }
        }

        private void AnalyzeData2(byte cmd, byte ch, int i)
        {
            switch (cmd)
            {
                case 0x80://Note OFF
                    note[ch][msg[i + 0]] = 0;
                    msgInd = 0;
                    break;
                case 0x90://Note ON
                    note[ch][msg[i + 0]] = msg[i + 1];
                    UpdateLevel(ch, msg[i + 1]);
                    msgInd = 0;
                    break;
                case 0xa0://Key Press
                    keyPress[ch][msg[i + 0]] = msg[i + 1];
                    msgInd = 0;
                    break;
                case 0xb0://Control Change
                    cc[ch][msg[i + 0]] = msg[i + 1];
                    AnalyzeControlChange(ch);
                    msgInd = 0;
                    break;
                case 0xe0://Pitch Bend
                    bend[ch] = (short)(msg[i + 1] * 0x80 + msg[i + 0]);
                    msgInd = 0;
                    break;
            }
        }


        private void AnalyzeControlChange(byte ch)
        {
            byte ccno = msg[1];
            switch (ccno)
            {
                case 0x06://Data Entry MSB
                    AnalyzeDataEntryMSB(ch);
                    if (!isRPN[ch]) AnalyzeNRPN(ch);
                    else AnalyzeRPN(ch);
                    break;
                case 0x26://Data Entry LSB
                    if (isRPN[ch]) AnalyzeRPN(ch);
                    break;
                case 0x65:
                case 0x64:
                    isRPN[ch] = true;
                    break;
                case 0x63:
                case 0x62:
                    isRPN[ch] = false;
                    break;
            }
        }

        private void AnalyzeRPN(byte ch)
        {
            var ccCh = cc[ch];

            byte rpnMsb = ccCh[0x65] ?? 0;
            byte rpnLsb = ccCh[0x64] ?? 0;
            byte datMsb = ccCh[0x06] ?? 0;
            byte datLsb = ccCh[0x26] ?? 0;

            ushort key = (ushort)((rpnMsb << 8) | rpnLsb);
            ushort val = (ushort)((datMsb << 8) | datLsb);

            //if (dicRPN[ch].ContainsKey(key))
            dicRPN[ch][key] = val;
            //else
            //    dicRPN[ch].Add(key, val);
        }

        private void AnalyzeNRPN(byte ch)
        {
            var ccCh = cc[ch];

            byte NRPN_MSB = ccCh[0x63] ?? 0;
            byte NRPN_LSB = ccCh[0x62] ?? 0;
            byte Dat_MSB = ccCh[0x06] ?? 0;

            ushort key = (ushort)((NRPN_MSB << 8) | NRPN_LSB);
            byte val = Dat_MSB;
            //if (dicNRPN[ch].ContainsKey(key))
            dicNRPN[ch][key] = val;
            //else
            //    dicNRPN[ch].Add(key, val);
        }

        private void AnalyzeDataEntryMSB(byte ch)
        {
            switch (cc[ch][0x63])//NRPN MSB
            {
                case 0x01:
                    switch (cc[ch][0x62]) //NRPN LSB
                    {
                        case 0x08:
                            nrpnVibRate[ch] = cc[ch][0x06];
                            break;
                        case 0x09:
                            nrpnVibDepth[ch] = cc[ch][0x06];
                            break;
                        case 0x0a:
                            nrpnVibDelay[ch] = cc[ch][0x06];
                            break;
                        case 0x20:
                            nrpnLPF[ch] = cc[ch][0x06];
                            break;
                        case 0x21:
                            nrpnLPFRsn[ch] = cc[ch][0x06];
                            break;
                        case 0x24:
                            nrpnHPF[ch] = cc[ch][0x06];
                            break;
                        case 0x30:
                            nrpnEQBaseGain[ch] = cc[ch][0x06];
                            break;
                        case 0x31:
                            nrpnEQTrebleGain[ch] = cc[ch][0x06];
                            break;
                        case 0x34:
                            nrpnEQBaseFrq[ch] = cc[ch][0x06];
                            break;
                        case 0x35:
                            nrpnEQTrebleFrq[ch] = cc[ch][0x06];
                            break;
                        case 0x63:
                            nrpnEGAttack[ch] = cc[ch][0x06];
                            break;
                        case 0x64:
                            nrpnEGDecay[ch] = cc[ch][0x06];
                            break;
                        case 0x66:
                            nrpnEGRls[ch] = cc[ch][0x06];
                            break;
                    }
                    break;
            }
        }

        public int waitSysEx = 0;

        public int waitGMReset = 300;
        public int waitGSReset = 300;
        public int waitXGReset = 300;
        public int waitEtcSysEx = 20;
        public int waitMax = 500;

        private void AnalyzeSystemMsg()
        {
            waitSysEx = 0;
            if (msg[0] != 0xf0) return;

            //
            // GM Reset
            //
            if (msgInd >= 5
                && msg[1] == 0x7e
                && msg[3] == 0x09
                && msg[4] == 0x01
                && msg[5] == 0xf7)
            {
                waitSysEx = waitGMReset;
                return;
            }

            if (msgInd < 8)
            {
                waitSysEx = waitEtcSysEx;
                return;
            }

            byte manufactureID = msg[1];
            int adr = 0;
            int ptr = 0;

            if (manufactureID == 0x43)//YAMAHA ID?
            {
                if ((msg[2] & 0xf0) != 0x10)
                {
                    waitSysEx = waitEtcSysEx;
                    return;
                }
                if (msg[3] != 0x4c)
                {
                    waitSysEx = waitEtcSysEx;
                    return;
                }

                adr = msg[4] * 0x10000 + msg[5] * 0x100 + msg[6];

                if (adr == 0x00007e
                    && msgInd >= 8
                    && msg[7] == 0x00)
                {
                    waitSysEx = waitXGReset;
                    return;
                }

                ptr = 7;
            }
            else if (manufactureID == 0x41)//Roland ID
            {
                //if (msg[3] != 0x42) return;
                //if (msg[4] != 0x12) return;
                adr = msg[5] * 0x10000 + msg[6] * 0x100 + msg[7];

                if (msg[3] == 0x42
                    && msg[4] == 0x12
                    && adr == 0x40007f
                    && msgInd >= 10
                    && msg[8] == 0x00)
                {
                    waitSysEx = waitGSReset;
                    return;
                }

                ptr = 8;
            }
            else if (manufactureID == 0x7f)//universal realtime message
            {
                if (msg[2] == 0x7f && msg[3] == 0x04 && msg[4] == 0x01 && msg[7] == 0xf7)
                {
                    MasterVolume = msg[5];
                }
                waitSysEx = waitEtcSysEx;
                return;
            }

            waitSysEx = Math.Min(waitEtcSysEx + (msgInd >> 1), waitMax);

            while (ptr < msgInd && msg[ptr] != 0xf7)
            {
                byte dat = msg[ptr];

                if (manufactureID == 0x43) //YAMAHA
                {
                    if (adr == 0x020100 || adr == 0x020101)
                    {
                        //REVERB TYPE MSB/LSB
                        if (adr == 0x020100) RevType_MSB = dat & 0xff;
                        else RevType_LSB = dat & 0xff;
                        ReverbXG = GetRevTypeXG();
                    }
                    else if (adr == 0x020120 || adr == 0x020121)
                    {
                        //CHORUS TYPE MSB/LSB
                        if (adr == 0x020120) ChoType_MSB = dat & 0xff;
                        else ChoType_LSB = dat & 0xff;
                        ChorusXG = GetChoTypeXG();
                    }
                    else if (adr == 0x020140 || adr == 0x020141)
                    {
                        //VARIATION TYPE MSB/LSB
                        if (adr == 0x020140) VarType_MSB = dat & 0xff;
                        else VarType_LSB = dat & 0xff;
                        VariationXG = GetVarTypeXG();
                    }
                    else if (adr == 0x030000 || adr == 0x030001)
                    {
                        //INSERTION EFFECT1 TYPE MSB/LSB
                        if (adr == 0x030000) Ins1Type_MSB = dat & 0xff;
                        else Ins1Type_LSB = dat & 0xff;
                        Insertion1XG = GetIns1TypeXG();
                    }
                    else if (adr == 0x030100 || adr == 0x030101)
                    {
                        //INSERTION EFFECT2 TYPE MSB/LSB
                        if (adr == 0x030100) Ins2Type_MSB = dat & 0xff;
                        else Ins2Type_LSB = dat & 0xff;
                        Insertion2XG = GetIns2TypeXG();
                    }
                    else if (adr == 0x030200 || adr == 0x030201)
                    {
                        //INSERTION EFFECT3 TYPE MSB/LSB
                        if (adr == 0x030200) Ins3Type_MSB = dat & 0xff;
                        else Ins3Type_LSB = dat & 0xff;
                        Insertion3XG = GetIns3TypeXG();
                    }
                    else if (adr == 0x030300 || adr == 0x030301)
                    {
                        //INSERTION EFFECT4 TYPE MSB/LSB
                        if (adr == 0x030300) Ins4Type_MSB = dat & 0xff;
                        else Ins4Type_LSB = dat & 0xff;
                        Insertion4XG = GetIns4TypeXG();
                    }
                    else if (adr >= 0x060000 && adr <= 0x06001f)
                    {
                        if (adr == 0x060000) for (int i = 0; i < 32; i++) LCDDisplayLetter[i] = 0x20;

                        //DISPLAY LETTER Data
                        dat = (byte)((dat < 0x20 || dat > 0x7f) ? 0x20 : dat);
                        LCDDisplayLetter[adr & 0x1f] = dat;
                        LCDDisplayLetterLen = (adr & 0x1f) + 1;
                        LCDDisplayLetterTime = 400;
                        if (LCDDisplayLetterLen > 16) LCDDisplayLetterTime = 40;
                    }
                    else if (adr >= 0x070000 && adr <= 0x07002f)
                    {
                        //DISPLAY Dot Data
                        LCDDisplay[adr & 0x3f] = dat;
                        if (adr == 0x07002f)
                        {
                            LCDDisplayTimeXG = 400;
                        }
                    }
                }
                else if (manufactureID == 0x41) //GS
                {
                    if (adr >= 0x100000 && adr <= 0x10001f)
                    {
                        if (adr == 0x100000) for (int i = 0; i < 32; i++) LCDDisplayLetter[i] = 0x20;

                        //DISPLAY LETTER Data
                        dat = (byte)((dat < 0x20 || dat > 0x7f) ? 0x20 : dat);
                        LCDDisplayLetter[adr & 0x1f] = dat;
                        LCDDisplayLetterLen = (adr & 0x1f);// + 1;
                        LCDDisplayLetterTime = 400;
                        if (LCDDisplayLetterLen > 16) LCDDisplayLetterTime = 40;
                    }
                    else if (adr >= 0x100100 && adr <= 0x10013f)
                    {
                        //DISPLAY Dot Data
                        LCDDisplay[adr & 0x3f] = dat;
                        if (adr == 0x10013f)
                        {
                            LCDDisplayTime = 400;
                        }
                    }
                    else if (adr >= 0x200000 && adr < 0x201000)
                    {
                        //8850Display Dot Data(160px x 64 px) ((27byte x 4row) x 16set)
                        if ((adr & 0x7f) < 108)
                        {
                            LCD8850Display[((adr & 0xf00) >> 8) * 108 + (adr & 0x7f)] = dat;// % (27*4)] = dat;
                        }
                        if (adr == 0x200000 + 0xf00 + 108 - 1)
                        {
                            LCD8850DisplayTime = 400;
                        }
                    }
                    else if (adr == 0x400130)
                    {
                        //REVERB MACRO
                        if (dat >= 0 && dat <= 7) ReverbGS = dat;
                    }
                    else if (adr == 0x400138)
                    {
                        //CHORUS MACRO
                        if (dat >= 0 && dat <= 7) ChorusGS = dat;
                    }
                    else if (adr == 0x400150)
                    {
                        //Delay MACRO
                        if (dat >= 0 && dat <= 9) DelayGS = dat;
                    }
                    else if (adr == 0x400300)
                    {
                        //EFX Type
                        EFXType_MSB = dat & 0xff;
                        EFXGS = GetIns1TypeFromEFX();
                    }
                    else if (adr == 0x400301)
                    {
                        //EFX Type
                        EFXType_LSB = dat & 0xff;
                        EFXGS = GetIns1TypeFromEFX();
                    }
                }


                ptr++;
                adr++;
            }

        }

        private int GetRevTypeXG() => LookupXGType(revTypeXGMap, RevType_MSB, RevType_LSB);

        private int GetChoTypeXG() => LookupXGType(choTypeXGMap, ChoType_MSB, ChoType_LSB);

        private int GetVarTypeXG() => LookupXGType(varTypeXGMap, VarType_MSB, VarType_LSB);

        private int GetIns1TypeXG() => LookupXGType(varTypeXGMap, Ins1Type_MSB, Ins1Type_LSB);

        private int GetIns2TypeXG() => LookupXGType(varTypeXGMap, Ins2Type_MSB, Ins2Type_LSB);

        private int GetIns3TypeXG() => LookupXGType(varTypeXGMap, Ins3Type_MSB, Ins3Type_LSB);

        private int GetIns4TypeXG() => LookupXGType(varTypeXGMap, Ins4Type_MSB, Ins4Type_LSB);

        private static int LookupXGType(
            Dictionary<int, int> map,
            int msb,
            int lsb)
        {
            int key = (msb << 8) | lsb;

            if (map.TryGetValue(key, out int v))
                return v;

            key = msb << 8;

            if (map.TryGetValue(key, out v))
                return v;

            return 0;
        }


        private int GetIns1TypeFromEFX()
        {
            return ins1TypeFromEFXMap.TryGetValue(
                (EFXType_MSB << 8) | EFXType_LSB,
                out int v)
                ? v
                : 0;
        }

    }
}