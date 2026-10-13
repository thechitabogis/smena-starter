using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmenaStarter
{
    [CreateAssetMenu(menuName = "Smena/Furniture Catalog")]
    public class FurnitureCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string displayName;
            public GameObject prefab;
        }

        public List<Entry> items = new List<Entry>();
    }
}
