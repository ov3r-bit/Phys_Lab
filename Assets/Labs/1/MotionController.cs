using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotionController : MonoBehaviour
{
    [Header("Объекты сцены")]
    public Transform movingObject;          // Сфера
    public LineRenderer trajectoryLine;     // Линия траектории

    [Header("Элементы UI")]
    public TMP_Dropdown taskDropdown;       // Выбор задания
    public TMP_InputField[] paramInputs;    // 10 полей: vx, vy, vz, ax, ay, az, x0, y0, z0, t
    public Button startButton;              // Кнопка "Старт"

    [Header("Вывод информации (опционально)")]
    public TMP_Text coordText;              // Текущая координата (x, y, z)
    public TMP_Text pathText;               // Пройденный путь
    public TMP_Text speedText;              // Текущая скорость

    // Внутренние переменные
    private float timeElapsed = 0f;
    private float totalTime = 10f;
    private List<Vector3> positions = new List<Vector3>();
    private List<float> pathValues = new List<float>();    // путь для каждого кадра
    private List<float> speedValues = new List<float>();   // скорость для каждого кадра
    private Vector3 startPos;
    private bool isMoving = false;

    // Параметры движения (заполняются из UI)
    private float vx, vy, vz, ax, ay, az, x0, y0, z0;

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

        // Если поля для вывода не назначены – скрываем ошибку
        if (coordText == null) Debug.LogWarning("coordText не назначен в инспекторе");
        if (pathText == null) Debug.LogWarning("pathText не назначен в инспекторе");
        if (speedText == null) Debug.LogWarning("speedText не назначен в инспекторе");
    }

    void Update()
    {
        if (isMoving)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= totalTime)
            {
                timeElapsed = totalTime;
                isMoving = false;
                // Последнее обновление
                UpdatePosition(timeElapsed);
                return;
            }
            UpdatePosition(timeElapsed);
        }
    }

    void StartSimulation()
    {
        // Считываем параметры
        int task = taskDropdown.value + 1; // 1..5
        ParseInputs();

        // Инициализация
        timeElapsed = 0f;
        positions.Clear();
        pathValues.Clear();
        speedValues.Clear();
        trajectoryLine.positionCount = 0;
        startPos = new Vector3(x0, y0, z0);
        movingObject.position = startPos;

        // В зависимости от задания выбираем закон движения и заполняем списки
        switch (task)
        {
            case 1: Task1(); break;
            case 2: Task2(); break;
            case 3: Task3(); break;
            case 4: Task4(); break;
            case 5: Task5(); break;
            default: break;
        }

        // Отрисовываем траекторию
        DrawTrajectory();

        // Обновляем информацию в начальный момент
        UpdatePosition(0f);

        // Запускаем анимацию
        isMoving = true;
    }

    void ParseInputs()
    {
        float.TryParse(paramInputs[0].text, out vx);
        float.TryParse(paramInputs[1].text, out vy);
        float.TryParse(paramInputs[2].text, out vz);
        float.TryParse(paramInputs[3].text, out ax);
        float.TryParse(paramInputs[4].text, out ay);
        float.TryParse(paramInputs[5].text, out az);
        float.TryParse(paramInputs[6].text, out x0);
        float.TryParse(paramInputs[7].text, out y0);
        float.TryParse(paramInputs[8].text, out z0);
        float.TryParse(paramInputs[9].text, out totalTime);
        if (totalTime <= 0) totalTime = 10f;
    }

    // ---------- Задания (заполняют списки positions, pathValues, speedValues) ----------

    void Task1()
    {
        // Цикл с шагом 0.05
        for (float t = 0; t < totalTime; t += 0.05f)
        {
            float S = vx * t;
            positions.Add(new Vector3(x0 + S, y0, z0));
            pathValues.Add(S);
            speedValues.Add(vx);
        }
        // Явно добавляем конечную точку
        float S_end = vx * totalTime;
        positions.Add(new Vector3(x0 + S_end, y0, z0));
        pathValues.Add(S_end);
        speedValues.Add(vx);
    }

    void Task2()
    {
        // Равномерное 1D: координата x = x0 + v*t, путь S = v*t
        for (float t = 0; t <= totalTime; t += 0.05f)
        {
            float S = vx * t;
            float x = x0 + S;
            positions.Add(new Vector3(x, y0, z0));
            pathValues.Add(S);
            speedValues.Add(vx);
        }
    }

    void Task3()
    {
        // Равноускоренное 1D: x = x0 + v0*t + 0.5*a*t^2, v = v0 + a*t, S = v0*t + 0.5*a*t^2
        for (float t = 0; t <= totalTime; t += 0.05f)
        {
            float S = vx * t + 0.5f * ax * t * t;
            float x = x0 + S;
            float v = vx + ax * t;
            positions.Add(new Vector3(x, y0, z0));
            pathValues.Add(S);
            speedValues.Add(v);
        }
    }

    void Task4()
    {
        // 2D равномерное
        for (float t = 0; t < totalTime; t += 0.05f)
        {
            float x = x0 + vx * t;
            float y = y0 + vy * t;
            float z = z0;
            float S = Mathf.Sqrt(vx * vx + vy * vy) * t;
            float speed = Mathf.Sqrt(vx * vx + vy * vy);
            positions.Add(new Vector3(x, y, z));
            pathValues.Add(S);
            speedValues.Add(speed);
        }
        // Конечная точка
        float t_end = totalTime;
        float x_end = x0 + vx * t_end;
        float y_end = y0 + vy * t_end;
        float z_end = z0;
        float S_end = Mathf.Sqrt(vx * vx + vy * vy) * t_end;
        float speed_end = Mathf.Sqrt(vx * vx + vy * vy);
        positions.Add(new Vector3(x_end, y_end, z_end));
        pathValues.Add(S_end);
        speedValues.Add(speed_end);
    }


    void Task5()
    {
        // 3D равномерное
        for (float t = 0; t < totalTime; t += 0.05f)
        {
            float x = x0 + vx * t;
            float y = y0 + vy * t;
            float z = z0 + vz * t;
            float S = Mathf.Sqrt(vx * vx + vy * vy + vz * vz) * t;
            float speed = Mathf.Sqrt(vx * vx + vy * vy + vz * vz);
            positions.Add(new Vector3(x, y, z));
            pathValues.Add(S);
            speedValues.Add(speed);
        }
        float t_end = totalTime;
        float x_end = x0 + vx * t_end;
        float y_end = y0 + vy * t_end;
        float z_end = z0 + vz * t_end;
        float S_end = Mathf.Sqrt(vx * vx + vy * vy + vz * vz) * t_end;
        float speed_end = Mathf.Sqrt(vx * vx + vy * vy + vz * vz);
        positions.Add(new Vector3(x_end, y_end, z_end));
        pathValues.Add(S_end);
        speedValues.Add(speed_end);
    }

    // ---------- Вспомогательные методы ----------

    void DrawTrajectory()
    {
        trajectoryLine.positionCount = positions.Count;
        trajectoryLine.SetPositions(positions.ToArray());
    }

    void UpdatePosition(float t)
    {
        // Находим индекс, соответствующий текущему времени
        int index = Mathf.FloorToInt(t / totalTime * positions.Count);
        index = Mathf.Clamp(index, 0, positions.Count - 1);

        // Перемещаем сферу
        movingObject.position = positions[index];

        // Обновляем информацию в UI (если поля назначены)
        if (coordText != null)
        {
            Vector3 pos = positions[index];
            coordText.text = $"Координата: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})";
        }
        if (pathText != null && pathValues.Count > index)
        {
            pathText.text = $"Пройденный путь: {pathValues[index]:F2}";
        }
        if (speedText != null && speedValues.Count > index)
        {
            speedText.text = $"Скорость: {speedValues[index]:F2}";
        }
    }
}