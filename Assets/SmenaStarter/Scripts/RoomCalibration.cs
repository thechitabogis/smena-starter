using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    /// <summary>
    /// Базис арены на HostMarker. Точка A — ноль сервера, точка B — вперёд на зашитую длину.
    /// Шлем присылает только измеренную длину. Позу якоря сервер не получает.
    /// </summary>
    public class RoomCalibration : NetworkBehaviour
    {
        public const float LengthTolerance = 0.15f;

        [SyncVar] float baselineMeters;
        [SyncVar(hook = nameof(OnBasis))] bool basisActive;

        readonly HashSet<int> ready = new HashSet<int>();

        public bool HasPoints => basisActive;
        public float Baseline => baselineMeters;
        public Vector3 PointA => Vector3.zero;
        public Vector3 PointB => Vector3.forward * baselineMeters;

        public static RoomCalibration FindHost()
        {
            return FindAnyObjectByType<RoomCalibration>(FindObjectsInactive.Exclude);
        }

        public void ServerSetBaseline(float meters)
        {
            if (!NetworkServer.active)
                return;
            if (meters < 0.2f || meters > 30f)
                return;
            ready.Clear();
            basisActive = false;
            baselineMeters = meters;
            basisActive = true;
        }

        public void ServerClear()
        {
            if (!NetworkServer.active)
                return;
            basisActive = false;
            ready.Clear();
        }

        public static void MarkReady(int connectionId, float measuredMeters)
        {
            var host = FindHost();
            if (host == null || !NetworkServer.active || !host.basisActive)
                return;
            if (Mathf.Abs(measuredMeters - host.baselineMeters) > LengthTolerance)
                return;
            host.ready.Add(connectionId);
        }

        public static bool IsReady(int connectionId)
        {
            var host = FindHost();
            return host != null && host.ready.Contains(connectionId);
        }

        public static bool AnyHeadset()
        {
            return HeadsetCount() > 0;
        }

        public static bool HeadsetsReady()
        {
            var host = FindHost();
            if (host == null || !host.basisActive)
                return false;
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (!IsHeadset(conn))
                    continue;
                if (!host.ready.Contains(conn.connectionId))
                    return false;
            }
            return true;
        }

        public static int ReadyCount()
        {
            var host = FindHost();
            return host != null ? host.ready.Count : 0;
        }

        public static int HeadsetCount()
        {
            if (!NetworkServer.active)
                return 0;
            var n = 0;
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (IsHeadset(conn))
                    n++;
            }
            return n;
        }

        public static float HostLength()
        {
            var host = FindHost();
            if (host == null || !host.basisActive)
                return 0f;
            return host.baselineMeters;
        }

        static bool IsHeadset(NetworkConnectionToClient conn)
        {
            return conn != null
                && conn.identity != null
                && conn.connectionId != 0
                && conn.identity.GetComponent<VrPlayerLocomotion>() != null;
        }

        void OnBasis(bool _, bool __)
        {
            FloorCalibrator.Invalidate();
        }
    }
}
