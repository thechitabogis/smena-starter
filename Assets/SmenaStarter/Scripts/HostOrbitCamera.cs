using UnityEngine;

namespace SmenaStarter
{
    public class HostOrbitCamera : MonoBehaviour
    {
        public Transform lookAt;
        public float distance = 8f;
        public float height = 4f;

        void LateUpdate()
        {
            if (lookAt == null)
                return;
            var target = lookAt.position + Vector3.up * height;
            transform.position = target - Vector3.forward * distance + Vector3.right * 2f;
            transform.LookAt(lookAt);
        }
    }
}
