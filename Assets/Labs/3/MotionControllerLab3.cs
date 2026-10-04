using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotionControllerLab3 : MonoBehaviour
{
    [Header("Объекты сцены")]
    public Transform movingObject;          // Сфера
    public LineRenderer trajectoryLine;     // Линия траектории

    [Header("Элементы UI (ввод)")]
    public TMP_Dropdown taskDropdown;       // Выбор задания (1 или 2)
    public TMP_InputField inputA;           // Коэф. A в a(t) = A + B*t
    public TMP_InputField inputB;           // Коэф. B
    public TMP_InputField inputC;           // Коэф. C (для ay = C + D*t, задание 2)
    public TMP_InputField inputD;           // Коэф. D (для ay)
    public TMP_InputField inputT1;          // Время начала действия ускорения
    public TMP_InputField inputT2;          // Конечное время (для пути)
    public TMP_InputField inputT3;          // Момент времени для скорости и ускорения
    public Button startButton;              // Кнопка "Старт"

    [Header("Элементы UI (вывод результатов)")]
    public TMP_Text resultPath;             // Путь за t2
    public TMP_Text resultSpeed;            // Скорость в t3
    public TMP_Text resultAccel;            // Ускорение в t3

    [Header("Элементы UI (текущие значения, опционально)")]
    public TMP_Text currentCoord;           // Текущая координата (x, y, z)
    public TMP_Text currentSpeed;           // Текущая скорость (модуль)
    public TMP_Text currentAccel;           // Текущее ускорение (модуль)

    // Параметры движения
    private float A, B, C, D, t1, t2, t3;
    private int task;                       // 1 или 2

    // Списки для траектории и значений
    private List<Vector3> positions = new List<Vector3>();
    private List<float> speeds = new List<float>();      // модуль скорости в каждой точке
    private List<float> accels = new List<float>();      // модуль ускорения в каждой точке
    private List<float> pathLengths = new List<float>(); // накопленный путь

    // Результаты
    private float totalPath = 0f;
    private float speedAtT3 = 0f;
    private float accelAtT3 = 0f;

    // Анимация
    private float timeElapsed = 0f;
    private bool isMoving = false;

    void Start()
    {
        startButton.onClick.AddListener(StartSimulation);
        // Настройка LineRenderer
        trajectoryLine.positionCount = 0;
        trajectoryLine.startWidth = 0.05f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.green;
        trajectoryLine.endColor = Color.green;
    }

    void Update()
    {
        if (isMoving)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= t2)
            {
                timeElapsed = t2;
                isMoving = false;
            }
            UpdateCurrentValues(timeElapsed);
        }
    }

    void StartSimulation()
    {
        // Считываем параметры из полей ввода
        float.TryParse(inputA.text, out A);
        float.TryParse(inputB.text, out B);
        float.TryParse(inputC.text, out C);
        float.TryParse(inputD.text, out D);
        float.TryParse(inputT1.text, out t1);
        float.TryParse(inputT2.text, out t2);
        float.TryParse(inputT3.text, out t3);
        task = taskDropdown.value + 1; // 1 или 2

        // Защита от некорректных значений
        if (t1 < 0) t1 = 0;
        if (t2 <= t1) t2 = t1 + 1f;
        if (t3 < t1 || t3 > t2) t3 = (t1 + t2) / 2f;

        // Очищаем предыдущие данные
        positions.Clear();
        speeds.Clear();
        accels.Clear();
        pathLengths.Clear();
        trajectoryLine.positionCount = 0;

        // Дискретизация времени с шагом 0.02 с
        float dt = 0.02f;
        float accumulatedPath = 0f;
        Vector3 prevPos = Vector3.zero;
        bool firstPoint = true;

        // Генерируем точки от 0 до t2 включительно
        for (float t = 0; t <= t2 + dt/2; t += dt)
        {
            float tClamped = Mathf.Min(t, t2);
            Vector3 pos;
            float speed, accel;
            ComputeMotion(tClamped, out pos, out speed, out accel);

            positions.Add(pos);
            speeds.Add(speed);
            accels.Add(accel);

            // Накопление пути (длина дуги) – интегрируем по отрезкам
            if (!firstPoint)
            {
                accumulatedPath += Vector3.Distance(pos, prevPos);
            }
            pathLengths.Add(accumulatedPath);
            prevPos = pos;
            firstPoint = false;

            // Запоминаем значения в момент t3 (ближайшая точка)
            if (Mathf.Abs(tClamped - t3) < dt/2)
            {
                speedAtT3 = speed;
                accelAtT3 = accel;
            }
        }

        totalPath = accumulatedPath; // путь за t2

        // Отображаем траекторию
        trajectoryLine.positionCount = positions.Count;
        trajectoryLine.SetPositions(positions.ToArray());

        // Выводим результаты
        resultPath.text = $"Путь за t2: {totalPath:F3}";
        resultSpeed.text = $"Скорость в t3: {speedAtT3:F3}";
        resultAccel.text = $"Ускорение в t3: {accelAtT3:F3}";

        // Сбрасываем анимацию
        timeElapsed = 0f;
        movingObject.position = positions[0];
        UpdateCurrentValues(0f);
        isMoving = true;
    }

    // Вычисление положения, модуля скорости и модуля ускорения в момент времени t
    void ComputeMotion(float t, out Vector3 pos, out float speed, out float accel)
    {
        // До t1 объект неподвижен
        if (t < t1)
        {
            pos = Vector3.zero;
            speed = 0f;
            accel = 0f;
            return;
        }

        float tau = t - t1; // время после начала действия ускорения

        if (task == 1) // 1D движение по оси X
        {
            // a(t) = A + B*t (здесь t – абсолютное время)
            float a_t = A + B * t;
            // Интегрируем скорость: v(t) = ∫_{t1}^{t} (A + B*τ) dτ
            float v = A * tau + 0.5f * B * (t * t - t1 * t1);
            // Интегрируем координату: x(t) = ∫_{t1}^{t} v(τ) dτ
            // Аналитическое выражение:
            float x = A * tau * tau / 2f + 0.5f * B * ((t * t * t - t1 * t1 * t1) / 3f - t1 * t1 * tau);
            pos = new Vector3(x, 0, 0);
            speed = Mathf.Abs(v);
            accel = Mathf.Abs(a_t);
        }
        else // Задание 2: 2D движение (x, y)
        {
            // ax = A + B*t, ay = C + D*t
            float ax_t = A + B * t;
            float ay_t = C + D * t;
            // Скорости:
            float vx = A * tau + 0.5f * B * (t * t - t1 * t1);
            float vy = C * tau + 0.5f * D * (t * t - t1 * t1);
            // Координаты:
            float x = A * tau * tau / 2f + 0.5f * B * ((t * t * t - t1 * t1 * t1) / 3f - t1 * t1 * tau);
            float y = C * tau * tau / 2f + 0.5f * D * ((t * t * t - t1 * t1 * t1) / 3f - t1 * t1 * tau);
            pos = new Vector3(x, y, 0);
            speed = Mathf.Sqrt(vx * vx + vy * vy);
            accel = Mathf.Sqrt(ax_t * ax_t + ay_t * ay_t);
        }
    }

    // Обновление текущих значений (координата, скорость, ускорение) в момент t
    void UpdateCurrentValues(float t)
    {
        int index = Mathf.FloorToInt(t / t2 * (positions.Count - 1));
        index = Mathf.Clamp(index, 0, positions.Count - 1);
        movingObject.position = positions[index];

        if (currentCoord != null)
        {
            Vector3 pos = positions[index];
            currentCoord.text = $"Текущая координата: ({pos.x:F3}, {pos.y:F3}, {pos.z:F3})";
        }
        if (currentSpeed != null && speeds.Count > index)
            currentSpeed.text = $"Текущая скорость: {speeds[index]:F3}";
        if (currentAccel != null && accels.Count > index)
            currentAccel.text = $"Текущее ускорение: {accels[index]:F3}";
    }
}