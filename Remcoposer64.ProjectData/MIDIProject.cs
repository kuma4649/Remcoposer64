using Remcoposer64.Common;
using Remcoposer64.ProjectData.DTO;
using Remcoposer64.ProjectData.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Remcoposer64.ProjectData
{
    [Serializable]
    public class MIDIProject
    {
        private MIDIInformation _Information = new MIDIInformation();
        //private List<MIDITrack> _Track = new List<MIDITrack>();
        private List<MIDIUserExclusive> _UserExclusive = new List<MIDIUserExclusive>();
        private List<MIDIRythm> _Rythm = new List<MIDIRythm>();
        private bool _RelativeTempoChangeSW = false;
        private double _RelativeTempoChangeTargetTempo = 0.0;
        private double _RelativeTempoChangeTickSlice = 0.0;
        private double _RelativeTempoChangeNowTempo = 0.0;
        private Setting setting;

        public MIDIInformation Information
        {
            set
            {
                _Information = value;
            }
            get
            {
                return _Information;
            }
        }
        public LinkedList<MIDITrack> Track { get; set; } = new LinkedList<MIDITrack>();
        //{
        //    set
        //    {
        //        _Track = value;
        //    }
        //    get
        //    {
        //        return _Track;
        //    }
        //}
        public List<MIDIUserExclusive> UserExclusive
        {
            set
            {
                _UserExclusive = value;
            }
            get
            {
                return _UserExclusive;
            }
        }
        public List<MIDIRythm> Rythm
        {
            set
            {
                _Rythm = value;
            }
            get
            {
                return _Rythm;
            }
        }

        public bool RelativeTempoChangeSW
        {
            set
            {
                _RelativeTempoChangeSW = value;
            }
            get
            {
                return _RelativeTempoChangeSW;
            }
        }
        public double RelativeTempoChangeTargetTempo
        {
            set
            {
                _RelativeTempoChangeTargetTempo = value;
            }
            get
            {
                return _RelativeTempoChangeTargetTempo;
            }
        }
        public double RelativeTempoChangeTickSlice
        {
            set
            {
                _RelativeTempoChangeTickSlice = value;
            }
            get
            {
                return _RelativeTempoChangeTickSlice;
            }
        }
        public double RelativeTempoChangeNowTempo
        {
            set
            {
                _RelativeTempoChangeNowTempo = value;
            }
            get
            {
                return _RelativeTempoChangeNowTempo;
            }
        }

        public MIDIProject()
        {
        }

        public MIDIProject(Setting setting)
        {
            this.setting = setting;
            this.Information = new MIDIInformation();
            this.Track = new LinkedList<MIDITrack>();
            this.UserExclusive = new List<MIDIUserExclusive>();
            this.Rythm = new List<MIDIRythm>();
        }


        public void Save(string fn)
        {
            try
            {
                string filename = fn;

                MIDIProjectDto dto = GetDto(this);

                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MIDIProjectDto));
                    serializer.Serialize(fs, dto);
                    fs.Close();
                }
            }
            catch
            {
                throw;
            }
        }


        //初めのトラックを得る
        public LinkedListNode<MIDITrack> getStartTrackNode()
        {
            //if (Information.tStartIndex == null) return null;

            return Track.First;// (int)Information.tStartIndex];
        }

        //最後の小節を得る
        public LinkedListNode<MIDITrack> getEndTrackNode()
        {
            //if (Information.tEndIndex == null) return null;

            return Track.Last;// [(int)Information.tEndIndex];
        }

        //指定した小節の次の小節を得る
        public LinkedListNode<MIDITrack> getNextTrackNode(LinkedListNode<MIDITrack>  trk)
        {
            if (trk == null
                //|| trk.AfterIndex == null
                ) return null;

            return trk.Next;// Track[(int)trk.AfterIndex];
        }

        //指定した小節の前の小節を得る
        public LinkedListNode<MIDITrack> getPrevTrackNode(LinkedListNode< MIDITrack> trk)
        {
            if (trk == null 
                //|| trk.BeforeIndex == null
                ) return null;

            return trk.Previous;// Track[(int)trk.BeforeIndex];
        }

        //全ての小節をメモリから消去する
        public void clearAllTrackMemory()
        {
            this.Track.Clear();
            this.Information.tCounter = 0;
            this.Information.tStartIndex = null;
            this.Information.tEndIndex = null;
            this.Information.tNumber = 0;
        }

        //全ての小節を消去する
        public void clearTrack()
        {
            this.Information.tCounter = 0;
            this.Information.tStartIndex = null;
            this.Information.tEndIndex = null;
        }

        /// <summary>
        /// 小節を挿入する
        /// (既存小節が増えると挿入位置を特定するのに時間がかかるようになるので注意)
        /// </summary>
        /// <param name="StartTick">絶対値によるTick値</param>
        /// <param name="EndTick">絶対値によるTick値</param>
        /// <param name="mea">小節</param>
        public void insertTrack(int TrackNumber, MIDITrack trk)
        {
            if (trk == null) return;
            trk.TrackNumber = TrackNumber;

            if (this.Track == null)
            {
                this.Track = new LinkedList<MIDITrack>();
            }
            if (this.Track.Count == 0 || this.Information.tStartIndex == null)//初めの小節
            {
                //trk.AfterIndex = null;
                //trk.BeforeIndex = null;
                //trk.Number = this.Information.tNumber;
                this.Track.AddLast(trk);
                //this.Information.tStartIndex = 0;
                //this.Information.tEndIndex = 0;
                //this.Information.tCounter = 1;
                //this.Information.tNumber++;
                return;
            }

            //遅くなる原因になっているループ
            LinkedListNode<MIDITrack> pTrk = getStartTrackNode();
            while (true)
            {
                if (pTrk.Value.TrackNumber > trk.TrackNumber)
                {
                    pTrk = getPrevTrackNode(pTrk);
                    break;
                }
                LinkedListNode<MIDITrack> ppTrk = getNextTrackNode(pTrk);
                if (ppTrk == null) break;
                pTrk = ppTrk;
            }

            //trk.BeforeIndex = pTrk.Number;
            //trk.AfterIndex = pTrk.AfterIndex;
            //trk.Number = this.Information.tNumber;
            //pTrk.AfterIndex = trk.Number;
            LinkedListNode<MIDITrack> ntrk = this.Track.AddLast(trk);
            if (trk.AfterIndex == null)
            {
                this.Information.tEndIndex = this.Information.tNumber;
            }
            else
            {
                pTrk = getNextTrackNode(ntrk);
                //pTrk.BeforeIndex = trk.Number;
            }
            //this.Information.tCounter++;
            //this.Information.tNumber++;

        }

        public MIDITrack getTrack4TrackNumber(int trkNumber)
        {
            LinkedListNode<MIDITrack> trk = getStartTrackNode();
            while (trk != null)
            {
                if (trk.Value.TrackNumber == trkNumber)
                {
                    break;
                }
                trk = trk.Next;
            }

            return trk == null ? null : trk.Value;
        }

        public void SetSetting(Setting setting)
        {
            this.setting = setting;
        }

        public void Reinit()
        {
        }


        private MIDIProjectDto GetDto(MIDIProject src)
        {
            var dto = new MIDIProjectDto
            {
                Information = GetInformationDto(src.Information),
                Tracks = GetTrackDto(src.Track),
                UserExclusive = GetUserExclusiveDto(src.UserExclusive),
                Rythm = GetRythmDto(src.Rythm)
            };

            return dto;
        }

        private MIDIInformationDto GetInformationDto(MIDIInformation inf)
        {
            return new MIDIInformationDto
            {
                FileFullName = inf.FileFullName,
                FileName = inf.FileName,
                Title = inf.Title,
                Writer = inf.Writer,
                Composer = inf.Composer,
                Arranger = inf.Arranger,
                Copyright = inf.Copyright,
                Memo = inf.Memo,
                Key = inf.Key,
                PlayBIAS = inf.PlayBIAS,
                ControlFileCM6 = inf.ControlFileCM6,
                ControlFileGSD = inf.ControlFileGSD,
                ControlFileGSD2 = inf.ControlFileGSD2,
                CreationDate = inf.CreationDate,
                UpdatedDate = inf.UpdatedDate,
                TimeBase = inf.TimeBase,
                Tempo = inf.Tempo,
                BeatDen = inf.BeatDen,
                BeatMol = inf.BeatMol,
                Marker = inf.Marker != null ? new List<int>(inf.Marker) : new List<int>(),
                tStartIndex = inf.tStartIndex,
                tEndIndex = inf.tEndIndex,
                tNumber = inf.tNumber,
                tCounter = inf.tCounter
            };
        }

        private List<MIDIUserExclusiveDto> GetUserExclusiveDto(List<MIDIUserExclusive> list)
        {
            var result = new List<MIDIUserExclusiveDto>();

            foreach (var ux in list)
            {
                result.Add(new MIDIUserExclusiveDto
                {
                    Name = ux.Name,
                    Memo = ux.Memo,
                    Exclusive = ux.Exclusive
                });
            }

            return result;
        }

        private List<MIDIRythmDto> GetRythmDto(List<MIDIRythm> list)
        {
            var result = new List<MIDIRythmDto>();

            foreach (var r in list)
            {
                result.Add(new MIDIRythmDto
                {
                    Name = r.Name,
                    Key = r.Key,
                    Gt = r.Gt
                });
            }

            return result;
        }

        private List<MIDITrackDto> GetTrackDto(LinkedList<MIDITrack> list)
        {
            var result = new List<MIDITrackDto>();

            foreach (var trk in list)
            {
                var dto = new MIDITrackDto
                {
                    Number = trk.Number,
                    TrackNumber = trk.TrackNumber,
                    Name = trk.Name,
                    Memo = trk.Memo,
                    Comment = trk.Comment,

                    OutDevice = trk.OutDevice,
                    OutChannel = trk.OutChannel,
                    InDevice = trk.InDevice,
                    InChannel = trk.InChannel,

                    Solo = trk.Solo,
                    Mute = trk.Mute,

                    Key = trk.Key,
                    St = trk.St,

                    // LinkedList<MIDIPart> → List<MIDIPartDto>
                    Parts = GetPartDto(trk.Part),

                    // LinkedList<MIDIRythm> → List<MIDIRythmDto>
                    Rythm = GetRythmDto(trk.Rythm.ToList())
                };

                result.Add(dto);
            }

            return result;
        }


        private List<MIDIPartDto> GetPartDto(LinkedList<MIDIPart> list)
        {
            var result = new List<MIDIPartDto>();

            foreach (var part in list)
            {
                result.Add(new MIDIPartDto
                {
                    Number = part.Number,
                    Name = part.Name,
                    StartTick = part.StartTick,
                    Events = GetEventDto(part.Event)
                });
            }

            return result;
        }

        private List<MIDIEventDto> GetEventDto(LinkedList<MIDIEvent> list)
        {
            var result = new List<MIDIEventDto>();

            foreach (var ev in list)
            {
                result.Add(ConvertEvent(ev));
            }

            return result;
        }

        private MIDIEventDto ConvertEvent(MIDIEvent ev)
        {
            switch (ev)
            {
                case MIDINoteEvent ne:
                    return ConvertNoteEvent(ne);

                case MIDIControlChangeEvent cc:
                    return ConvertControlChangeEvent(cc);

                case MIDIProgramChangeEvent pc:
                    return ConvertProgramChangeEvent(pc);

                case MIDIPitchBendEvent pb:
                    return ConvertPitchBendEvent(pb);

                case MIDIPolyphonicKeyPressureEvent pk:
                    return ConvertPolyPressureEvent(pk);

                case MIDIChannelPressureEvent cp:
                    return ConvertChannelPressureEvent(cp);

                case MIDITempoEvent te:
                    return ConvertTempoEvent(te);

                case MIDIMemoEvent me:
                    return ConvertMemoEvent(me);

                case MIDIMetaEvent meta:
                    return ConvertMetaEvent(meta);

                case MIDIBarLineEvent bar:
                    return ConvertBarLineEvent(bar);

                case MIDIEndEvent end:
                    return ConvertEndEvent(end);

                case MIDIRolandBaseEvent rb:
                    return ConvertRolandBaseEvent(rb);

                case MIDIRolandDeviceEvent rd:
                    return ConvertRolandDeviceEvent(rd);

                case MIDIRolandParaEvent rp:
                    return ConvertRolandParaEvent(rp);

                case MIDISysExEvent sx:
                    return ConvertSysExEvent(sx);

                case MIDISameMeasEvent sm:
                    return ConvertSameMeasEvent(sm);

                case MIDILoopStartEvent ls:
                    return ConvertLoopStartEvent(ls);

                case MIDILoopEndEvent le:
                    return ConvertLoopEndEvent(le);

                case MIDIBankProgramEvent bp:
                    return ConvertBankProgramEvent(bp);

                default:
                    throw new NotImplementedException($"未対応のイベント: {ev.GetType().Name}");
            }
        }

        private MIDIEventDto ConvertLoopStartEvent(MIDILoopStartEvent ls)
        {
            return new MIDILoopStartEventDto
            {
                Type = ls.Type,
                Meas = ls.Meas,
                Step = ls.Step,
                ST = ls.ST,
                MIDIMessage = ls.MIDIMessage,
                MIDIMessageLst = ls.MIDIMessageLst
            };
        }

        private MIDIEventDto ConvertLoopEndEvent(MIDILoopEndEvent le)
        {
            return new MIDILoopEndEventDto
            {
                Type = le.Type,
                Meas = le.Meas,
                Step = le.Step,
                ST = le.ST,
                MIDIMessage = le.MIDIMessage,
                MIDIMessageLst = le.MIDIMessageLst,
                LoopCounter = le.LoopCounter
            };
        }

        private MIDIEventDto ConvertSameMeasEvent(MIDISameMeasEvent sm)
        {
            return new MIDISameMeasEventDto
            {
                Type = sm.Type,
                Meas = sm.Meas,
                Step = sm.Step,
                ST = sm.ST,
                MIDIMessage = sm.MIDIMessage,
                MIDIMessageLst = sm.MIDIMessageLst,

                RepeatMeas = sm.RepeatMeas
            };
        }


        private MIDIEventDto ConvertSysExEvent(MIDISysExEvent sx)
        {
            return new MIDISysExEventDto
            {
                Type = sx.Type,
                Meas = sx.Meas,
                Step = sx.Step,
                ST = sx.ST,
                MIDIMessage = sx.MIDIMessage,
                MIDIMessageLst = sx.MIDIMessageLst,

                Data = sx.Data,
                Name = sx.Name,
                GT = sx.GT,
                VEL = sx.VEL,
            };
        }


        private MIDIEventDto ConvertRolandParaEvent(MIDIRolandParaEvent rp)
        {
            return new MIDIRolandParaEventDto
            {
                Type = rp.Type,
                Meas = rp.Meas,
                Step = rp.Step,
                ST = rp.ST,
                MIDIMessage = rp.MIDIMessage,
                MIDIMessageLst = rp.MIDIMessageLst,

                RolandPara_gt = rp.RolandPara_gt,
                RolandPara_vel = rp.RolandPara_vel
            };
        }


        private MIDIEventDto ConvertRolandDeviceEvent(MIDIRolandDeviceEvent rd)
        {
            return new MIDIRolandDeviceEventDto
            {
                Type = rd.Type,
                Meas = rd.Meas,
                Step = rd.Step,
                ST = rd.ST,
                MIDIMessage = rd.MIDIMessage,
                MIDIMessageLst = rd.MIDIMessageLst,

                RolandDev_gt = rd.RolandDev_gt,
                RolandDev_vel = rd.RolandDev_vel
            };
        }


        private MIDIEventDto ConvertRolandBaseEvent(MIDIRolandBaseEvent rb)
        {
            return new MIDIRolandBaseEventDto
            {
                Type = rb.Type,
                Meas = rb.Meas,
                Step = rb.Step,
                ST = rb.ST,
                MIDIMessage = rb.MIDIMessage,
                MIDIMessageLst = rb.MIDIMessageLst,

                RolandBase_gt = rb.RolandBase_gt,
                RolandBase_vel = rb.RolandBase_vel
            };
        }


        private MIDIEventDto ConvertEndEvent(MIDIEndEvent end)
        {
            return new MIDIEndEventDto
            {
                Type = end.Type,
                Meas = end.Meas,
                Step = end.Step,
                ST = end.ST,
                MIDIMessage = end.MIDIMessage,
                MIDIMessageLst = end.MIDIMessageLst
            };
        }


        private MIDIEventDto ConvertBarLineEvent(MIDIBarLineEvent bar)
        {
            return new MIDIBarLineEventDto
            {
                Type = bar.Type,
                Meas = bar.Meas,
                Step = bar.Step,
                ST = bar.ST,
                MIDIMessage = bar.MIDIMessage,
                MIDIMessageLst = bar.MIDIMessageLst,

                MeasureTotalST = bar.MeasureTotalST
            };
        }


        private MIDIEventDto ConvertMetaEvent(MIDIMetaEvent meta)
        {
            return new MIDIMetaEventDto
            {
                Type = meta.Type,
                Meas = meta.Meas,
                Step = meta.Step,
                ST = meta.ST,
                MIDIMessage = meta.MIDIMessage,
                MIDIMessageLst = meta.MIDIMessageLst,

                MetaType = meta.MetaType,
                Data = meta.Data
            };
        }


        private MIDIEventDto ConvertMemoEvent(MIDIMemoEvent me)
        {
            return new MIDIMemoEventDto
            {
                Type = me.Type,
                Meas = me.Meas,
                Step = me.Step,
                ST = me.ST,
                MIDIMessage = me.MIDIMessage,
                MIDIMessageLst = me.MIDIMessageLst,

                Text = me.Text
            };
        }


        private MIDIEventDto ConvertTempoEvent(MIDITempoEvent te)
        {
            return new MIDITempoEventDto
            {
                Type = te.Type,
                Meas = te.Meas,
                Step = te.Step,
                ST = te.ST,
                MIDIMessage = te.MIDIMessage,
                MIDIMessageLst = te.MIDIMessageLst,

                TempoValue = te.TempoValue
            };
        }


        private MIDIEventDto ConvertChannelPressureEvent(MIDIChannelPressureEvent cp)
        {
            return new MIDIChannelPressureEventDto
            {
                Type = cp.Type,
                Meas = cp.Meas,
                Step = cp.Step,
                ST = cp.ST,
                MIDIMessage = cp.MIDIMessage,
                MIDIMessageLst = cp.MIDIMessageLst,

                PressureChannel = cp.PressureChannel,
                PressureValue = cp.PressureValue
            };
        }


        private MIDIEventDto ConvertPolyPressureEvent(MIDIPolyphonicKeyPressureEvent pk)
        {
            return new MIDIPolyphonicKeyPressureEventDto
            {
                Type = pk.Type,
                Meas = pk.Meas,
                Step = pk.Step,
                ST = pk.ST,
                MIDIMessage = pk.MIDIMessage,
                MIDIMessageLst = pk.MIDIMessageLst,

                KeyNumber = pk.KeyNumber,
                PressureValue = pk.PressureValue
            };
        }

        private MIDIEventDto ConvertPitchBendEvent(MIDIPitchBendEvent pb)
        {
            return new MIDIPitchBendEventDto
            {
                Type = pb.Type,
                Meas = pb.Meas,
                Step = pb.Step,
                ST = pb.ST,
                MIDIMessage = pb.MIDIMessage,
                MIDIMessageLst = pb.MIDIMessageLst,

                PitchValue = pb.PitchValue
            };
        }


        private MIDIEventDto ConvertProgramChangeEvent(MIDIProgramChangeEvent pc)
        {
            return new MIDIProgramChangeEventDto
            {
                Type = pc.Type,
                Meas = pc.Meas,
                Step = pc.Step,
                ST = pc.ST,
                MIDIMessage = pc.MIDIMessage,
                MIDIMessageLst = pc.MIDIMessageLst,

                ProgramNumber = pc.ProgramNumber
            };
        }


        private MIDIEventDto ConvertControlChangeEvent(MIDIControlChangeEvent cc)
        {
            return new MIDIControlChangeEventDto
            {
                Type = cc.Type,
                Meas = cc.Meas,
                Step = cc.Step,
                ST = cc.ST,
                MIDIMessage = cc.MIDIMessage,
                MIDIMessageLst = cc.MIDIMessageLst,

                ControllerNumber = cc.ControllerNumber,
                ControllerValue = cc.ControllerValue
            };
        }

        private MIDIEventDto ConvertBankProgramEvent(MIDIBankProgramEvent bp)
        {
            return new MIDIBankProgramEventDto
            {
                Type = bp.Type,
                Meas = bp.Meas,
                Step = bp.Step,
                ST = bp.ST,
                MIDIMessage = bp.MIDIMessage,
                MIDIMessageLst = bp.MIDIMessageLst,

                ProgramNumber = bp.ProgramNumber,
                ControlChange = bp.ControlChange
            };
        }

        private MIDINoteEventDto ConvertNoteEvent(MIDINoteEvent ne)
        {
            return new MIDINoteEventDto
            {
                Type = ne.Type,
                Meas = ne.Meas,
                Step = ne.Step,
                ST = ne.ST,
                MIDIMessage = ne.MIDIMessage,
                MIDIMessageLst = ne.MIDIMessageLst,

                KeyNumber = ne.KeyNumber,
                NoteName = ne.NoteName,
                GT = ne.GT,
                Vel = ne.Vel,
                Tie = ne.Tie
            };
        }


        public static MIDIProject Load(string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(MIDIProjectDto));

            using (var sr = new StreamReader(fileName))
            {
                MIDIProjectDto dto = (MIDIProjectDto)xs.Deserialize(sr);
                return ConvertDtoToProject(dto);
            }
        }

        private static MIDIProject ConvertDtoToProject(MIDIProjectDto dto)
        {
            MIDIProject prj = new MIDIProject();

            prj.Information = ConvertDtoToInformation(dto.Information);
            prj.Rythm = ConvertDtoToRythm(dto.Rythm);
            prj.UserExclusive = ConvertDtoToUserExclusive(dto.UserExclusive);
            prj.Track = ConvertDtoToTrack(dto.Tracks);

            return prj;
        }

        private static LinkedList<MIDITrack> ConvertDtoToTrack(List<MIDITrackDto> tracks)
        {
            var result = new LinkedList<MIDITrack>();

            if (tracks == null)
                return result;

            foreach (var trackDto in tracks)
            {
                var track = new MIDITrack();

                // 基本情報
                track.Number = trackDto.Number;
                track.TrackNumber = trackDto.TrackNumber;
                track.Name = trackDto.Name;
                track.Memo = trackDto.Memo;
                track.Comment = trackDto.Comment;

                // デバイス設定
                track.OutDevice = trackDto.OutDevice;
                track.OutChannel = trackDto.OutChannel;
                track.InDevice = trackDto.InDevice;
                track.InChannel = trackDto.InChannel;

                // ソロ／ミュート
                track.Solo = trackDto.Solo;
                track.Mute = trackDto.Mute;

                // キー／ST
                track.Key = trackDto.Key;
                track.St = trackDto.St;

                // Rythm
                track.Rythm = ConvertDtoToRythmLinkedList(trackDto.Rythm);

                // Parts
                track.Part = ConvertDtoToPart(trackDto.Parts);

                result.AddLast(track);
            }

            return result;
        }


        private static LinkedList<MIDIPart> ConvertDtoToPart(List<MIDIPartDto> parts)
        {
            var result = new LinkedList<MIDIPart>();

            if (parts == null)
                return result;

            foreach (var p in parts)
            {
                var part = new MIDIPart();

                part.Number = p.Number;
                part.Name = p.Name;
                part.StartTick = p.StartTick;

                // イベント復元
                foreach (var evDto in p.Events)
                {
                    part.Event.AddLast(ConvertDtoToEvent(evDto));
                }

                result.AddLast(part);
            }

            return result;
        }


        private static List<MIDIUserExclusive> ConvertDtoToUserExclusive(List<MIDIUserExclusiveDto> list)
        {
            var result = new List<MIDIUserExclusive>();

            if (list == null)
                return result;

            foreach (var ux in list)
            {
                result.Add(new MIDIUserExclusive
                {
                    Name = ux.Name,
                    Memo = ux.Memo,
                    Exclusive = ux.Exclusive
                });
            }

            return result;
        }


        private static List<MIDIRythm> ConvertDtoToRythm(List<MIDIRythmDto> list)
        {
            var result = new List<MIDIRythm>();

            if (list == null)
                return result;

            foreach (var r in list)
            {
                result.Add(new MIDIRythm
                {
                    Name = r.Name,
                    Key = r.Key,
                    Gt = r.Gt
                });
            }

            return result;
        }

        private static LinkedList<MIDIRythm> ConvertDtoToRythmLinkedList(List<MIDIRythmDto> list)
        {
            var result = new LinkedList<MIDIRythm>();

            if (list == null)
                return result;

            foreach (var r in list)
            {
                result.AddLast(new MIDIRythm
                {
                    Name = r.Name,
                    Key = r.Key,
                    Gt = r.Gt
                });
            }

            return result;
        }


        private static MIDIInformation ConvertDtoToInformation(MIDIInformationDto dto)
        {
            var inf = new MIDIInformation();

            inf.FileFullName = dto.FileFullName;
            inf.FileName = dto.FileName;

            inf.Title = dto.Title;
            inf.Writer = dto.Writer;
            inf.Composer = dto.Composer;
            inf.Arranger = dto.Arranger;
            inf.Copyright = dto.Copyright;
            inf.Memo = dto.Memo;

            inf.Key = dto.Key;
            inf.PlayBIAS = dto.PlayBIAS;

            inf.ControlFileCM6 = dto.ControlFileCM6;
            inf.ControlFileGSD = dto.ControlFileGSD;
            inf.ControlFileGSD2 = dto.ControlFileGSD2;

            inf.CreationDate = dto.CreationDate;
            inf.UpdatedDate = dto.UpdatedDate;

            inf.TimeBase = dto.TimeBase;
            inf.Tempo = dto.Tempo;

            inf.BeatDen = dto.BeatDen;
            inf.BeatMol = dto.BeatMol;

            inf.Marker = dto.Marker != null ? new List<int>(dto.Marker) : new List<int>();

            inf.tStartIndex = dto.tStartIndex;
            inf.tEndIndex = dto.tEndIndex;
            inf.tNumber = dto.tNumber;
            inf.tCounter = dto.tCounter;

            return inf;
        }

        private static MIDIEvent ConvertDtoToEvent(MIDIEventDto dto)
        {
            switch (dto.Type)
            {
                case MIDIEventType.NoteON:
                    var n = new MIDINoteEvent();
                    n.Meas = dto.Meas;
                    n.Step = dto.Step;
                    n.ST = dto.ST;
                    n.KeyNumber = ((MIDINoteEventDto)dto).KeyNumber;
                    n.Vel = ((MIDINoteEventDto)dto).Vel;
                    n.GT = ((MIDINoteEventDto)dto).GT;
                    return n;

                case MIDIEventType.ControlChange:
                    var cc = new MIDIControlChangeEvent();
                    cc.Meas = dto.Meas;
                    cc.Step = dto.Step;
                    cc.ST = dto.ST;
                    cc.ControllerNumber = ((MIDIControlChangeEventDto)dto).ControllerNumber;
                    cc.ControllerValue = ((MIDIControlChangeEventDto)dto).ControllerValue;
                    return cc;

                case MIDIEventType.ProgramChange:
                    var pc = new MIDIProgramChangeEvent();
                    pc.Meas = dto.Meas;
                    pc.Step = dto.Step;
                    pc.ST = dto.ST;
                    pc.ProgramNumber = ((MIDIProgramChangeEventDto)dto).ProgramNumber;
                    return pc;

                case MIDIEventType.PitchBend:
                    var pb = new MIDIPitchBendEvent();
                    pb.Meas = dto.Meas;
                    pb.Step = dto.Step;
                    pb.ST = dto.ST;
                    pb.PitchValue = ((MIDIPitchBendEventDto)dto).PitchValue;
                    return pb;

                case MIDIEventType.KeyAfterTouch:
                    var pk = new MIDIPolyphonicKeyPressureEvent();
                    pk.Meas = dto.Meas;
                    pk.Step = dto.Step;
                    pk.ST = dto.ST;
                    pk.KeyNumber = ((MIDIPolyphonicKeyPressureEventDto)dto).KeyNumber;
                    pk.PressureValue = ((MIDIPolyphonicKeyPressureEventDto)dto).PressureValue;
                    return pk;

                case MIDIEventType.ChannelAfterTouch:
                    var cp = new MIDIChannelPressureEvent();
                    cp.Meas = dto.Meas;
                    cp.Step = dto.Step;
                    cp.ST = dto.ST;
                    cp.PressureChannel = ((MIDIChannelPressureEventDto)dto).PressureChannel;
                    cp.PressureValue = ((MIDIChannelPressureEventDto)dto).PressureValue;
                    return cp;

                case MIDIEventType.MetaTempo:
                    var te = new MIDITempoEvent();
                    te.Meas = dto.Meas;
                    te.Step = dto.Step;
                    te.ST = dto.ST;
                    te.TempoValue = ((MIDITempoEventDto)dto).TempoValue;
                    return te;

                case MIDIEventType.Memo:
                    var me = new MIDIMemoEvent();
                    me.Meas = dto.Meas;
                    me.Step = dto.Step;
                    me.ST = dto.ST;
                    me.Text = ((MIDIMemoEventDto)dto).Text;
                    return me;

                case MIDIEventType.Meta:
                    var meta = new MIDIMetaEvent();
                    meta.Meas = dto.Meas;
                    meta.Step = dto.Step;
                    meta.ST = dto.ST;
                    meta.MetaType = ((MIDIMetaEventDto)dto).MetaType;
                    meta.Data = ((MIDIMetaEventDto)dto).Data;
                    return meta;

                case MIDIEventType.BarLine:
                    var bar = new MIDIBarLineEvent();
                    bar.Meas = dto.Meas;
                    bar.Step = dto.Step;
                    bar.ST = dto.ST;
                    bar.MeasureTotalST = ((MIDIBarLineEventDto)dto).MeasureTotalST;
                    return bar;

                case MIDIEventType.MetaEndOfTrack:
                    var end = new MIDIEndEvent();
                    end.Meas = dto.Meas;
                    end.Step = dto.Step;
                    end.ST = dto.ST;
                    return end;

                case MIDIEventType.RolandBase:
                    var rb = new MIDIRolandBaseEvent();
                    rb.Meas = dto.Meas;
                    rb.Step = dto.Step;
                    rb.ST = dto.ST;
                    rb.RolandBase_gt = ((MIDIRolandBaseEventDto)dto).RolandBase_gt;
                    rb.RolandBase_vel = ((MIDIRolandBaseEventDto)dto).RolandBase_vel;
                    return rb;

                case MIDIEventType.RolandDevice:
                    var rd = new MIDIRolandDeviceEvent();
                    rd.Meas = dto.Meas;
                    rd.Step = dto.Step;
                    rd.ST = dto.ST;
                    rd.RolandDev_gt = ((MIDIRolandDeviceEventDto)dto).RolandDev_gt;
                    rd.RolandDev_vel = ((MIDIRolandDeviceEventDto)dto).RolandDev_vel;
                    return rd;

                case MIDIEventType.RolandPara:
                    var rp = new MIDIRolandParaEvent();
                    rp.Meas = dto.Meas;
                    rp.Step = dto.Step;
                    rp.ST = dto.ST;
                    rp.RolandPara_gt = ((MIDIRolandParaEventDto)dto).RolandPara_gt;
                    rp.RolandPara_vel = ((MIDIRolandParaEventDto)dto).RolandPara_vel;
                    return rp;

                case MIDIEventType.SysExF0:
                    var sx = new MIDISysExEvent();
                    sx.Meas = dto.Meas;
                    sx.Step = dto.Step;
                    sx.ST = dto.ST;
                    sx.GT = ((MIDISysExEventDto)dto).GT;
                    sx.VEL = ((MIDISysExEventDto)dto).VEL;
                    sx.Data = ((MIDISysExEventDto)dto).Data;
                    sx.Name = ((MIDISysExEventDto)dto).Name;
                    return sx;

                case MIDIEventType.SameMeas:
                    var sm = new MIDISameMeasEvent();
                    sm.Meas = dto.Meas;
                    sm.Step = dto.Step;
                    sm.ST = dto.ST;
                    sm.RepeatMeas = ((MIDISameMeasEventDto)dto).RepeatMeas;
                    return sm;

                case MIDIEventType.LoopStart:
                    return new MIDILoopStartEvent
                    {
                        Meas = dto.Meas,
                        Step = dto.Step,
                        ST = dto.ST,
                        MIDIMessage = dto.MIDIMessage,
                        MIDIMessageLst = dto.MIDIMessageLst
                    };

                case MIDIEventType.LoopEnd:
                    var leDto = (MIDILoopEndEventDto)dto;
                    return new MIDILoopEndEvent
                    {
                        Meas = dto.Meas,
                        Step = dto.Step,
                        ST = dto.ST,
                        MIDIMessage = dto.MIDIMessage,
                        MIDIMessageLst = dto.MIDIMessageLst,
                        LoopCounter = leDto.LoopCounter
                    };

                case MIDIEventType.BankProgram:
                    var bp = new MIDIBankProgramEvent();
                    bp.Meas = dto.Meas;
                    bp.Step = dto.Step;
                    bp.ST = dto.ST;
                    bp.ProgramNumber = ((MIDIBankProgramEventDto)dto).ProgramNumber;
                    bp.ControlChange = ((MIDIBankProgramEventDto)dto).ControlChange;
                    return bp;
            }

            throw new Exception("Unknown event type: " + dto.Type);
        }

    }

}
