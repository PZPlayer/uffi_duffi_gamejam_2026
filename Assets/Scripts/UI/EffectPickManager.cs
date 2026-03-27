using Jam.Effects;
using Jam.Items;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Jam.UI
{
    [Serializable]
    public struct PickDraft
    {
        public string Question;
        public int pickVariants;
    }

    [RequireComponent(typeof(Animator))]
    public class EffectPickManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEvent _onCardPickStart;
        [SerializeField] private UnityEvent _onCardPickEnd;
        [SerializeField] private UnityEvent _onBonusPick; // событие при выборе бонусной карты
        [SerializeField] private List<PickDraft> _firstPickDrafts;
        [SerializeField] private List<PickDraft> _secondPickDrafts;
        [SerializeField] private List<PickDraft> _bonusPickDrafts; // специальные бонусные драфты
        [SerializeField] private GameObject _descriptionCardPrefab;
        [SerializeField] private GameObject _cardsContainer;
        [SerializeField] private GameObject _cardsContainerForMenu;
        [SerializeField] private PlayerEffectHandler _handler;
        [SerializeField] private List<IdleEffect> _availableEffects;
        [SerializeField] private List<IdleEffect> _bonusEffects; // особые эффекты для бонусных карт
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Settings")]
        [SerializeField] private float _cardSpawnDelay = 0.2f;
        [SerializeField] private bool _useBonusPick = true; // включать ли бонусный выбор

        private List<IdleEffect> _oldEffects = new List<IdleEffect>();
        private Animator _animator;
        private int _currentDraftIndex = 0;
        private List<IdleEffect> _selectedEffects = new List<IdleEffect>();
        private List<EffectDescriptionCard> _activeCards = new List<EffectDescriptionCard>();
        private List<EffectDescriptionCard> _activeMenuCards = new List<EffectDescriptionCard>();
        private bool _isPicking = false;
        private bool _isSecondPick = false;
        private bool _isBonusPick = false; // флаг бонусного выбора
        private IdleEffect _bonusSelectedEffect = null;

        private List<PickDraft> CurrentPickDrafts => _isSecondPick ? _secondPickDrafts : _firstPickDrafts;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (_cardsContainer == null)
                _cardsContainer = gameObject;

            StartPicking();
        }

        public void AddBonus() => _useBonusPick = true;

        public void ChangePickingMethod()
        {
            _isSecondPick = true;
        }

        public void StartPicking()
        {
            // Проверяем, нужно ли показать бонусный выбор
            if (_useBonusPick)
            {
                StartBonusPicking();
                return;
            }

            // Обычный выбор
            if (CurrentPickDrafts == null || CurrentPickDrafts.Count == 0)
            {
                Debug.LogWarning("No pick drafts available!");
                return;
            }

            _onCardPickStart?.Invoke();
            _isPicking = true;
            _currentDraftIndex = 0;
            _selectedEffects.Clear();
            ClearCards(true);
            _animator.SetTrigger("Pick");
            ShowNextDraft();
        }

        private void StartBonusPicking()
        {
            _onCardPickStart?.Invoke();

            _isBonusPick = true;
            _isPicking = true;
            ClearCards();
            _animator.SetTrigger("Pick");

            PickDraft bonusDraft = _bonusPickDrafts[0];
            _titleText.text = bonusDraft.Question;
            SpawnBonusCards(bonusDraft.pickVariants);
        }

        private void SpawnBonusCards(int count)
        {
            ClearCards();

            for (int i = 0; i < count; i++)
            {
                IdleEffect selectedEffect = null;

                if (_bonusEffects != null && i < _bonusEffects.Count)
                {
                    selectedEffect = _bonusEffects[UnityEngine.Random.Range(0, _bonusEffects.Count)];
                    
                }

                _bonusEffects.Remove(selectedEffect);

                if (selectedEffect == null) continue;

                GameObject cardObj = Instantiate(_descriptionCardPrefab, _cardsContainer.transform);
                EffectDescriptionCard card = cardObj.GetComponent<EffectDescriptionCard>();

                if (card != null)
                {
                    Button btn = cardObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => OnBonusCardPicked(selectedEffect));
                    card.Initialize(selectedEffect, this, true);
                    _activeCards.Add(card);
                }
            }
        }

        private void OnBonusCardPicked(IdleEffect selectedEffect)
        {
            if (!_isBonusPick) return;

            _bonusSelectedEffect = selectedEffect;
            _onBonusPick?.Invoke();
            _isBonusPick = false;
            _useBonusPick = false;

            PlayerCard card = selectedEffect as PlayerCard;

            Camera.main.GetComponent<ItemsHandler>().WeaponSet(card.Weapon);
            
            StartPicking();
        }

        private void ShowNextDraft()
        {
            if (_currentDraftIndex >= CurrentPickDrafts.Count)
            {
                FinalizePicking();
                return;
            }

            PickDraft currentDraft = CurrentPickDrafts[_currentDraftIndex];
            _titleText.text = currentDraft.Question;
            SpawnCards(currentDraft.pickVariants);
        }

        private void SpawnCards(int count)
        {
            ClearCards();

            for (int i = 0; i < count; i++)
            {
                IdleEffect selectedEffect;

                if (count <= 2 && _isSecondPick)
                {
                    if (_oldEffects != null && _oldEffects.Count > 0)
                    {
                        selectedEffect = _oldEffects[UnityEngine.Random.Range(0, _oldEffects.Count)];
                    }
                    else
                    {
                        selectedEffect = _availableEffects[UnityEngine.Random.Range(0, _availableEffects.Count)];
                    }
                }
                else
                {
                    selectedEffect = _availableEffects[UnityEngine.Random.Range(0, _availableEffects.Count)];
                }

                GameObject cardObj = Instantiate(_descriptionCardPrefab, _cardsContainer.transform);
                EffectDescriptionCard card = cardObj.GetComponent<EffectDescriptionCard>();

                if (card != null)
                {
                    Button btn = cardObj.GetComponent<Button>();
                    if (!_selectedEffects.Contains(selectedEffect))
                    {
                        btn.onClick.AddListener(card.Pick);
                    }
                    else
                    {
                        btn.interactable = false;
                    }

                    card.Initialize(selectedEffect, this);
                    _activeCards.Add(card);
                }
            }

            int nonInteractbles = 0;

            foreach (var effect in _activeCards)
            {
                if (effect.GetComponent<Button>().interactable == false) nonInteractbles++;
            }

            if (nonInteractbles == _activeCards.Count) StartPicking();
        }

        private void ClearCards(bool clearAllCards = false)
        {
            foreach (var card in _activeCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }

            _activeCards.Clear();

            if (!clearAllCards) return;

            foreach (var card in _activeMenuCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }

            _activeMenuCards.Clear();
        }

        public void Pick(IdleEffect selectedEffect)
        {
            if (!_isPicking) return;
            if (_isBonusPick) return; // игнорируем обычный выбор во время бонуса

            _selectedEffects.Add(selectedEffect);
            _currentDraftIndex++;

            if (_currentDraftIndex < CurrentPickDrafts.Count)
            {
                ShowNextDraft();
            }
            else
            {
                FinalizePicking();
            }
        }

        private void FinalizePicking()
        {
            _onCardPickEnd?.Invoke();
            _isPicking = false;
            _animator.SetTrigger("Picked");
            _oldEffects = new List<IdleEffect>(_selectedEffects);

            foreach (var effect in _selectedEffects)
            {
                GameObject cardObj = Instantiate(_descriptionCardPrefab, _cardsContainerForMenu.transform);
                EffectDescriptionCard card = cardObj.GetComponent<EffectDescriptionCard>();
                card.GetComponent<Button>().enabled = false;
                card.GetComponent<Animator>().enabled = false;
                card.Initialize(effect, this);

                _activeMenuCards.Add(card);

                _handler.AddEffect(effect, JsonUtility.ToJson(effect));
            }

            _selectedEffects.Clear();
            gameObject.SetActive(false);
        }

        public void PickedVariantSend(IdleEffect effect)
        {
            if (_selectedEffects.Contains(effect)) return;
            if (_isBonusPick) return;

            Pick(effect);
        }

        public List<PickDraft> GetPickDrafts() => CurrentPickDrafts;

        private void OnDisable()
        {
            ClearCards();
        }
    }
}