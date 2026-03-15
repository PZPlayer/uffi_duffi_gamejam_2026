using UnityEngine;

namespace Jam.Items
{
    public class ItemsHandler : MonoBehaviour
    {
        public GameObject CurrentItem {  get; private set; }

        [SerializeField] private Transform _hand;
        [SerializeField] private GameObject _startWeapon;
        [SerializeField] private StarterAssets.StarterAssetsInputs input;
        
        private Item curItem;

        private void Start ()
        {
            input.OnUseClick += TryCallUse;
            input.OnSecUseClick += TryCallSecUse;
            WeaponSet(_startWeapon);
        }

        private void TryCallUse()
        {
            if (curItem != null && curItem.TryGetComponent<IUsable>(out IUsable usble) && usble != null)
            {
                usble.Use();
            }
        }

        private void TryCallSecUse()
        {
            if (curItem != null && curItem.TryGetComponent<ISecondUsable>(out ISecondUsable usble) && usble != null)
            {
                usble.UseSecond();
            }
        }


        public void WeaponSet(GameObject newWeapon)
        {
            if (newWeapon.TryGetComponent<Item>(out Item newItem))
            {
                if (newItem == null) return;
                
                if (CurrentItem != null)
                {
                    CurrentItem.SetActive(false);
                }

                CurrentItem = Instantiate(newWeapon, _hand.transform);
                curItem = CurrentItem.GetComponent<Item>();
            }
        }

        private void OnDestroy()
        {
            input.OnUseClick -= TryCallUse;
            input.OnSecUseClick -= TryCallSecUse;
        }
    }
}
