using System;
using System.Collections.Generic;
using NAudio.Midi;

namespace Remcomposer64.Core
{
    public class MidiDeviceManager : IDisposable
    {
        private readonly Dictionary<string, MidiOut> openedOutDevices = new();
        private readonly Dictionary<string, MidiIn> openedInDevices = new();

        private readonly Dictionary<string, int> outNameToIndex = new();
        private readonly Dictionary<string, int> inNameToIndex = new();

        private bool disposed = false;

        public MidiDeviceManager()
        {
            RefreshDeviceList();
        }

        // デバイス一覧を更新
        public void RefreshDeviceList()
        {
            outNameToIndex.Clear();

            int count = MidiOut.NumberOfDevices;
            for (int i = 0; i < count; i++)
            {
                var info = MidiOut.DeviceInfo(i);
                outNameToIndex[info.ProductName] = i;
            }

            inNameToIndex.Clear();

            count = MidiIn.NumberOfDevices;
            for (int i = 0; i < count; i++)
            {
                var info = MidiIn.DeviceInfo(i);
                inNameToIndex[info.ProductName] = i;
            }
        }

        public MidiOut GetMIDIOutDevice(string deviceName)
        {
            if (openedOutDevices.TryGetValue(deviceName, out var existing))
                return existing;

            if (!outNameToIndex.TryGetValue(deviceName, out int index))
                return null;
                //throw new Exception($"Not found MIDI OUT '{deviceName}'.");

            var dev = new MidiOut(index);
            openedOutDevices[deviceName] = dev;
            return dev;
        }

        public MidiIn GetMIDIInDevice(string deviceName)
        {
            if (openedInDevices.TryGetValue(deviceName, out var existing))
                return existing;

            if (!inNameToIndex.TryGetValue(deviceName, out int index))
                return null;
                //throw new Exception($"Not found MIDI IN '{deviceName}'");

            var dev = new MidiIn(index);
            openedInDevices[deviceName] = dev;
            return dev;
        }

        // アプリ終了時にすべて閉じる
        public void CloseAll()
        {
            foreach (var dev in openedOutDevices.Values)
            {
                try { dev.Dispose(); } catch { }
            }
            openedOutDevices.Clear();

            foreach (var dev in openedInDevices.Values)
            {
                try { dev.Dispose(); } catch { }
            }
            openedInDevices.Clear();
        }

        // IDisposable 実装
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // マネージャが管理しているデバイスをすべて閉じる
                CloseAll();
            }

            disposed = true;
        }

        ~MidiDeviceManager()
        {
            Dispose(false);
        }
    }
}
