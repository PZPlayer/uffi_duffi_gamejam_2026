using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Jam.Other
{
    [RequireComponent(typeof(Animator))]
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _loadingProcentage;
        [SerializeField] private Image _loadingBar;
        [SerializeField] private GameObject _preesAnyKey;

        private Animator anmtr;

        private float _loadingProgress;
        private Coroutine _sceneLoader;

        private void Awake()
        {
            anmtr = GetComponent<Animator>();
        }

        public void LoadScene(int sceneID)
        {
            anmtr.SetTrigger("Start");
            if (_sceneLoader == null) _sceneLoader = StartCoroutine(LoadSceneAsync(sceneID));
        }

        private void UpdateValues()
        {
            _loadingBar.fillAmount = _loadingProgress ;
            _loadingProcentage.text = _loadingProgress * 100 + " %";
        }
        private IEnumerator LoadSceneAsync(int sceneID)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                UpdateValues();
                float progress = operation.progress / 0.9f;
                _loadingProgress = progress;

                if (_loadingProgress >= 1)
                {
                    _preesAnyKey.SetActive(true);

                    if (Keyboard.current.anyKey.wasPressedThisFrame)
                    {
                        operation.allowSceneActivation = true;
                    }
                }

                yield return null;
            }
        }
    }
}
