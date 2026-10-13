using System.Collections;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmenaStarter
{
    public class SmenaNetworkManager : NetworkManager
    {
        public const string LobbyScene = "00_Lobby";
        public const string RoomScene = "01_Room";

        [Header("Smena")]
        public GameObject hostMarkerPrefab;

        [Scene]
        public string roomScene;

        public override void Awake()
        {
            base.Awake();
            var kcp = GetComponent<KcpTransport>();
            if (kcp != null)
                kcp.Port = (ushort)LocalIpUtil.Port;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            FurnitureNetworkBootstrap.RegisterSpawnHandler();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var isListenHost = conn.connectionId == 0 && NetworkServer.localConnection == conn;
            var prefab = isListenHost && hostMarkerPrefab != null ? hostMarkerPrefab : playerPrefab;
            if (prefab == null)
            {
                Debug.LogError("Smena: playerPrefab не назначен. Меню Smena → Build Starter Scenes.");
                return;
            }

            Transform start = GetStartPosition();
            Vector3 pos = start != null ? start.position : Vector3.up;
            Quaternion rot = start != null ? start.rotation : Quaternion.identity;
            GameObject player = Instantiate(prefab, pos, rot);
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            if (!IsRoom(sceneName))
                return;

            RoomFrame.Apply(SceneManager.GetActiveScene());
            StartCoroutine(ReapplyRoomFrame());
        }

        static bool IsRoom(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
            return sceneName == RoomScene
                || sceneName.EndsWith("/" + RoomScene + ".unity")
                || sceneName.EndsWith("\\" + RoomScene + ".unity");
        }

        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
            RoomFrame.Apply(SceneManager.GetActiveScene());
            StartCoroutine(ReapplyRoomFrame());
        }

        IEnumerator ReapplyRoomFrame()
        {
            yield return null;
            RoomFrame.Apply(SceneManager.GetActiveScene());
        }

        public bool CanEnterRoom()
        {
            if (!RoomCalibration.AnyHeadset())
                return true;
            return RoomCalibration.HeadsetsReady();
        }

        public void HostGoToRoom()
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("Smena: «В комнату» жмёт только хост.");
                return;
            }

            if (!CanEnterRoom())
            {
                Debug.LogWarning("Smena: комната закрыта, пока шлемы не отметят две точки.");
                return;
            }

            var scene = string.IsNullOrWhiteSpace(roomScene) ? RoomScene : roomScene;
            ServerChangeScene(scene);
        }
    }
}
