using Jam.Effects;
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
        [SerializeField] private List<PickDraft> _firstPickDrafts;
        [SerializeField] private List<PickDraft> _secondPickDrafts;
        [SerializeField] private GameObject _descriptionCardPrefab;
        [SerializeField] private GameObject _cardsContainer;
        [SerializeField] private PlayerEffectHandler _handler;
        [SerializeField] private List<IdleEffect> _availableEffects;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Settings")]
        [SerializeField] private float _cardSpawnDelay = 0.2f;

        private List<IdleEffect> _oldEffects = new List<IdleEffect>();
        private Animator _animator;
        private int _currentDraftIndex = 0;
        private List<IdleEffect> _selectedEffects = new List<IdleEffect>();
        private List<EffectDescriptionCard> _activeCards = new List<EffectDescriptionCard>();
        private bool _isPicking = false;
        private bool _isSecondPick = false;

        private List<PickDraft> CurrentPickDrafts => _isSecondPick ? _secondPickDrafts : _firstPickDrafts;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (_cardsContainer == null)
                _cardsContainer = gameObject;

            StartPicking();
        }

        public void ChangePickingMethod()
        {
            _isSecondPick = true;
        }

        public void StartPicking()
        {
            if (CurrentPickDrafts == null || CurrentPickDrafts.Count == 0)
            {
                Debug.LogWarning("No pick drafts available!");
                return;
            }

            _onCardPickStart?.Invoke();
            _isPicking = true;
            _currentDraftIndex = 0;
            _selectedEffects.Clear();
            ClearCards();
            _animator.SetTrigger("Pick");
            ShowNextDraft();
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

                if (count == 1 && _isSecondPick)
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
        }

        private void ClearCards()
        {
            foreach (var card in _activeCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _activeCards.Clear();
        }

        public void Pick(IdleEffect selectedEffect)
        {
            if (!_isPicking) return;

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
                _handler.AddEffect(effect, JsonUtility.ToJson(effect));
            }

            _selectedEffects.Clear();
            gameObject.SetActive(false);
        }

        public void PickedVariantSend(IdleEffect effect)
        {
            if (_selectedEffects.Contains(effect)) return;

            Pick(effect);
        }

        public List<PickDraft> GetPickDrafts() => CurrentPickDrafts;

        private void OnDisable()
        {
            ClearCards();
        }
    }
}