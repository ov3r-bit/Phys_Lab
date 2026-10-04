using UnityEngine;
using TMPro;

public class Lab62Manager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown taskDropdown;
    public TMP_InputField speedInput;
    public TMP_InputField angleInput;
    public TMP_Text resultText;

    [Header("Main object")]
    public Rigidbody movingObject;

    [Header("Point A")]
    public Transform pointA;

    [Header("Pre-made obstacles")]
    public GameObject staticObstacles;

    [Header("Task 2 moving obstacle")]
    public GameObject movingObstacle;

    [Header("Random spawn")]
    public GameObject randomStaticPrefab;
    public GameObject randomMovingPrefab;

    public Transform randomSpawnParent;

    public float spawnWidth = 14f;
    public float spawnHeight = 8f;

    public int randomStaticCount = 5;
    public int randomMovingCount = 3;

    private float speed;
    private float angle;

    private bool simulationRunning;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = movingObject.position;

        taskDropdown.onValueChanged.AddListener(ChangeTask);

        ChangeTask(taskDropdown.value);
    }

    // =====================================================
    // ВЫБОР ЗАДАНИЯ
    // =====================================================

    public void ChangeTask(int task)
    {
        StopSimulation();

        ClearRandomObjects();

        switch (task)
        {
            case 0:
                SetupTask1();
                break;

            case 1:
                SetupTask2();
                break;

            case 2:
                SetupTask3();
                break;
        }

        ResetMovingObject();
    }

    // =====================================================
    // ЗАДАНИЕ 1
    // =====================================================

    private void SetupTask1()
    {
        resultText.text =
            "Задание 1\n" +
            "Неподвижные препятствия";

        // Включаем заранее созданные препятствия
        staticObstacles.SetActive(true);

        // Подвижное препятствие выключаем
        movingObstacle.SetActive(false);
    }

    // =====================================================
    // ЗАДАНИЕ 2
    // =====================================================

    private void SetupTask2()
    {
        resultText.text =
            "Задание 2\n" +
            "Добавлено подвижное препятствие";

        // Обычные препятствия остаются
        staticObstacles.SetActive(true);

        // Включаем подвижное
        movingObstacle.SetActive(true);

        // Ставим его в начальную позицию
        movingObstacle.transform.position =
            new Vector3(2f, 0.5f, 2f);

        Rigidbody rb =
            movingObstacle.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // =====================================================
    // ЗАДАНИЕ 3
    // =====================================================

    private void SetupTask3()
    {
        resultText.text =
            "Задание 3\n" +
            "Случайная генерация препятствий";

        // Выключаем заранее созданные
        staticObstacles.SetActive(false);

        // Выключаем препятствие задания 2
        movingObstacle.SetActive(false);

        // Создаём случайные
        SpawnRandomObjects();
    }

    // =====================================================
    // ЗАПУСК ДВИЖЕНИЯ
    // =====================================================

    public void StartSimulation()
    {
        speed = Parse(speedInput.text, 5f);
        angle = Parse(angleInput.text, 30f);

        float radians = angle * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(
            Mathf.Cos(radians),
            0f,
            Mathf.Sin(radians)
        ).normalized;

        movingObject.linearVelocity =
            direction * speed;

        simulationRunning = true;

        resultText.text =
            $"Движение запущено\n" +
            $"Скорость: {speed:F2} м/с\n" +
            $"Угол: {angle:F2}°";
    }

    // =====================================================
    // СБРОС
    // =====================================================

    public void ResetSimulation()
    {
        StopSimulation();

        ResetMovingObject();

        resultText.text =
            "Симуляция сброшена";
    }

    private void StopSimulation()
    {
        simulationRunning = false;

        movingObject.linearVelocity = Vector3.zero;
        movingObject.angularVelocity = Vector3.zero;
    }

    private void ResetMovingObject()
    {
        movingObject.position = startPosition;

        movingObject.linearVelocity = Vector3.zero;
        movingObject.angularVelocity = Vector3.zero;
    }

    // =====================================================
    // ПРОВЕРКА ТОЧКИ A
    // =====================================================

    private void FixedUpdate()
    {
        if (!simulationRunning)
            return;

        float distance = Vector3.Distance(
            movingObject.position,
            pointA.position
        );

        if (distance < 0.7f)
        {
            simulationRunning = false;

            movingObject.linearVelocity =
                Vector3.zero;

            resultText.text =
                "ОБЪЕКТ ПОПАЛ В ТОЧКУ A";
        }
    }

    // =====================================================
    // RANDOM SPAWN
    // =====================================================

    private void SpawnRandomObjects()
    {
        for (int i = 0; i < randomStaticCount; i++)
        {
            Vector3 position =
                GetRandomPosition();

            GameObject obj =
                Instantiate(
                    randomStaticPrefab,
                    position,
                    Quaternion.identity,
                    randomSpawnParent
                );

            Rigidbody rb =
                obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
            }
        }

        for (int i = 0; i < randomMovingCount; i++)
        {
            Vector3 position =
                GetRandomPosition();

            GameObject obj =
                Instantiate(
                    randomMovingPrefab,
                    position,
                    Quaternion.identity,
                    randomSpawnParent
                );

            Rigidbody rb =
                obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;

                rb.linearVelocity =
                    GetRandomVelocity();
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(
            -spawnWidth / 2f,
            spawnWidth / 2f
        );

        float z = Random.Range(
            -spawnHeight / 2f,
            spawnHeight / 2f
        );

        return new Vector3(
            x,
            -3.92f,
            z
        );
    }

    private Vector3 GetRandomVelocity()
    {
        Vector2 direction =
            Random.insideUnitCircle.normalized;

        float randomSpeed =
            Random.Range(1f, 3f);

        return new Vector3(
            direction.x * randomSpeed,
            0f,
            direction.y * randomSpeed
        );
    }

    // =====================================================
    // УДАЛЕНИЕ RANDOM OBJECTS
    // =====================================================

    private void ClearRandomObjects()
    {
        if (randomSpawnParent == null)
            return;

        for (int i =
             randomSpawnParent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                randomSpawnParent.GetChild(i).gameObject
            );
        }
    }

    // =====================================================
    // PARSE
    // =====================================================

    private float Parse(
        string value,
        float defaultValue)
    {
        value = value.Replace(',', '.');

        if (float.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float result))
        {
            return result;
        }

        return defaultValue;
    }
}