using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Jam.UI
{
    public class EndGamePanel : MonoBehaviour
    {
        #region Fields
        [Header("UI Elements")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI mainTitleText;
        [SerializeField] private TextMeshProUGUI subTitleText;

        [Header("Settings")]
        [SerializeField] private string winMessage = "ПОБЕДА!";
        [SerializeField] private string loseMessage = "ПОРАЖЕНИЕ";
        [SerializeField] private string winSubTitleText;
        [SerializeField] private string loseSubTitleText;
        [SerializeField] private Color winColor = Color.green;
        [SerializeField] private Color loseColor = Color.red;

        [Header("Events")]
        public UnityEvent OnRestartPressed;
        public UnityEvent OnMenuPressed;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Скрываем панель при старте, если забыли выключить в эдиторе
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Отображает панель завершения игры.
        /// </summary>
        /// <param name="isWin">True - победа, False - поражение.</param>
        /// <param name="additionalInfo">Дополнительный текст (например, счет или причина).</param>
        public void ShowResult(bool isWin, string additionalInfo = "")
        {
            panelRoot.SetActive(true);

            // Настройка основного текста
            mainTitleText.text = isWin ? winMessage : loseMessage;
            mainTitleText.color = isWin ? winColor : loseColor;

            // Настройка вспомогательного текста
            if (subTitleText != null)
            {
                subTitleText.text = additionalInfo;
            }
        }

        public void ShowResult(bool isWin)
        {
            panelRoot.SetActive(true);
            mainTitleText.text = isWin ? winMessage : loseMessage;
            mainTitleText.color = isWin ? winColor : loseColor;
            subTitleText.text = isWin ? winMessage : loseMessage;
        }

        // Методы для кнопок (вызываются через OnClick в UI)
        public void RestartButton()
        {
            OnRestartPressed?.Invoke();
        }

        public void MenuButton()
        {
            OnMenuPressed?.Invoke();
        }
        #endregion
    }
}
