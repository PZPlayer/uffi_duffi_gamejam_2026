using UnityEngine;

namespace Jam.Items
{
    public abstract class Item : MonoBehaviour
    {
        [SerializeField] protected ItemInfo _itemInfo;
    }
}

