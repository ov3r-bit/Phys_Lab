using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotionControllerLab5 : MonoBehaviour
{
    [Header("Объекты сцены")]
    public Transform movingObject;          // Сфера
    public LineRenderer trajectoryLine;     // Линия траектории

    [Header("Элементы UI (ввод)")]
    public TMP_Dropdown taskDropdown;       // 0 - горизонт, 1 - под углом с V0, 2 - с ускорением
    public TMP_InputField inputH;           // Высота h
    public TMP_InputField inputV0;          // Начальная скорость (модуль)
    public TMP_InputField inputAngle;       // Угол α (градусы)
    public TMP_InputField inputA;           // Дополнительное ускорение (для задания 3)
    public TMP_InputField inputG;           // Ускорение свободного падения (по умолчанию 9.81)
    public Button startButton;

    [Header("Элементы UI (вывод)")]
    public TMP_Text resultText;             // Основной вывод результатов
    public TMP_Text currentCoord;           // Текущая позиция
    public TMP_Text currentSpeed;           // Текущая скорость (модуль)

    // Параметры
    private float h, V0, angleDeg, A, g = 9.81f;
    private int task;

    // Списки для траектории
    private List<Vector3> positions = new List<Vector3>();
    private List<float> speeds = new List<float>();   // модуль скорости в каждой точке
    private List<float> pathLengths = new List<float>(); // накопленный путь

    // Результаты
    private float flightTime = 0f;          // время полёта
    private float range = 0f;               // дальность (по горизонтали)
    private float speedAtLanding = 0f;      // скорость при приземлении (модуль)
    private float avgSpeed = 0f;            // средняя путевая скорость
    private float totalPath = 0f;           // полный путь

    // Анимация
    private float timeElapsed = 0f;
    private bool isMoving = false;

    void Start()
    {
        startButton.onClick.AddListener(StartSimulation);
        trajectoryLine.positionCount = 0;
        trajectoryLine.startWidth = 0.05f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.green;
        trajectoryLine.endColor = Color.green;
        inputG.text = "9.81"; // по умолчанию
    }

    void Update()
    {
        if (isMoving)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= flightTime)
            {
                timeElapsed = flightTime;
                isMoving = false;
            }
            UpdateCurrentValues(timeElapsed);
        }
    }

    void StartSimulation()
    {
        // Считываем параметры
        float.TryParse(inputH.text, out h);
        float.TryParse(inputV0.text, out V0);
        float.TryParse(inputAngle.text, out angleDeg);
        float.TryParse(inputA.text, out A);
        float.TryParse(inputG.text, out g);
        if (g <= 0) g = 9.81f;

        task = taskDropdown.value; // 0,1,2

        // Защита от отрицательных значений
        if (h < 0) h = 0;
        if (V0 < 0) V0 = 0;
        if (A < 0) A = 0;

        // Очистка
        positions.Clear();
        speeds.Clear();
        pathLengths.Clear();
        trajectoryLine.positionCount = 0;

        // Генерация точек траектории с шагом 0.02 с
        float dt = 0.02f;
        float time = 0f;
        Vector3 pos = new Vector3(0, h, 0); // начальная позиция (x=0, y=h, z=0)
        Vector3 prevPos = pos;
        float accumulatedPath = 0f;
        bool firstPoint = true;

        // В зависимости от задания выбираем модель движения
        if (task == 0) // Горизонтальный бросок
        {
            // vx = V0 (горизонтальная), vy = 0, ay = -g
            // Уравнения: x = V0*t, y = h - 0.5*g*t^2
            // Время полёта: t = sqrt(2h/g)
            flightTime = Mathf.Sqrt(2f * h / g);
            if (float.IsNaN(flightTime) || flightTime <= 0) flightTime = 0.1f;

            for (time = 0; time <= flightTime + dt/2; time += dt)
            {
                float t = Mathf.Min(time, flightTime);
                float x = V0 * t;
                float y = h - 0.5f * g * t * t;
                if (y < 0) y = 0;
                pos = new Vector3(x, y, 0);
                positions.Add(pos);
                float vx = V0;
                float vy = -g * t;
                float speed = Mathf.Sqrt(vx*vx + vy*vy);
                speeds.Add(speed);

                if (!firstPoint)
                    accumulatedPath += Vector3.Distance(pos, prevPos);
                prevPos = pos;
                firstPoint = false;
            }
            // Вычисляем результаты
            range = V0 * flightTime;
            float vx_land = V0;
            float vy_land = -g * flightTime;
            speedAtLanding = Mathf.Sqrt(vx_land*vx_land + vy_land*vy_land);
            totalPath = accumulatedPath;
            avgSpeed = totalPath / flightTime;

            resultText.text =
                $"Время полёта: {flightTime:F3} с\n" +
                $"Дальность: {range:F3} м\n" +
                $"Скорость при приземлении: {speedAtLanding:F3} м/с\n" +
                $"Средняя путевая скорость: {avgSpeed:F3} м/с\n" +
                $"Пройденный путь: {totalPath:F3} м";
        }
        else if (task == 1) // Бросок под углом с начальной скоростью V0
        {
            // Угол в радианы
            float alpha = angleDeg * Mathf.Deg2Rad;
            float vx0 = V0 * Mathf.Cos(alpha);
            float vy0 = V0 * Mathf.Sin(alpha);
            // Уравнения: x = vx0*t, y = h + vy0*t - 0.5*g*t^2
            // Время полёта: решаем квадратное уравнение: -0.5*g*t^2 + vy0*t + h = 0
            float discr = vy0*vy0 + 2f*g*h;
            if (discr < 0) discr = 0;
            flightTime = (vy0 + Mathf.Sqrt(discr)) / g;
            if (flightTime <= 0) flightTime = 0.1f;

            for (time = 0; time <= flightTime + dt/2; time += dt)
            {
                float t = Mathf.Min(time, flightTime);
                float x = vx0 * t;
                float y = h + vy0 * t - 0.5f * g * t * t;
                if (y < 0) y = 0;
                pos = new Vector3(x, y, 0);
                positions.Add(pos);
                float vx = vx0;
                float vy = vy0 - g * t;
                float speed = Mathf.Sqrt(vx*vx + vy*vy);
                speeds.Add(speed);

                if (!firstPoint)
                    accumulatedPath += Vector3.Distance(pos, prevPos);
                prevPos = pos;
                firstPoint = false;
            }
            range = vx0 * flightTime;
            float vx_land = vx0;
            float vy_land = vy0 - g * flightTime;
            speedAtLanding = Mathf.Sqrt(vx_land*vx_land + vy_land*vy_land);
            totalPath = accumulatedPath;
            avgSpeed = totalPath / flightTime;

            resultText.text =
                $"Время полёта: {flightTime:F3} с\n" +
                $"Дальность: {range:F3} м\n" +
                $"Скорость при приземлении: {speedAtLanding:F3} м/с\n" +
                $"Средняя путевая скорость: {avgSpeed:F3} м/с\n" +
                $"Пройденный путь: {totalPath:F3} м";
        }
        else if (task == 2) // Бросок с постоянным ускорением A (дополнительно к g)
        {
            // Угол в радианы
            float alpha = angleDeg * Mathf.Deg2Rad;
            // Ускорение: ax = A*cos(alpha), ay = A*sin(alpha) - g
            float ax = A * Mathf.Cos(alpha);
            float ay = A * Mathf.Sin(alpha) - g;
            // Начальная скорость 0 (объект покоится)
            // Уравнения: x = 0.5*ax*t^2, y = h + 0.5*ay*t^2
            // Время полёта: решаем y=0 => h + 0.5*ay*t^2 = 0 => t = sqrt(-2h/ay) при ay<0
            if (ay >= 0)
            {
                // Если ускорение направлено вверх, объект не упадёт (бесконечное время)
                flightTime = 10f; // ограничим
            }
            else
            {
                flightTime = Mathf.Sqrt(-2f * h / ay);
            }
            if (flightTime <= 0) flightTime = 0.1f;

            for (time = 0; time <= flightTime + dt/2; time += dt)
            {
                float t = Mathf.Min(time, flightTime);
                float x = 0.5f * ax * t * t;
                float y = h + 0.5f * ay * t * t;
                if (y < 0) y = 0;
                pos = new Vector3(x, y, 0);
                positions.Add(pos);
                float vx = ax * t;
                float vy = ay * t;
                float speed = Mathf.Sqrt(vx*vx + vy*vy);
                speeds.Add(speed);

                if (!firstPoint)
                    accumulatedPath += Vector3.Distance(pos, prevPos);
                prevPos = pos;
                firstPoint = false;
            }
            range = 0.5f * ax * flightTime * flightTime;
            float vx_land = ax * flightTime;
            float vy_land = ay * flightTime;
            speedAtLanding = Mathf.Sqrt(vx_land*vx_land + vy_land*vy_land);
            totalPath = accumulatedPath;
            avgSpeed = totalPath / flightTime;

            resultText.text =
                $"Время полёта: {flightTime:F3} с\n" +
                $"Дальность: {range:F3} м\n" +
                $"Скорость при приземлении: {speedAtLanding:F3} м/с\n" +
                $"Средняя путевая скорость: {avgSpeed:F3} м/с\n" +
                $"Пройденный путь: {totalPath:F3} м";
        }

        // Отображаем траекторию
        trajectoryLine.positionCount = positions.Count;
        trajectoryLine.SetPositions(positions.ToArray());

        // Сбрасываем анимацию
        timeElapsed = 0f;
        movingObject.position = positions[0];
        UpdateCurrentValues(0f);
        isMoving = true;
    }

    void UpdateCurrentValues(float time)
    {
        int index = Mathf.FloorToInt(time / flightTime * (positions.Count - 1));
        index = Mathf.Clamp(index, 0, positions.Count - 1);
        movingObject.position = positions[index];

        if (currentCoord != null)
        {
            Vector3 pos = positions[index];
            currentCoord.text = $"Текущая позиция: ({pos.x:F3}, {pos.y:F3}, {pos.z:F3})";
        }
        if (currentSpeed != null && speeds.Count > index)
            currentSpeed.text = $"Текущая скорость: {speeds[index]:F3} м/с";
    }
}