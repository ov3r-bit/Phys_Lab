using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerLab7 : MonoBehaviour
{
    [Header("Объекты сцены")]
    public Transform body;                 // Тело на поверхности (дочерний объект плоскости)
    public Transform hangingBody;          // Висящий груз (для задания 2)
    public Transform surface;              // Плоскость
    public LineRenderer trajectory;        // Траектория (в мировых координатах)

    [Header("Элементы UI (ввод)")]
    public TMP_Dropdown taskDropdown;
    public TMP_InputField inputM;
    public TMP_InputField inputMu;
    public TMP_InputField inputAngle;
    public TMP_InputField inputF0;
    public TMP_InputField inputA;
    public TMP_InputField inputT1;
    public TMP_InputField inputM2;
    public TMP_InputField inputMx;
    public Button startButton;

    [Header("Элементы UI (вывод)")]
    public TMP_Text resultText;
    public TMP_Text currentInfo;

    private float m, mu, angleDeg, F0, A, t1, m2, mx;
    private int task;
    private float g = 9.81f;

    private List<Vector3> positions = new List<Vector3>();
    private float totalAnimTime = 10f;
    private float timeElapsed = 0f;
    private bool isMoving = false;
    private bool motionStarted = false;
    private float currentSpeed = 0f;
    private float currentAccel = 0f;

    void Start()
    {
        startButton.onClick.AddListener(StartSimulation);
        trajectory.positionCount = 0;
        trajectory.startWidth = 0.05f;
        trajectory.endWidth = 0.05f;
        trajectory.material = new Material(Shader.Find("Sprites/Default"));
        trajectory.startColor = Color.green;
        trajectory.endColor = Color.green;
    }

    void Update()
    {
        if (isMoving)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= totalAnimTime)
            {
                timeElapsed = totalAnimTime;
                isMoving = false;
            }
            UpdateCurrentState(timeElapsed);
        }
    }

    void StartSimulation()
    {
        float.TryParse(inputM.text, out m);
        float.TryParse(inputMu.text, out mu);
        float.TryParse(inputAngle.text, out angleDeg);
        float.TryParse(inputF0.text, out F0);
        float.TryParse(inputA.text, out A);
        float.TryParse(inputT1.text, out t1);
        float.TryParse(inputM2.text, out m2);
        float.TryParse(inputMx.text, out mx);
        task = taskDropdown.value;

        if (m <= 0) m = 1;
        if (mu < 0) mu = 0;
        if (t1 < 0) t1 = 0;
        if (F0 < 0) F0 = 0;
        if (A < 0) A = 0;

        positions.Clear();
        trajectory.positionCount = 0;

        // Сброс позиции куба (в локальных координатах)
        if (body != null)
            body.localPosition = Vector3.zero;

        float angleRad = angleDeg * Mathf.Deg2Rad;

        // Для заданий, использующих наклон, поворачиваем плоскость
        if ((task == 1 || task == 2) && surface != null)
            surface.rotation = Quaternion.Euler(0, 0, angleDeg);

        if (task == 0) // 1.1 Горизонталь
        {
            float Ftr = mu * m * g;
            float F_applied = F0 + A * t1;
            bool canMove = F_applied > Ftr;
            motionStarted = canMove;

            if (canMove)
            {
                currentAccel = (F_applied - Ftr) / m;
                currentSpeed = 0f;
                float dt = 0.02f;
                float totalTime = 5f;
                for (float t = 0; t <= totalTime; t += dt)
                {
                    float x = 0.5f * currentAccel * t * t;
                    positions.Add(new Vector3(x, 0, 0));
                }
                totalAnimTime = totalTime;
            }
            else
            {
                positions.Add(Vector3.zero);
                totalAnimTime = 1f;
            }

            resultText.text =
                $"Задание 1.1 (горизонт)\n" +
                $"m = {m:F2} кг, µ = {mu:F2}\n" +
                $"F(t1) = {F_applied:F2} Н, Fтр = {Ftr:F2} Н\n" +
                $"Движение началось: {(canMove ? "ДА" : "НЕТ")}\n" +
                (canMove ? $"Ускорение: {currentAccel:F3} м/с²" : "");
        }
        else if (task == 1) // 1.2 Наклонная
        {
            float F_gravity = m * g * Mathf.Sin(angleRad);
            float F_norm = m * g * Mathf.Cos(angleRad);
            float Ftr = mu * F_norm;
            float F_applied = F0 + A * t1;
            float F_net = F_applied - F_gravity - Ftr;
            bool canMove = F_net > 0;
            motionStarted = canMove;

            if (canMove)
            {
                currentAccel = F_net / m;
                currentSpeed = 0f;
                float dt = 0.02f;
                float totalTime = 5f;
                for (float t = 0; t <= totalTime; t += dt)
                {
                    float x = 0.5f * currentAccel * t * t;
                    positions.Add(new Vector3(x, 0, 0));
                }
                totalAnimTime = totalTime;
            }
            else
            {
                positions.Add(Vector3.zero);
                totalAnimTime = 1f;
            }

            resultText.text =
                $"Задание 1.2 (наклонная, α={angleDeg:F1}°)\n" +
                $"m = {m:F2} кг, µ = {mu:F2}\n" +
                $"F(t1) = {F_applied:F2} Н, Fтр = {Ftr:F2} Н\n" +
                $"mg*sinα = {F_gravity:F2} Н\n" +
                $"Движение началось: {(canMove ? "ДА" : "НЕТ")}\n" +
                (canMove ? $"Ускорение: {currentAccel:F3} м/с²" : "");
        }
        else if (task == 2) // Задание 2 – связанные тела с наклоном
        {
            // Сила натяжения (висит груз массой m2+mx)
            float T = (m2 + mx) * g;
            // Составляющая силы тяжести для тела на плоскости
            float F_gravity = m * g * Mathf.Sin(angleRad);
            float F_norm = m * g * Mathf.Cos(angleRad);
            float Ftr = mu * F_norm;
            // Равнодействующая для системы: T тянет вверх по плоскости, сила тяжести и трение – вниз
            float F_net = T - F_gravity - Ftr;
            bool canMove = F_net > 0;
            motionStarted = canMove;

            if (canMove)
            {
                float totalMass = m + m2 + mx;
                currentAccel = F_net / totalMass;
                currentSpeed = 0f;
                float dt = 0.02f;
                float totalTime = 5f;
                for (float t = 0; t <= totalTime; t += dt)
                {
                    float x = 0.5f * currentAccel * t * t;
                    positions.Add(new Vector3(x, 0, 0));
                }
                totalAnimTime = totalTime;
            }
            else
            {
                positions.Add(Vector3.zero);
                totalAnimTime = 1f;
            }

            resultText.text =
                $"Задание 2 (связанные тела, α={angleDeg:F1}°)\n" +
                $"m1 = {m:F2} кг, µ = {mu:F2}\n" +
                $"m2 = {m2:F2} кг, mx = {mx:F2} кг\n" +
                $"T = {T:F2} Н, Fтр = {Ftr:F2} Н\n" +
                $"mg*sinα = {F_gravity:F2} Н\n" +
                $"Движение началось: {(canMove ? "ДА" : "НЕТ")}\n" +
                (canMove ? $"Ускорение: {currentAccel:F3} м/с²" : "");
        }

        // Преобразование локальных координат в мировые для LineRenderer
        Vector3[] worldPositions = new Vector3[positions.Count];
        bool useLocal = (task == 1 || task == 2) && surface != null;
        for (int i = 0; i < positions.Count; i++)
        {
            if (useLocal)
                worldPositions[i] = surface.TransformPoint(positions[i]);
            else
                worldPositions[i] = positions[i];
        }
        trajectory.positionCount = worldPositions.Length;
        trajectory.SetPositions(worldPositions);

        // Начальное состояние
        timeElapsed = 0f;
        if (positions.Count > 0)
        {
            if (useLocal)
                body.localPosition = positions[0];
            else
                body.position = positions[0];
        }
        isMoving = true;
        UpdateCurrentState(0f);
    }

    void UpdateCurrentState(float t)
    {
        int index = Mathf.FloorToInt(t / totalAnimTime * (positions.Count - 1));
        index = Mathf.Clamp(index, 0, positions.Count - 1);
        if (positions.Count > 0)
        {
            bool useLocal = (task == 1 || task == 2) && surface != null;
            if (useLocal)
                body.localPosition = positions[index];
            else
                body.position = positions[index];
        }

        if (currentInfo != null)
        {
            string info = $"Время: {t:F2} с\n";
            info += $"Позиция: {body.position}\n";
            info += $"Скорость: {currentSpeed:F3} м/с\n";
            info += $"Ускорение: {currentAccel:F3} м/с²\n";
            info += $"Движение: {(motionStarted ? "идёт" : "не началось")}";
            currentInfo.text = info;
        }
    }
}