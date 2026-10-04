using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Task3_VariableDensity : MonoBehaviour
{
    [Header("Среда")]
    public float rho1 = 1000f;
    public float g    = 9.81f;
    public float waterLevelY = 1f;

    [Header("Тело")]
    public float V = 0.001f;
    public float bodyHeight = 0.6f;
    public float v = 2f;   // горизонтальная скорость

    [Header("График плотности ρ2(t), аппроксимация графика из PDF")]
    public Vector2[] densityCurve = new Vector2[]
    {
        new Vector2(0f,   500f),
        new Vector2(1.5f, 500f),
        new Vector2(2.0f, 700f),
        new Vector2(3.0f, 700f),
        new Vector2(3.5f, 900f),
        new Vector2(4.0f, 1200f),
        new Vector2(5.0f, 1200f),
        new Vector2(5.5f, 400f),
        new Vector2(6.5f, 400f),
        new Vector2(7.0f, 800f),
        new Vector2(8.0f, 800f),
        new Vector2(8.5f, 500f),
        new Vector2(9.5f, 500f),
        new Vector2(10f,  1100f),
        new Vector2(11f,  1100f),
        new Vector2(11.5f,600f),
        new Vector2(12.5f,600f)
    };

    private Rigidbody rb;
    private float t = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = 1f;
    }

    float DensityAt(float time)
    {
        if (time <= densityCurve[0].x) return densityCurve[0].y;
        for (int i = 0; i < densityCurve.Length - 1; i++)
        {
            var a = densityCurve[i];
            var b = densityCurve[i + 1];
            if (time >= a.x && time <= b.x)
            {
                float k = (time - a.x) / Mathf.Max(0.0001f, b.x - a.x);
                return Mathf.Lerp(a.y, b.y, k);
            }
        }
        return densityCurve[densityCurve.Length - 1].y;
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;

        float rho2 = DensityAt(t);
        rb.mass = rho2 * V;

        // Вес
        rb.AddForce(Vector3.down * rho2 * g * V, ForceMode.Force);

        // Архимедова сила по погружению
        float depth = Mathf.Max(0f, waterLevelY - transform.position.y + bodyHeight * 0.5f);
        float submerged = Mathf.Clamp01(depth / bodyHeight);
        float Fa = rho1 * g * V * submerged;
        rb.AddForce(Vector3.up * Fa, ForceMode.Force);

        // Постоянное горизонтальное движение
        Vector3 vel = rb.linearVelocity;
        vel.x = v;
        rb.linearVelocity = vel;

        rb.linearDamping = 1.5f;
        rb.angularDamping = 2f;

        // Цвет по плотности
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            float k = Mathf.InverseLerp(400f, 1200f, rho2);
            rend.material.color = Color.Lerp(Color.cyan, Color.red, k);
        }
    }
}