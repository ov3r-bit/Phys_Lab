using UnityEngine;
using TMPro;

public class MotionControllerLab6_1 : MonoBehaviour
{
    [Header("Dropdown")]
    public TMP_Dropdown taskDropdown;

    [Header("Physics")]
    public ElasticCollision3D collision;

    [Header("Mass")]
    public TMP_InputField mass1Input;
    public TMP_InputField mass2Input;

    [Header("Object 1 velocity")]
    public TMP_InputField velocity1XInput;
    public TMP_InputField velocity1YInput;
    public TMP_InputField velocity1ZInput;

    [Header("Object 2 velocity")]
    public TMP_InputField velocity2XInput;
    public TMP_InputField velocity2YInput;
    public TMP_InputField velocity2ZInput;

    [Header("Text")]
    public TMP_Text taskDescription;
    public TMP_Text resultText;

    private void Start()
    {
        taskDropdown.onValueChanged.AddListener(OnTaskChanged);

        // Загружаем выбранное задание
        OnTaskChanged(taskDropdown.value);
    }

    public void OnTaskChanged(int task)
    {
        if (task == 0)
        {
            SetupTask2();
        }
        else if (task == 1)
        {
            SetupTask3();
        }
    }

    // =========================
    // ЗАДАНИЕ 2
    // =========================

    private void SetupTask2()
    {
        taskDescription.text =
            "Задание 2\n" +
            "Прямой нецентральный удар.\n" +
            "Массы объектов равны.";

        mass1Input.text = "1";
        mass2Input.text = "1";

        velocity1XInput.text = "5";
        velocity1YInput.text = "0";
        velocity1ZInput.text = "0";

        velocity2XInput.text = "0";
        velocity2YInput.text = "0";
        velocity2ZInput.text = "0";

        collision.isNonCentral = true;

        collision.ResetSimulation(0);
    }

    // =========================
    // ЗАДАНИЕ 3
    // =========================

    private void SetupTask3()
    {
        taskDescription.text =
            "Задание 3\n" +
            "Объект 2 может перемещаться\n" +
            "со скоростью v2.";

        mass1Input.text = "1";
        mass2Input.text = "2";

        velocity1XInput.text = "5";
        velocity1YInput.text = "0";
        velocity1ZInput.text = "0";

        velocity2XInput.text = "-2";
        velocity2YInput.text = "0";
        velocity2ZInput.text = "0";

        collision.isNonCentral = false;

        collision.ResetSimulation(1);
    }

    // =========================
    // ЗАПУСК
    // =========================

    public void StartExperiment()
    {
        float m1 = Parse(mass1Input.text, 1f);
        float m2 = Parse(mass2Input.text, 1f);

        Vector3 v1 = new Vector3(
            Parse(velocity1XInput.text, 0f),
            Parse(velocity1YInput.text, 0f),
            Parse(velocity1ZInput.text, 0f)
        );

        Vector3 v2 = new Vector3(
            Parse(velocity2XInput.text, 0f),
            Parse(velocity2YInput.text, 0f),
            Parse(velocity2ZInput.text, 0f)
        );

        // Передаём введённые значения в физический скрипт
        collision.mass1 = m1;
        collision.mass2 = m2;

        collision.velocity1 = v1;
        collision.velocity2 = v2;

        // Запускаем
        collision.StartSimulation();

        resultText.text =
            "Эксперимент запущен\n\n" +
            $"m1 = {m1} кг\n" +
            $"m2 = {m2} кг\n\n" +
            $"v1 = {v1} м/с\n" +
            $"v2 = {v2} м/с";
    }

    // =========================
    // СБРОС
    // =========================

    public void ResetExperiment()
    {
        collision.ResetSimulation(taskDropdown.value);

        resultText.text = "Эксперимент сброшен";
    }

    // =========================
    // ПРЕОБРАЗОВАНИЕ ЧИСЛА
    // =========================

    private float Parse(string value, float defaultValue)
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