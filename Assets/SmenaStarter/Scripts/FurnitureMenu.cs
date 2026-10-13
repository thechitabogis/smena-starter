using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SmenaStarter
{
    public class FurnitureMenu : MonoBehaviour
    {
        VrPlayerLocomotion player;
        FurnitureCatalog catalog;
        Canvas canvas;
        Camera menuCamera;
        bool building;

        void Awake()
        {
            player = GetComponent<VrPlayerLocomotion>();
            catalog = Resources.Load<FurnitureCatalog>("FurnitureCatalog");
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(EnsureMenu());
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            StartCoroutine(EnsureMenu());
        }

        IEnumerator EnsureMenu()
        {
            if (building)
                yield break;
            building = true;
            // QuestClientRig creates its camera after scene load.
            yield return null;
            yield return null;
            var camera = Camera.main;
            if (camera != null && (canvas == null || menuCamera != camera))
                BuildMenu(camera);

            if (canvas != null)
                canvas.gameObject.SetActive(SceneManager.GetActiveScene().name == SmenaNetworkManager.RoomScene);
            building = false;
        }

        void Update()
        {
            if (canvas != null && canvas.gameObject.activeSelf !=
                (SceneManager.GetActiveScene().name == SmenaNetworkManager.RoomScene))
                canvas.gameObject.SetActive(SceneManager.GetActiveScene().name == SmenaNetworkManager.RoomScene);
            if (canvas == null && !building)
                StartCoroutine(EnsureMenu());
        }

        void BuildMenu(Camera camera)
        {
            if (catalog == null || catalog.items == null)
            {
                Debug.LogError("Smena: не найден Resources/FurnitureCatalog.asset.");
                return;
            }

            EnsureEventSystem();
            if (canvas != null)
                Destroy(canvas.gameObject);

            var root = new GameObject("FurnitureMenuCanvas", typeof(RectTransform), typeof(Canvas));
            canvas = root.GetComponent<Canvas>();
            menuCamera = camera;
            var xr = Application.platform == RuntimePlatform.Android;
            canvas.renderMode = xr ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = camera;
            canvas.sortingOrder = 50;

            var rect = root.GetComponent<RectTransform>();
            if (xr)
            {
                rect.SetParent(camera.transform, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(580f, 920f);
                rect.localPosition = new Vector3(-0.6f, -0.05f, 1.4f);
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one * 0.001f;
                root.AddComponent<TrackedDeviceGraphicRaycaster>();
            }
            else
            {
                root.AddComponent<GraphicRaycaster>();
            }

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            if (!xr)
            {
                panelRect.anchorMin = new Vector2(0f, 1f);
                panelRect.anchorMax = new Vector2(0f, 1f);
                panelRect.pivot = new Vector2(0f, 1f);
                panelRect.anchoredPosition = new Vector2(18f, -18f);
                panelRect.sizeDelta = new Vector2(580f, 920f);
            }
            panel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.13f, 0.94f);

            var title = MakeText(panel.transform, "МЕБЕЛЬ — выбери предмет", 27, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(-20f, 48f);

            var scrollGo = new GameObject("FurnitureList", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(panel.transform, false);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(12f, 12f);
            scrollRect.offsetMax = new Vector2(-12f, -62f);
            scrollGo.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.2f, 1f);
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(6f, 6f);
            viewportRect.offsetMax = new Vector2(-6f, -6f);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = xr ? 40f : 30f;

            for (var i = 0; i < catalog.items.Count; i++)
            {
                var index = i;
                var entry = catalog.items[index];
                if (entry == null || entry.prefab == null)
                    continue;
                var buttonGo = new GameObject(entry.displayName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                buttonGo.transform.SetParent(content.transform, false);
                buttonGo.GetComponent<Image>().color = new Color(0.96f, 0.77f, 0.12f, 1f);
                var element = buttonGo.GetComponent<LayoutElement>();
                element.minHeight = 52f;
                element.preferredHeight = 58f;
                var button = buttonGo.GetComponent<Button>();
                button.targetGraphic = buttonGo.GetComponent<Image>();
                MakeText(buttonGo.transform, entry.displayName, 20, TextAnchor.MiddleCenter).color = Color.black;
                button.onClick.AddListener(() => Spawn(index));
            }
        }

        void Spawn(int index)
        {
            if (player != null && player.isLocalPlayer && NetworkClient.isConnected)
                player.CmdSpawnFurniture(index);
        }

        static Text MakeText(Transform parent, string value, int size, TextAnchor alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 2f);
            rect.offsetMax = new Vector2(-8f, -2f);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.supportRichText = false;
            return text;
        }

        static void EnsureEventSystem()
        {
            var eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();

            var oldInput = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldInput != null)
            {
                oldInput.enabled = false;
                Destroy(oldInput);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                var input = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (input != null)
                    Destroy(input);
                if (eventSystem.GetComponent<XRUIInputModule>() == null)
                    eventSystem.gameObject.AddComponent<XRUIInputModule>();
            }
            else if (eventSystem.GetComponent<XRUIInputModule>() == null)
            {
                var input = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (input == null)
                    input = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                input.AssignDefaultActions();
            }
        }
    }
}
