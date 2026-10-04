using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotionControllerLab4 : MonoBehaviour
{
    [Header("Объекты сцены")]
    public Transform movingObject;          // Сфера
    public LineRenderer trajectoryLine;     // Линия траектории

    [Header("Элементы UI (ввод)")]
    public TMP_Dropdown taskDropdown;       // 0 - окружность, 1 - винт равномерная, 2 - винт с ускорением
    public TMP_InputField inputR;           // Радиус
    public TMP_InputField inputOmega0;      // Начальная угловая скорость (рад/с)
    public TMP_InputField inputAlpha;       // Угловое ускорение (рад/с²)
    public TMP_InputField inputVz;          // Скорость вдоль оси Z (м/с)
    public TMP_InputField inputAz;          // Ускорение вдоль оси Z (м/с²)
    public TMP_InputField inputTime;        // Общее время t
    public TMP_InputField inputT1;          // Момент времени t1 (для координат)
    public Button startButton;

    [Header("Элементы UI (вывод)")]
    public TMP_Text resultText;             // Основной вывод результатов
    public TMP_Text currentCoord;           // Текущая позиция (x, y, z)
    public TMP_Text currentSpeed;           // Текущая скорость (модуль)

    // Параметры
    private float R, omega0, alpha, Vz, az, t, t1;
    private int task;

    // Списки для траектории
    private List<Vector3> positions = new List<Vector3>();
    private List<float> speeds = new List<float>();   // модуль скорости в каждой точке
    private List<float> angles = new List<float>();   // угол поворота (для окружности)

    private float totalPath = 0f;           // пройденный путь за время t
    private float totalRevolutions = 0f;    // число оборотов
    private float angleAtT = 0f;            // угол поворота за время t
    private float omegaAtT = 0f;            // угловая скорость в момент t
    private float speedAtT = 0f;            // линейная скорость в момент t
    private Vector3 coordAtT1;              // координаты в момент t1

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
    }

    void Update()
    {
        if (isMoving)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= t)
            {
                timeElapsed = t;
                isMoving = false;
            }
            UpdateCurrentValues(timeElapsed);
        }
    }

    void StartSimulation()
    {
        // Считываем параметры
        float.TryParse(inputR.text, out R);
        float.TryParse(inputOmega0.text, out omega0);
        float.TryParse(inputAlpha.text, out alpha);
        float.TryParse(inputVz.text, out Vz);
        float.TryParse(inputAz.text, out az);
        float.TryParse(inputTime.text, out t);
        float.TryParse(inputT1.text, out t1);
        task = taskDropdown.value; // 0,1,2

        // Защита от некорректных значений
        if (R <= 0) R = 1f;
        if (t <= 0) t = 5f;
        if (t1 < 0 || t1 > t) t1 = t / 2f;

        // Очистка
        positions.Clear();
        speeds.Clear();
        angles.Clear();
        trajectoryLine.positionCount = 0;

        // Генерация точек траектории с шагом 0.02 с
        float dt = 0.02f;
        float accumulatedPath = 0f;
        Vector3 prevPos = Vector3.zero;
        bool firstPoint = true;

        // Вычисляем конечные значения
        if (task == 0) // Окружность (равноускоренное вращение)
        {
            // Угловая скорость: ω(t) = ω0 + α*t
            // Угол: φ(t) = ω0*t + 0.5*α*t^2
            // Координаты: x = R*cos(φ), y = R*sin(φ), z = 0
            // Путь: S = R * |φ(t)| (при условии, что направление не меняется)
            // Число оборотов: N = φ(t) / (2π)
            // Линейная скорость: v = R * ω(t)

            for (float time = 0; time <= t + dt/2; time += dt)
            {
                float tm = Mathf.Min(time, t);
                float phi = omega0 * tm + 0.5f * alpha * tm * tm;
                float omega = omega0 + alpha * tm;
                float x = R * Mathf.Cos(phi);
                float y = R * Mathf.Sin(phi);
                Vector3 pos = new Vector3(x, y, 0);
                positions.Add(pos);
                speeds.Add(R * Mathf.Abs(omega));
                angles.Add(phi);

                if (!firstPoint)
                    accumulatedPath += Vector3.Distance(pos, prevPos);
                prevPos = pos;
                firstPoint = false;
            }

            totalPath = accumulatedPath;
            angleAtT = omega0 * t + 0.5f * alpha * t * t;
            totalRevolutions = angleAtT / (2f * Mathf.PI);
            omegaAtT = omega0 + alpha * t;
            speedAtT = R * Mathf.Abs(omegaAtT);
            // Координаты в момент t1 (если нужны)
            float phi1 = omega0 * t1 + 0.5f * alpha * t1 * t1;
            coordAtT1 = new Vector3(R * Mathf.Cos(phi1), R * Mathf.Sin(phi1), 0);

            // Вывод результатов
            resultText.text = 
                $"Число оборотов за время t: {totalRevolutions:F3}\n" +
                $"Угол поворота за время t: {angleAtT:F3} рад\n" +
                $"Путь за время t: {totalPath:F3} м\n" +
                $"Угловая скорость в момент t: {omegaAtT:F3} рад/с\n" +
                $"Линейная скорость в момент t: {speedAtT:F3} м/с\n" +
                $"Координаты в момент t1: ({coordAtT1.x:F3}, {coordAtT1.y:F3}, {coordAtT1.z:F3})";
        }
        else if (task == 1) // Винтовая линия (равномерная) – движение по окружности + равномерное вдоль Z
        {
            // Угловая скорость постоянна: omega = omega0 (если не задана, то берем omega0)
            // Скорость вдоль Z: Vz постоянна
            // Параметризация: x = R*cos(ω*t), y = R*sin(ω*t), z = Vz*t
            // Путь: S = sqrt( (R*ω)^2 + Vz^2 ) * t
            // Координаты в момент t1: аналогично

            for (float time = 0; time <= t + dt/2; time += dt)
            {
                float tm = Mathf.Min(time, t);
                float phi = omega0 * tm;
                float x = R * Mathf.Cos(phi);
                float y = R * Mathf.Sin(phi);
                float z = Vz * tm;
                Vector3 pos = new Vector3(x, y, z);
                positions.Add(pos);
                float speed = Mathf.Sqrt( (R*omega0)*(R*omega0) + Vz*Vz );
                speeds.Add(speed);
                angles.Add(phi);

                if (!firstPoint)
                    accumulatedPath += Vector3.Distance(pos, prevPos);
                prevPos = pos;
                firstPoint = false;
            }

            totalPath = accumulatedPath;
            angleAtT = omega0 * t;
            totalRevolutions = angleAtT / (2f * Mathf.PI);
            omegaAtT = omega0;
            speedAtT = Mathf.Sqrt( (R*omega0)*(R*omega0) + Vz*Vz );
            float phi1 = omega0 * t1;
            coordAtT1 = new Vector3(R * Mathf.Cos(phi1), R * Mathf.Sin(phi1), Vz * t1);

            resultText.text = 
                $"Путь за время t: {totalPath:F3} м\n" +
                $"Координаты в момент t1: ({coordAtT1.x:F3}, {coordAtT1.y:F3}, {coordAtT1.z:F3})\n" +
                $"Угол поворота за время t: {angleAtT:F3} рад\n" +
                $"Число оборотов: {totalRevolutions:F3}\n" +
                $"Скорость (модуль): {speedAtT:F3} м/с";
        }
        else if (task == 2) // Винтовая линия с ускорением вдоль Z
        {
            // Угловая скорость постоянна: omega = omega0 (можно сделать и с ускорением, но по условию только ускорение вдоль оси)
            // Ускорение вдоль Z: az(t) = A + B*t? Но в условии "a=A+B*t" – значит заданы коэффициенты A и B?
            // В наших полях только одно значение az – возможно, это ускорение в момент времени? 
            // Чтобы упростить, будем считать, что az – постоянное ускорение (равноускоренное вдоль Z).
            // Тогда z(t) = Vz*t + 0.5*az*t^2, скорость вдоль Z = Vz + az*t.
            // Полная скорость: sqrt( (R*ω)^2 + (Vz+az*t)^2 )
            // Путь – длина дуги, вычисляем численно.

            for (float time = 0; time <= t + dt/2; time += dt)
            {
                float tm = Mathf.Min(time, t);
                float phi = omega0 * tm;
                float x = R * Mathf.Cos(phi);
                float y = R * Mathf.Sin(phi);
                float z = Vz * tm + 0.5f * az * tm * tm;
                Vector3 pos = new Vector3(x, y, z);
                positions.Add(pos);
                float vz_now = Vz + az * tm;
                float speed = Mathf.Sqrt( (R*omega0)*(R*omega0) + vz_now*vz_now );
                speeds.Add(speed);
                angles.Add(phi);

                if (!firstPoint)
                    accumulatedPath += Vector3.Distance(pos, prevPos);
                prevPos = pos;
                firstPoint = false;
            }

            totalPath = accumulatedPath;
            angleAtT = omega0 * t;
            totalRevolutions = angleAtT / (2f * Mathf.PI);
            omegaAtT = omega0;
            float vz_t = Vz + az * t;
            speedAtT = Mathf.Sqrt( (R*omega0)*(R*omega0) + vz_t*vz_t );
            float phi1 = omega0 * t1;
            float z1 = Vz * t1 + 0.5f * az * t1 * t1;
            coordAtT1 = new Vector3(R * Mathf.Cos(phi1), R * Mathf.Sin(phi1), z1);

            resultText.text = 
                $"Путь за время t: {totalPath:F3} м\n" +
                $"Координаты в момент t1: ({coordAtT1.x:F3}, {coordAtT1.y:F3}, {coordAtT1.z:F3})\n" +
                $"Угол поворота за время t: {angleAtT:F3} рад\n" +
                $"Число оборотов: {totalRevolutions:F3}\n" +
                $"Скорость (модуль) в момент t: {speedAtT:F3} м/с";
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
        int index = Mathf.FloorToInt(time / t * (positions.Count - 1));
        index = Mathf.Clamp(index, 0, positions.Count - 1);
        movingObject.position = positions[index];

        if (currentCoord != null)
        {
            Vector3 pos = positions[index];
            currentCoord.text = $"Текущая координата: ({pos.x:F3}, {pos.y:F3}, {pos.z:F3})";
        }
        if (currentSpeed != null && speeds.Count > index)
            currentSpeed.text = $"Текущая скорость: {speeds[index]:F3} м/с";
    }
}