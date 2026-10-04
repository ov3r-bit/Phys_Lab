using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class LensShape : MonoBehaviour
{
    public enum LensType
    {
        Biconvex,
        PlanoConvex,
        PositiveMeniscus,
        Biconcave,
        PlanoConcave,
        NegativeMeniscus
    }

    [Header("Lens")]
    public LensType lensType = LensType.Biconvex;

    [Header("Size")]
    public float width = 2f;
    public float height = 4f;

    [Header("Optical properties")]
    public float refractiveIndex = 1.5f;

    [Header("Curvature")]
    [Range(0.05f, 1f)]
    public float curvature = 0.35f;

    private PolygonCollider2D polygon;

    private void Awake()
    {
        polygon = GetComponent<PolygonCollider2D>();

        GenerateLens();
    }

    private void OnValidate()
    {
        polygon = GetComponent<PolygonCollider2D>();

        if (polygon != null)
            GenerateLens();
    }

    public void GenerateLens()
    {
        if (polygon == null)
            polygon = GetComponent<PolygonCollider2D>();

        Vector2[] points;

        switch (lensType)
        {
            case LensType.Biconvex:
                points = CreateBiconvex();
                break;

            case LensType.PlanoConvex:
                points = CreatePlanoConvex();
                break;

            case LensType.PositiveMeniscus:
                points = CreatePositiveMeniscus();
                break;

            case LensType.Biconcave:
                points = CreateBiconcave();
                break;

            case LensType.PlanoConcave:
                points = CreatePlanoConcave();
                break;

            case LensType.NegativeMeniscus:
                points = CreateNegativeMeniscus();
                break;

            default:
                points = CreateBiconvex();
                break;
        }

        polygon.pathCount = 1;
        polygon.SetPath(0, points);
    }

    // ============================================================
    // 1. ДВОЯКОВЫПУКЛАЯ
    // ============================================================

    private Vector2[] CreateBiconvex()
    {
        return CreateLensProfile(
            leftCurve: 1f,
            rightCurve: 1f
        );
    }

    // ============================================================
    // 2. ПЛОСКОВЫПУКЛАЯ
    // ============================================================

    private Vector2[] CreatePlanoConvex()
    {
        return CreateLensProfile(
            leftCurve: 1f,
            rightCurve: 0f
        );
    }

    // ============================================================
    // 3. ПОЛОЖИТЕЛЬНЫЙ МЕНИСК
    // ============================================================

    private Vector2[] CreatePositiveMeniscus()
    {
        return CreateLensProfile(
            leftCurve: 1f,
            rightCurve: 0.25f
        );
    }

    // ============================================================
    // 4. ДВОЯКОВОГНУТАЯ
    // ============================================================

    private Vector2[] CreateBiconcave()
    {
        return CreateLensProfile(
            leftCurve: -1f,
            rightCurve: -1f
        );
    }

    // ============================================================
    // 5. ПЛСКОВОГНУТАЯ
    // ============================================================

    private Vector2[] CreatePlanoConcave()
    {
        return CreateLensProfile(
            leftCurve: -1f,
            rightCurve: 0f
        );
    }

    // ============================================================
    // 6. ОТРИЦАТЕЛЬНЫЙ МЕНИСК
    // ============================================================

    private Vector2[] CreateNegativeMeniscus()
    {
        return CreateLensProfile(
            leftCurve: -0.25f,
            rightCurve: -1f
        );
    }

    // ============================================================
    // СОЗДАНИЕ ПРОФИЛЯ
    // ============================================================

    private Vector2[] CreateLensProfile(
        float leftCurve,
        float rightCurve)
    {
        int segments = 60;

        Vector2[] points =
            new Vector2[(segments + 1) * 2];

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;

            float y =
                Mathf.Lerp(
                    -halfHeight,
                    halfHeight,
                    t
                );

            float normalizedY =
                y / halfHeight;

            float circle =
                Mathf.Sqrt(
                    Mathf.Max(
                        0f,
                        1f -
                        normalizedY *
                        normalizedY
                    )
                );

            // ----------------------------------------------------
            // Левая поверхность
            // ----------------------------------------------------

            float leftX =
                -halfWidth;

            if (leftCurve > 0f)
            {
                leftX +=
                    halfWidth *
                    curvature *
                    leftCurve *
                    circle;
            }
            else if (leftCurve < 0f)
            {
                leftX -=
                    halfWidth *
                    curvature *
                    Mathf.Abs(leftCurve) *
                    circle;
            }

            // ----------------------------------------------------
            // Правая поверхность
            // ----------------------------------------------------

            float rightX =
                halfWidth;

            if (rightCurve > 0f)
            {
                rightX -=
                    halfWidth *
                    curvature *
                    rightCurve *
                    circle;
            }
            else if (rightCurve < 0f)
            {
                rightX +=
                    halfWidth *
                    curvature *
                    Mathf.Abs(rightCurve) *
                    circle;
            }

            points[i] =
                new Vector2(
                    leftX,
                    y
                );

            points[
                points.Length - 1 - i
            ] =
                new Vector2(
                    rightX,
                    y
                );
        }

        return points;
    }
}