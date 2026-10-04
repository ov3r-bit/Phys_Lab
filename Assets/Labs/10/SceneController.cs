using UnityEngine;
using UnityEngine.InputSystem;
public class SceneController : MonoBehaviour
{
    [Header("Камера")]
    public Camera cam;

    [Header("Позиции обзора (мировые координаты)")]
    public Vector3 posAll   = new Vector3(  0f, 18f, -14f);
    public Vector3 posTask1 = new Vector3(-12f,  6f,  -8f);
    public Vector3 posTask2 = new Vector3(  0f,  6f,  -8f);
    public Vector3 posTask3 = new Vector3( 12f,  6f,  -8f);

    [Header("Точки, на которые смотрит камера")]
    public Vector3 lookAll   = new Vector3(  0f, 1f, 0f);
    public Vector3 lookTask1 = new Vector3(-12f, 1f, 0f);
    public Vector3 lookTask2 = new Vector3(  0f, 1f, 0f);
    public Vector3 lookTask3 = new Vector3( 12f, 1f, 0f);

    [Header("Сглаживание")]
    [Tooltip("Сколько секунд камера едет к цели")]
    public float moveDuration = 0.6f;

    private Vector3 startPos;
    private Vector3 startLook;
    private Vector3 targetPos;
    private Vector3 targetLook;

    private float moveTimer = 1f;   // 1 = движение завершено

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Начальное состояние — общий вид
        targetPos  = posAll;
        targetLook = lookAll;

        startPos  = targetPos;
        startLook = targetLook;

        ApplyCamera(1f); // мгновенно поставить камеру в общий вид
    }

    void Update()
    {
        // --- Переключение цели ---
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.digit0Key.wasPressedThisFrame) SetTarget(posAll,   lookAll);
            if (kb.digit1Key.wasPressedThisFrame) SetTarget(posTask1, lookTask1);
            if (kb.digit2Key.wasPressedThisFrame) SetTarget(posTask2, lookTask2);
            if (kb.digit3Key.wasPressedThisFrame) SetTarget(posTask3, lookTask3);
        }

        // --- Анимация перелёта с гарантированным окончанием ---
        if (moveTimer < 1f)
        {
            moveTimer += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(moveTimer));
            ApplyCamera(t);
        }
        else
        {
            // Даже после завершения держим камеру точно в цели,
            // чтобы не было накопления ошибок
            ApplyCamera(1f);
        }
    }

    /// <summary>Задать новую цель перелёта.</summary>
    void SetTarget(Vector3 pos, Vector3 look)
    {
        // Стартуем из ТЕКУЩЕГО положения камеры, а не из прошлой цели —
        // это убирает рывки при быстром переключении
        startPos  = cam.transform.position;
        startLook = GetCurrentLookPoint();

        targetPos  = pos;
        targetLook = look;

        moveTimer = 0f;
    }

    /// <summary>Точка, куда камера смотрит сейчас (на расстоянии 10 м).</summary>
    Vector3 GetCurrentLookPoint()
    {
        return cam.transform.position + cam.transform.forward * 10f;
    }

    /// <summary>Интерполяция позиции и поворота. t = 0 — старт, t = 1 — цель.</summary>
    void ApplyCamera(float t)
    {
        Vector3 pos  = Vector3.Lerp(startPos,  targetPos,  t);
        Vector3 look = Vector3.Lerp(startLook, targetLook, t);

        cam.transform.position = pos;

        Vector3 dir = look - pos;
        if (dir.sqrMagnitude > 0.0001f)
            cam.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}