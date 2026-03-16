using UnityEngine;

namespace Jam.Items
{
    public class ItemsHandler : MonoBehaviour
    {
        public GameObject CurrentItem { get; private set; }

        [SerializeField] private Transform _hand;
        [SerializeField] private GameObject _startWeapon;
        [SerializeField] private GameObject _ownerWeapon;
        [SerializeField] private StarterAssets.StarterAssetsInputs _input;

        private Item _curItem;

        private void Start()
        {
            _input.OnUseClick += TryCallUse;
            _input.OnSecUseClick += TryCallSecUse;
            WeaponSet(_startWeapon);
        }

        private void TryCallUse()
        {
            if (_curItem != null && _curItem.TryGetComponent<IUsable>(out IUsable usable) && usable != null)
            {
                usable.Use();
            }
        }

        private void TryCallSecUse()
        {
            if (_curItem != null && _curItem.TryGetComponent<ISecondUsable>(out ISecondUsable usable) && usable != null)
            {
                usable.UseSecond();
            }
        }

        public void WeaponSet(GameObject newWeapon)
        {
            if (!newWeapon.TryGetComponent<Item>(out _)) return;

            if (CurrentItem != null) Destroy(CurrentItem);

            CurrentItem = Instantiate(newWeapon, _hand.transform);
            _curItem = CurrentItem.GetComponent<Item>();

            if (_curItem != null)
            {
                _curItem.Owner = _ownerWeapon != null ? _ownerWeapon : this.transform.root.gameObject;
            }
        }

        private void OnDestroy()
        {
            _input.OnUseClick -= TryCallUse;
            _input.OnSecUseClick -= TryCallSecUse;
        }
    }
}