using NAudio.Midi;

namespace Remcoposer64.Core
{
    public class ImportMID
    {
        public class ImportResult
        {
            public int Format { get; set; }
            public int Tracks { get; set; }
            public int TPQN { get; set; }
            public List<MidiEvent[]> RawEvents { get; set; } = new();
        }

        public ImportResult Load(string filename)
        {
            Log.Write(LogLevel.Information, $"Import MIDI: {filename}");

            var mf = new MidiFile(filename, false);

            var result = new ImportResult()
            {
                Format = mf.FileFormat,
                Tracks = mf.Tracks,
                TPQN = mf.DeltaTicksPerQuarterNote
            };

            for (int t = 0; t < mf.Tracks; t++)
            {
                result.RawEvents.Add(mf.Events[t].ToArray());
            }

            Log.Write(LogLevel.Debug, $"Format={result.Format}, Tracks={result.Tracks}, TPQN={result.TPQN}");

            return result;
        }
    }
}
