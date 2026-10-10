using UnityEngine;

namespace SmenaStarter
{
    public class PlatformRigSwitcher : MonoBehaviour
    {
        public GameObject pcCameraRoot;
        public GameObject xrOriginRoot;

        void Awake()
        {
            bool quest = Application.platform == RuntimePlatform.Android;
            if (pcCameraRoot != null)
                pcCameraRoot.SetActive(!quest);
            if (xrOriginRoot != null)
                xrOriginRoot.SetActive(quest);
        }
    }
}
