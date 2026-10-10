using UnityEngine;

namespace SmenaStarter
{
    public class IdeaSlot : MonoBehaviour
    {
        public string slotName = "Слот идеи";

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.78f, 0.1f, 0.35f);
            Gizmos.DrawCube(transform.position + Vector3.up * 0.05f, new Vector3(0.8f, 0.1f, 0.8f));
        }
    }
}
