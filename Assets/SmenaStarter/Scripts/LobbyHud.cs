using System.Collections;
using System.Globalization;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SmenaStarter
{
    public class LobbyHud : MonoBehaviour
    {
        public SmenaNetworkManager manager;
        public InputField ipField;
        public Text ipHint;
        public Text countLabel;
        public Text statusLabel;
        public Button hostButton;
        public Button connectButton;
        public Button goRoomButton;
        public CanvasGroup panel;

        InputField baselineField;
        bool baselineArmed;

        void Start()
        {
            if (manager == null)
                manager = FindAnyObjectByType<SmenaNetworkManager>();

            var lan = LocalIpUtil.GetLanIPv4();
            var headset = Application.platform == RuntimePlatform.Android;
            if (ipHint != null)
                ipHint.text = headset
                    ? "IP компьютера — жёлтые кнопки снизу. Порт " + LocalIpUtil.Port
                    : "IP этого ПК: " + lan + "  порт " + LocalIpUtil.Port;
            if (ipField != null && string.IsNullOrWhiteSpace(ipField.text) && !headset)
                ipField.text = lan;

            hostButton?.onClick.AddListener(OnHost);
            connectButton?.onClick.AddListener(OnConnect);
            goRoomButton?.onClick.AddListener(OnGoRoom);

            if (goRoomButton != null)
                goRoomButton.gameObject.SetActive(false);

            if (Application.platform != RuntimePlatform.Android)
                BuildBaselineField();

            RefreshCount(CountConnected());
        }

        void Update()
        {
            RefreshCount(CountConnected());
            bool host = NetworkServer.active;
            if (goRoomButton != null)
                goRoomButton.gameObject.SetActive(host);
            if (!host)
                baselineArmed = false;
            else if (!baselineArmed && SceneManager.GetActiveScene().name == SmenaNetworkManager.LobbyScene)
                baselineArmed = PublishBaseline();
            if (statusLabel == null)
                return;
            if (NetworkClient.isConnected && SceneManager.GetActiveScene().name == SmenaNetworkManager.LobbyScene)
                statusLabel.text = host ? HostStatus() : FloorCalibrator.ClientStatus;
            else if (NetworkClient.isConnected)
                statusLabel.text = host
                    ? "ХОСТ. Жди шлем, потом «В комнату»."
                    : "КЛИЕНТ. Жди, пока хост откроет комнату.";
            else
                statusLabel.text = "Не в сети. Host на ПК или Connect со шлема.";
        }

        void BuildBaselineField()
        {
            if (statusLabel == null)
                return;
            var font = statusLabel.font;
            var labelGo = new GameObject("BaselineLabel");
            labelGo.transform.SetParent(transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.sizeDelta = new Vector2(420, 48);
            labelRt.anchoredPosition = new Vector2(-250, -230);
            var label = labelGo.AddComponent<Text>();
            label.font = font;
            label.fontSize = 22;
            label.alignment = TextAnchor.MiddleRight;
            label.color = Color.white;
            label.text = "Длина A→B, м";

            var fieldGo = new GameObject("BaselineField");
            fieldGo.transform.SetParent(transform, false);
            var fieldRt = fieldGo.AddComponent<RectTransform>();
            fieldRt.sizeDelta = new Vector2(220, 48);
            fieldRt.anchoredPosition = new Vector2(80, -230);
            var image = fieldGo.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.92f);
            baselineField = fieldGo.AddComponent<InputField>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(fieldGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 4);
            textRt.offsetMax = new Vector2(-10, -4);
            var text = textGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 24;
            text.color = Color.black;
            text.supportRichText = false;
            baselineField.textComponent = text;
            baselineField.text = "2.750";
            baselineField.onEndEdit.AddListener(_ => baselineArmed = PublishBaseline());
        }

        bool PublishBaseline()
        {
            var cal = RoomCalibration.FindHost();
            if (cal == null)
                return false;
            var meters = ReadBaseline();
            if (meters < 0.2f)
                return false;
            if (cal.HasPoints && Mathf.Abs(cal.Baseline - meters) < 0.0005f)
                return true;
            cal.ServerSetBaseline(meters);
            return true;
        }

        float ReadBaseline()
        {
            if (baselineField == null)
                return 2.75f;
            var text = baselineField.text.Trim().Replace(',', '.');
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var meters))
                return -1f;
            return meters;
        }

        string HostStatus()
        {
            var cal = RoomCalibration.FindHost();
            if (cal == null || !cal.HasPoints)
                return "Впиши длину наклеек A→B и нажми HOST.";
            if (!RoomCalibration.AnyHeadset())
                return "Базис " + cal.Baseline.ToString("0.000") + " м. Шлемов нет. Можно в комнату.";
            if (RoomCalibration.HeadsetsReady())
                return "Шлемы на одном базисе. Можно в комнату.";
            return "Базис " + cal.Baseline.ToString("0.000")
                + " м, A в нуле, B по +Z. Шлемы: "
                + RoomCalibration.ReadyCount() + "/" + RoomCalibration.HeadsetCount() + ".";
        }

        static int CountConnected()
        {
            if (NetworkServer.active)
            {
                var n = 0;
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn != null && conn.identity != null)
                        n++;
                }
                return n;
            }

            if (!NetworkClient.isConnected)
                return 0;

            var count = 0;
            foreach (var identity in NetworkClient.spawned.Values)
            {
                if (identity != null && identity.sceneId == 0)
                    count++;
            }
            return count;
        }

        void RefreshCount(int n)
        {
            if (countLabel != null)
                countLabel.text = "Игроков в лобби: " + n;
        }

        void OnHost()
        {
            if (manager == null)
                return;
            manager.networkAddress = LocalIpUtil.GetLanIPv4();
            manager.StartHost();
        }

        void OnConnect()
        {
            if (manager == null)
                return;
            var ip = ipField != null ? ipField.text.Trim() : "";
            if (string.IsNullOrEmpty(ip))
                return;
            manager.networkAddress = ip;
            manager.StartClient();
        }

        void OnGoRoom()
        {
            if (manager == null || !manager.CanEnterRoom())
                return;
            manager.HostGoToRoom();
            if (panel != null)
                StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut()
        {
            if (panel == null)
                yield break;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                panel.alpha = 1f - t / 0.35f;
                yield return null;
            }

            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
    }
}
