using UnityEngine;

namespace Jam.Items
{
    public abstract class Item : MonoBehaviour
    {
        [SerializeField] protected ItemInfo _itemInfo;
        public ItemInfo ItemInformation => _itemInfo;

        [field: SerializeField] public GameObject Owner { get; set; }
    }
}