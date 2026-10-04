using UnityEngine;

public class Task1_HydraulicPress : MonoBehaviour
{
    [Header("Параметры пресса")]
    public float S1 = 0.01f;
    public float S2 = 0.50f;
    public float F  = 200f;
    public float m  = 50f;
    public float g  = 9.81f;

    [Header("Ссылки")]
    public Transform smallPiston;
    public Transform bigPiston;
    public Transform load;

    [Header("Анимация")]
    public float riseHeight = 0.6f;
    public float speed      = 0.8f;

    private Vector3 smallStart, bigStart, loadStart;
    private bool canLift;
    private float progress = 0f;
    private string result = "";

    void Start()
    {
        smallStart = smallPiston.position;
        bigStart   = bigPiston.position;
        loadStart  = load.position;

        float F2 = F * S2 / S1;
        float weight = m * g;
        canLift = F2 >= weight;
        result = $"F2 = {F2:F1} Н   вес = {weight:F1} Н   →   " +
                 (canLift ? "ПОДЪЁМ возможен" : "подъём НЕВОЗМОЖЕН");

        Debug.Log("[Задание 1] " + result);
    }

    void Update()
    {
        if (!canLift) return;

        progress = Mathf.Clamp01(progress + Time.deltaTime * speed);

        bigPiston.position   = bigStart   + Vector3.up   * riseHeight * progress;
        load.position        = loadStart  + Vector3.up   * riseHeight * progress;
        smallPiston.position = smallStart + Vector3.down * riseHeight * (S2 / S1) * progress;
    }

    public string GetResult() => result;
}