using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;

namespace QuietOnTheSetUI
{
    /// <summary>
    /// Enforces a maximum master volume on active playback devices.
    /// The original app only reacted to volume notifications on the default
    /// device, so a missed callback or an output-device switch could leave
    /// the volume uncapped. This guard also polls, caps every active render
    /// device, and reconnects after device or COM errors.
    /// </summary>
    internal sealed class VolumeGuard : IDisposable
    {
        const int PollIntervalMs = 200;
        const int DeviceRefreshTicks = 10;

        readonly MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        readonly System.Windows.Forms.Timer timer;
        readonly Dictionary<string, MMDevice> devices = new Dictionary<string, MMDevice>();
        readonly object sync = new object();
        bool applying;
        bool disposed;
        bool isLocked;
        int maxVolume;
        int ticksSinceRefresh;
        MMDevice watchedDefault;

        public event Action<int> CurrentVolumeChanged;
        public event Action VolumeLimitResisted;

        int suppressAnnounceTicks;
        bool pendingResistAnnounce;

        public bool IsLocked
        {
            get { lock (sync) { return isLocked; } }
            set
            {
                lock (sync)
                {
                    isLocked = value;
                    if (value)
                    {
                        // Ignore the first cap after locking so a high
                        // starting volume does not count as a manual raise.
                        suppressAnnounceTicks = 3;
                    }
                }
                if (value)
                {
                    Enforce();
                }
            }
        }

        public int MaxVolume
        {
            get { lock (sync) { return maxVolume; } }
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                lock (sync) { maxVolume = value; }
                if (IsLocked)
                {
                    Enforce();
                }
            }
        }

        public VolumeGuard()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = PollIntervalMs;
            timer.Tick += (s, e) => Enforce();
            RefreshDevices();
            timer.Start();
        }

        public int ReadCurrentVolume()
        {
            try
            {
                if (watchedDefault == null)
                {
                    RefreshDevices();
                }
                if (watchedDefault == null)
                {
                    return 0;
                }
                return Convert.ToInt16(watchedDefault.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }
            catch
            {
                return 0;
            }
        }

        public void Enforce()
        {
            if (disposed)
            {
                return;
            }

            lock (sync)
            {
                if (applying)
                {
                    return;
                }
                applying = true;
            }

            try
            {
                ticksSinceRefresh++;
                if (watchedDefault == null || ticksSinceRefresh >= DeviceRefreshTicks)
                {
                    ticksSinceRefresh = 0;
                    RefreshDevices();
                }

                bool canAnnounce;
                lock (sync)
                {
                    if (suppressAnnounceTicks > 0)
                    {
                        suppressAnnounceTicks--;
                    }
                    canAnnounce = isLocked && suppressAnnounceTicks <= 0;
                }

                if (!IsLocked)
                {
                    return;
                }

                if (CapCachedDevices() && canAnnounce)
                {
                    pendingResistAnnounce = true;
                }
            }
            catch
            {
                try
                {
                    RefreshDevices();
                    if (IsLocked && CapCachedDevices())
                    {
                        bool canAnnounce;
                        lock (sync)
                        {
                            canAnnounce = suppressAnnounceTicks <= 0;
                        }
                        if (canAnnounce)
                        {
                            pendingResistAnnounce = true;
                        }
                    }
                }
                catch
                {
                }
            }
            finally
            {
                lock (sync) { applying = false; }
            }

            if (pendingResistAnnounce)
            {
                pendingResistAnnounce = false;
                var resisted = VolumeLimitResisted;
                if (resisted != null)
                {
                    resisted();
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            try
            {
                timer.Stop();
                timer.Dispose();
            }
            catch
            {
            }
            UnhookDefault();
            devices.Clear();
            TryRelease(enumerator);
        }

        static bool CapDevice(MMDevice device, float cap)
        {
            if (device == null)
            {
                return false;
            }
            var volume = device.AudioEndpointVolume;
            if (volume == null)
            {
                return false;
            }
            // A little slack so rounding noise does not look like a raise.
            if (volume.MasterVolumeLevelScalar > cap + 0.004f)
            {
                volume.MasterVolumeLevelScalar = cap;
                return true;
            }
            return false;
        }

        static void TryRelease(object obj)
        {
            var disposable = obj as IDisposable;
            if (disposable == null)
            {
                return;
            }
            try
            {
                disposable.Dispose();
            }
            catch
            {
            }
        }

        bool CapCachedDevices()
        {
            float cap = MaxVolume / 100f;
            var snapshot = new List<MMDevice>(devices.Values);
            if (watchedDefault != null && !snapshot.Contains(watchedDefault))
            {
                snapshot.Add(watchedDefault);
            }
            bool resisted = false;
            foreach (var device in snapshot)
            {
                try
                {
                    if (CapDevice(device, cap))
                    {
                        resisted = true;
                    }
                }
                catch
                {
                    // The device may have been removed.
                }
            }
            return resisted;
        }

        void RefreshDevices()
        {
            HookDefaultDeviceIfNeeded();
            try
            {
                var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                var seen = new HashSet<string>();
                for (int i = 0; i < collection.Count; i++)
                {
                    MMDevice device = collection[i];
                    string id = null;
                    try { id = device.ID; } catch { continue; }
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }
                    seen.Add(id);
                    if (!devices.ContainsKey(id))
                    {
                        devices[id] = device;
                    }
                }
                var stale = new List<string>();
                foreach (var id in devices.Keys)
                {
                    if (!seen.Contains(id))
                    {
                        stale.Add(id);
                    }
                }
                foreach (var id in stale)
                {
                    devices.Remove(id);
                }
            }
            catch
            {
            }
        }

        void HookDefaultDeviceIfNeeded()
        {
            string currentId = null;
            MMDevice current = null;
            try
            {
                current = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                currentId = current.ID;
            }
            catch
            {
                current = null;
                currentId = null;
            }

            string watchedId = null;
            try
            {
                if (watchedDefault != null)
                {
                    watchedId = watchedDefault.ID;
                }
            }
            catch
            {
                watchedId = null;
                UnhookDefault();
            }

            if (current == null)
            {
                if (watchedDefault != null)
                {
                    UnhookDefault();
                }
                return;
            }

            if (watchedDefault != null && currentId == watchedId)
            {
                return;
            }

            UnhookDefault();
            watchedDefault = current;
            try
            {
                watchedDefault.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
                if (!string.IsNullOrEmpty(currentId) && !devices.ContainsKey(currentId))
                {
                    devices[currentId] = watchedDefault;
                }
                ReportCurrent(watchedDefault);
            }
            catch
            {
                watchedDefault = null;
            }
        }

        void UnhookDefault()
        {
            if (watchedDefault == null)
            {
                return;
            }
            try
            {
                watchedDefault.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
            }
            catch
            {
            }
            watchedDefault = null;
        }

        void OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (IsLocked)
            {
                Enforce();
            }
            var handler = CurrentVolumeChanged;
            if (handler != null)
            {
                handler(Convert.ToInt16(data.MasterVolume * 100));
            }
        }

        void ReportCurrent(MMDevice device)
        {
            try
            {
                var handler = CurrentVolumeChanged;
                if (handler != null)
                {
                    handler(Convert.ToInt16(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100));
                }
            }
            catch
            {
            }
        }
    }
}
