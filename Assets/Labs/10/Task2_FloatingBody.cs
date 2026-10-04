using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Task2_FloatingBody : MonoBehaviour
{
    [Header("Среда")]
    public float rho1 = 1000f;
    public float g    = 9.81f;
    public float waterLevelY = 0f;

    [Header("Тело")]
    public float rho2 = 600f;
    public float V    = 0.001f;
    public float bodyHeight = 1f;

    private Rigidbody rb;
    private string result = "";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = rho2 * V;
        rb.useGravity = false;

        float Fa = rho1 * g * V;
        float P  = rho2 * g * V;

        if (rho2 < rho1)
            result = $"ρ2={rho2} < ρ1={rho1} → ВСПЛЫВАЕТ (погружено {rho2/rho1:P0})";
        else if (Mathf.Approximately(rho2, rho1))
            result = "ρ2 = ρ1 → РАВНОВЕСИЕ (плавает полностью погружённым)";
        else
            result = $"ρ2={rho2} > ρ1={rho1} → ТОНЕТ";

        Debug.Log("[Задание 2] " + result);
    }

    void FixedUpdate()
    {
        // Гравитация вручную
        rb.AddForce(Vector3.down * rho2 * g * V, ForceMode.Force);

        // Архимедова сила пропорционально погружённой части
        float depth = Mathf.Max(0f, waterLevelY - transform.position.y + bodyHeight * 0.5f);
        float submerged = Mathf.Clamp01(depth / bodyHeight);
        float Fa = rho1 * g * V * submerged;
        rb.AddForce(Vector3.up * Fa, ForceMode.Force);

        rb.linearDamping = 2f;
        rb.angularDamping = 2f;
    }

    public string GetResult() => result;
}