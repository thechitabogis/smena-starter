using UnityEngine;
using UnityEngine.UI;

namespace SmenaStarter
{
    /// <summary>
    /// На Quest системная клавиатура из TouchScreenKeyboard не показывается:
    /// её открывает Meta XR SDK, которого в этом проекте нет.
    /// Цифры — обычные Button, их луч уже нажимает.
    /// </summary>
    public class QuestLobbyKeyboard : MonoBehaviour
    {
        const int MaxLength = 15;

        static readonly string[] Keys =
        {
            "1", "2", "3",
            "4", "5", "6",
            "7", "8", "9",
            ".", "0", "<"
        };

        InputField field;
        bool built;

        void Awake()
        {
            field = GetComponent<InputField>();
        }

        void Start()
        {
            if (built || field == null || Application.platform != RuntimePlatform.Android)
                return;

            field.readOnly = true;
            if (field.textComponent != null)
                field.textComponent.raycastTarget = false;
            BuildPad(field.textComponent != null ? field.textComponent.font : null);
            built = true;
        }

        void BuildPad(Font font)
        {
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            var root = new GameObject("IpKeypad", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);

            const float stepX = 152f;
            const float stepY = 80f;
            var size = new Vector2(140f, 70f);
            for (var i = 0; i < Keys.Length; i++)
            {
                var label = Keys[i];
                var col = i % 3;
                var row = i / 3;
                var pos = new Vector2((col - 1) * stepX, -225f - row * stepY);
                var button = MakeKey(root.transform, label, pos, size, font);
                var captured = label;
                button.onClick.AddListener(() => Press(captured));
            }
        }

        static Button MakeKey(Transform parent, string label, Vector2 pos, Vector2 size, Font font)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.96f, 0.77f, 0.12f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.text = label;
            text.raycastTarget = false;
            return button;
        }

        void Press(string key)
        {
            if (field == null)
                return;

            var text = field.text ?? "";
            if (key == "<")
            {
                if (text.Length > 0)
                    field.text = text.Substring(0, text.Length - 1);
                return;
            }

            if (text.Length >= MaxLength)
                return;
            if (key == "." && (text.Length == 0 || text[text.Length - 1] == '.'))
                return;
            field.text = text + key;
        }
    }
}
