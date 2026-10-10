using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmenaStarter
{
    /// <summary>
    /// Локальный якорь комнаты. На хосте он в нуле. На шлеме это сдвиг и поворот,
    /// посчитанные в лобби. Сетевые позы остаются в координатах хоста.
    /// </summary>
    public static class RoomFrame
    {
        static Transform anchor;
        static int playersAfterFrame = int.MaxValue;
        static bool active;
        static Vector3 position;
        static Quaternion rotation = Quaternion.identity;

        public static bool Active => active;
        public static Transform Anchor => anchor;

        public static void Set(Vector3 worldPosition, Quaternion worldRotation)
        {
            active = true;
            position = worldPosition;
            position.y = 0f;
            rotation = Quaternion.Euler(0f, worldRotation.eulerAngles.y, 0f);
        }

        public static void Clear()
        {
            active = false;
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        public static Vector3 SharedToWorld(Vector3 shared)
        {
            if (anchor == null)
                return shared;
            return anchor.TransformPoint(shared);
        }

        public static Quaternion SharedToWorld(Quaternion shared)
        {
            if (anchor == null)
                return shared;
            return anchor.rotation * shared;
        }

        public static void Apply(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;
            if (scene.name != SmenaNetworkManager.RoomScene)
                return;

            if (anchor == null)
            {
                var go = new GameObject("worldAnchor");
                SceneManager.MoveGameObjectToScene(go, scene);
                anchor = go.transform;
                var posePos = active ? position : Vector3.zero;
                var poseRot = active ? rotation : Quaternion.identity;
                anchor.SetPositionAndRotation(posePos, poseRot);
                playersAfterFrame = Time.frameCount + 1;
            }

            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root.transform == anchor || root.transform.IsChildOf(anchor))
                    continue;
                if (root.GetComponent<NetworkManager>() != null)
                    continue;
                root.transform.SetParent(anchor, false);
            }

            if (Time.frameCount >= playersAfterFrame)
                ParentPlayers();
        }

        static void ParentPlayers()
        {
            var list = new List<NetworkIdentity>();
            if (NetworkServer.active)
            {
                foreach (var identity in NetworkServer.spawned.Values)
                    list.Add(identity);
            }
            else if (NetworkClient.active)
            {
                foreach (var identity in NetworkClient.spawned.Values)
                    list.Add(identity);
            }

            for (var i = 0; i < list.Count; i++)
            {
                var identity = list[i];
                if (identity == null || identity.sceneId != 0)
                    continue;
                if (identity.transform.IsChildOf(anchor))
                    continue;
                identity.transform.SetParent(anchor, false);
            }
        }
    }
}
