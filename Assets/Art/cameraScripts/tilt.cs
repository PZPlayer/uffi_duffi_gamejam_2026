using UnityEngine;

public class HandRigRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handRig;        // Камера рук (объект который нужно наклонять)
    [SerializeField] private Transform mainCamera;     // Главная камера (для Cinemachine)

    [Header("Rotation Settings (Degrees)")]
    [SerializeField] private float forwardTilt = 15f;   // Наклон вперёд (W)
    [SerializeField] private float backTilt = -15f;     // Наклон назад (S)
    [SerializeField] private float leftTilt = -15f;     // Наклон влево (A)
    [SerializeField] private float rightTilt = 15f;     // Наклон вправо (D)

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 8f;    // Скорость интерполяции
    [SerializeField] private float returnSpeed = 5f;    // Скорость возврата

    [Header("Movement Settings")]
    [SerializeField] private Transform player;          // Персонаж
    [SerializeField] private float minSpeed = 0.1f;     // Минимальная скорость для наклона

    private Quaternion originalRotation;
    private Vector3 lastPlayerPosition;
    private Vector3 currentTiltAngles;
    private Vector3 targetTiltAngles;
    private bool isCinemachinePresent;

    void Start()
    {
        // Если handRig не назначен, используем этот объект
        if (handRig == null)
            handRig = transform;

        // Сохраняем исходный поворот
        originalRotation = handRig.localRotation;

        // Если главная камера не назначена, ищем её
        if (mainCamera == null)
        {
            mainCamera = Camera.main?.transform;
            if (mainCamera == null)
            {
                GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
                if (cam != null) mainCamera = cam.transform;
            }
        }

        // Проверяем наличие Cinemachine
        isCinemachinePresent = FindObjectOfType<Cinemachine.CinemachineBrain>() != null;
        if (isCinemachinePresent)
        {
            Debug.Log("Обнаружен Cinemachine, скрипт настроен для работы с ним");
        }

        // Поиск персонажа
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player != null)
            lastPlayerPosition = player.position;
    }

    void LateUpdate()  // Используем LateUpdate для работы после Cinemachine
    {
        if (player == null) return;

        // Получаем движение персонажа
        Vector3 playerMovement = player.position - lastPlayerPosition;
        lastPlayerPosition = player.position;

        // Рассчитываем целевые углы
        if (playerMovement.magnitude > minSpeed)
        {
            // Получаем направление движения относительно камеры
            Vector3 cameraForward = mainCamera != null ? mainCamera.forward : Vector3.forward;
            Vector3 cameraRight = mainCamera != null ? mainCamera.right : Vector3.right;

            // Проецируем движение на плоскость камеры
            Vector3 forwardMovement = Vector3.Project(playerMovement, cameraForward);
            Vector3 rightMovement = Vector3.Project(playerMovement, cameraRight);

            float forwardAmount = Vector3.Dot(forwardMovement, cameraForward);
            float rightAmount = Vector3.Dot(rightMovement, cameraRight);

            float targetTiltX = 0f;  // Наклон вперёд/назад
            float targetTiltZ = 0f;  // Наклон влево/вправо

            // Движение вперёд/назад
            if (Mathf.Abs(forwardAmount) > 0.1f)
            {
                if (forwardAmount > 0)
                    targetTiltX = forwardTilt;
                else
                    targetTiltX = backTilt;
            }

            // Движение влево/вправо
            if (Mathf.Abs(rightAmount) > 0.1f)
            {
                if (rightAmount > 0)
                    targetTiltZ = rightTilt;
                else
                    targetTiltZ = leftTilt;
            }

            targetTiltAngles = new Vector3(targetTiltX, 0, targetTiltZ);
        }
        else
        {
            targetTiltAngles = Vector3.zero;
        }

        // Плавно изменяем углы (разная скорость для наклона и возврата)
        float currentSpeed = (targetTiltAngles == Vector3.zero) ? returnSpeed : smoothSpeed;
        currentTiltAngles = Vector3.Lerp(currentTiltAngles, targetTiltAngles, currentSpeed * Time.deltaTime);

        // Применяем поворот к камере рук
        if (handRig != null)
        {
            handRig.localRotation = originalRotation * Quaternion.Euler(currentTiltAngles.x, currentTiltAngles.y, currentTiltAngles.z);
        }
    }
}