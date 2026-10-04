using System.Collections.Generic;
using UnityEngine;

public class RaySimulator : MonoBehaviour
{
    [Header("Ray")]
    public Transform raySource;
    public float rayAngle = 30f;
    public float maxDistance = 100f;

    [Header("Optics")]
    public float airRefractiveIndex = 1.0f;

    [Header("Simulation")]
    public int maxInteractions = 30;
    public float surfaceOffset = 0.01f;

    [Header("Target")]
    public Transform target;
    public SpriteRenderer target_sprite;
    public float targetRadius = 0.25f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer =
            GetComponent<LineRenderer>();
    }

    private void Start()
    {
        SimulateRay();
    }

    private void Update()
    {
        SimulateRay();
    }

    // ============================================================
    // ОСНОВНАЯ СИМУЛЯЦИЯ
    // ============================================================

    public void SimulateRay()
    {
        List<Vector3> points =
            new List<Vector3>();

        Vector2 origin =
            raySource.position;

        float angle =
            rayAngle * Mathf.Deg2Rad;

        Vector2 direction =
            new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ).normalized;

        points.Add(origin);

        for (
            int interaction = 0;
            interaction < maxInteractions;
            interaction++)
        {
            RaycastHit2D hit =
                Physics2D.Raycast(
                    origin,
                    direction,
                    maxDistance
                );

            // ----------------------------------------------------
            // НИЧЕГО НЕ НАШЛИ
            // ----------------------------------------------------

            if (hit.collider == null)
            {
                points.Add(
                    origin +
                    direction *
                    maxDistance
                );

                break;
            }

            // ----------------------------------------------------
            // ЛИНЗА
            // ----------------------------------------------------

            if (hit.collider.CompareTag("Lens"))
            {
                LensShape lens =
                    hit.collider.GetComponent<LensShape>();

                if (lens == null)
                {
                    lens =
                        hit.collider
                        .GetComponentInParent<LensShape>();
                }

                if (lens == null)
                {
                    Debug.LogError(
                        "Объект с Tag Lens не содержит LensShape!"
                    );

                    break;
                }

                bool success =
                    ProcessLens(
                        hit,
                        lens,
                        ref direction,
                        out Vector2 exitPoint
                    );

                if (!success)
                {
                    Debug.LogWarning(
                        "Не удалось обработать линзу."
                    );

                    break;
                }

                // Точка входа
                points.Add(hit.point);

                // Точка выхода
                points.Add(exitPoint);

                // Начинаем уже ПОСЛЕ линзы
                origin =
                    exitPoint +
                    direction *
                    surfaceOffset;

                continue;
            }

            // ----------------------------------------------------
            // ЗЕРКАЛО
            // ----------------------------------------------------

            if (hit.collider.CompareTag("Mirror"))
            {
                points.Add(hit.point);

                direction =
                    Vector2.Reflect(
                        direction,
                        hit.normal
                    ).normalized;

                origin =
                    hit.point +
                    direction *
                    surfaceOffset;

                continue;
            }

            // ----------------------------------------------------
            // НЕИЗВЕСТНЫЙ ОБЪЕКТ
            // ----------------------------------------------------

            points.Add(hit.point);

            break;
        }

        // ========================================================
        // LINE RENDERER
        // ========================================================

        lineRenderer.positionCount =
            points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(
                i,
                points[i]
            );
        }

        CheckTarget(points);
    }

    // ============================================================
    // ОБРАБОТКА ЛИНЗЫ
    // ============================================================

    private bool ProcessLens(
        RaycastHit2D entryHit,
        LensShape lens,
        ref Vector2 direction,
        out Vector2 exitPoint)
    {
        exitPoint =
            Vector2.zero;

        Vector2 entryPoint =
            entryHit.point;

        Vector2 entryNormal =
            entryHit.normal;

        // --------------------------------------------------------
        // ВХОД: ВОЗДУХ → ЛИНЗА
        // --------------------------------------------------------

        Vector2 insideDirection;

        bool refractedIn =
            Refract(
                direction,
                entryNormal,
                airRefractiveIndex,
                lens.refractiveIndex,
                out insideDirection
            );

        if (!refractedIn)
        {
            // Теоретически для воздуха → стекла
            // это практически невозможно.
            direction =
                Vector2.Reflect(
                    direction,
                    entryNormal
                ).normalized;

            exitPoint =
                entryPoint;

            return true;
        }

        insideDirection.Normalize();

        // --------------------------------------------------------
        // ИЩЕМ ВЫХОД МАТЕМАТИЧЕСКИ
        // --------------------------------------------------------

        PolygonCollider2D polygon =
            lens.GetComponent<PolygonCollider2D>();

        if (polygon == null)
        {
            polygon =
                lens.GetComponentInChildren<
                    PolygonCollider2D>();
        }

        if (polygon == null)
        {
            Debug.LogError(
                "У LensShape нет PolygonCollider2D!"
            );

            return false;
        }

        Vector2[] localPoints =
            polygon.points;

        Transform colliderTransform =
            polygon.transform;

        float bestDistance =
            float.MaxValue;

        Vector2 bestPoint =
            Vector2.zero;

        Vector2 bestNormal =
            Vector2.zero;

        bool found =
            false;

        // --------------------------------------------------------
        // ПРОХОДИМ ПО ВСЕМ РЁБРАМ ПОЛИГОНА
        // --------------------------------------------------------

        for (
            int i = 0;
            i < localPoints.Length;
            i++)
        {
            Vector2 localA =
                localPoints[i];

            Vector2 localB =
                localPoints[
                    (i + 1) %
                    localPoints.Length
                ];

            Vector2 worldA =
                colliderTransform.TransformPoint(
                    localA
                );

            Vector2 worldB =
                colliderTransform.TransformPoint(
                    localB
                );

            if (
                RaySegmentIntersection(
                    entryPoint +
                    insideDirection *
                    surfaceOffset,

                    insideDirection,

                    worldA,
                    worldB,

                    out float distance,
                    out Vector2 intersection
                )
            )
            {
                // Не берём саму точку входа
                if (
                    distance <=
                    surfaceOffset * 2f
                )
                {
                    continue;
                }

                if (
                    distance <
                    bestDistance
                )
                {
                    bestDistance =
                        distance;

                    bestPoint =
                        intersection;

                    // Нормаль ребра
                    Vector2 edge =
                        worldB - worldA;

                    Vector2 normal =
                        new Vector2(
                            -edge.y,
                            edge.x
                        ).normalized;

                    // Нормаль должна быть направлена
                    // навстречу лучу
                    if (
                        Vector2.Dot(
                            insideDirection,
                            normal
                        ) > 0
                    )
                    {
                        normal = -normal;
                    }

                    bestNormal =
                        normal;

                    found =
                        true;
                }
            }
        }

        if (!found)
        {
            Debug.LogWarning(
                "Выход из линзы не найден."
            );

            return false;
        }

        exitPoint =
            bestPoint;

        // --------------------------------------------------------
        // ВЫХОД: ЛИНЗА → ВОЗДУХ
        // --------------------------------------------------------

        Vector2 outsideDirection;

        bool refractedOut =
            Refract(
                insideDirection,
                bestNormal,
                lens.refractiveIndex,
                airRefractiveIndex,
                out outsideDirection
            );

        if (refractedOut)
        {
            direction =
                outsideDirection.normalized;
        }
        else
        {
            // Полное внутреннее отражение
            direction =
                Vector2.Reflect(
                    insideDirection,
                    bestNormal
                ).normalized;
        }

        return true;
    }

    // ============================================================
    // ПЕРЕСЕЧЕНИЕ ЛУЧА И ОТРЕЗКА
    // ============================================================

    private bool RaySegmentIntersection(
        Vector2 rayOrigin,
        Vector2 rayDirection,
        Vector2 segmentA,
        Vector2 segmentB,
        out float distance,
        out Vector2 intersection)
    {
        distance = 0f;
        intersection = Vector2.zero;

        Vector2 segment =
            segmentB - segmentA;

        Vector2 originToSegment =
            segmentA - rayOrigin;

        float cross =
            Cross(
                rayDirection,
                segment
            );

        // Параллельны
        if (Mathf.Abs(cross) < 0.000001f)
            return false;

        float t =
            Cross(
                originToSegment,
                segment
            ) / cross;

        float u =
            Cross(
                originToSegment,
                rayDirection
            ) / cross;

        // t — расстояние по лучу
        // u — положение на отрезке

        if (t <= 0f)
            return false;

        if (u < 0f || u > 1f)
            return false;

        distance =
            t;

        intersection =
            rayOrigin +
            rayDirection *
            t;

        return true;
    }

    private float Cross(
        Vector2 a,
        Vector2 b)
    {
        return
            a.x * b.y -
            a.y * b.x;
    }

    // ============================================================
    // ЗАКОН СНЕЛЛИУСА
    // ============================================================

    private bool Refract(
        Vector2 direction,
        Vector2 normal,
        float n1,
        float n2,
        out Vector2 refractedDirection)
    {
        direction.Normalize();
        normal.Normalize();

        // Нормаль должна смотреть против луча
        if (
            Vector2.Dot(
                direction,
                normal
            ) > 0f
        )
        {
            normal = -normal;
        }

        float eta =
            n1 / n2;

        float cosI =
            Mathf.Clamp(
                Vector2.Dot(
                    -direction,
                    normal
                ),
                -1f,
                1f
            );

        float sinT2 =
            eta *
            eta *
            (
                1f -
                cosI *
                cosI
            );

        // Полное внутреннее отражение
        if (sinT2 > 1f)
        {
            refractedDirection =
                Vector2.zero;

            return false;
        }

        float cosT =
            Mathf.Sqrt(
                1f -
                sinT2
            );

        refractedDirection =
            eta *
            direction
            +
            (
                eta *
                cosI -
                cosT
            ) *
            normal;

        refractedDirection.Normalize();

        return true;
    }

    // ============================================================
    // ПРОВЕРКА ТОЧКИ A
    // ============================================================

    private void CheckTarget(
        List<Vector3> points)
    {
        if (target == null)
            return;

        bool targetHit =
            false;

        for (
            int i = 0;
            i < points.Count - 1;
            i++)
        {
            Vector2 start =
                points[i];

            Vector2 end =
                points[i + 1];

            Vector2 closestPoint =
                ClosestPointOnSegment(
                    start,
                    end,
                    target.position
                );

            float distance =
                Vector2.Distance(
                    closestPoint,
                    target.position
                );

            if (
                distance <=
                targetRadius
            )
            {
                targetHit =
                    true;

                break;
            }
        }

        if (targetHit)
        {
            target_sprite.color = Color.red;
            Debug.Log(
                "ЛУЧ ПОПАЛ В ТОЧКУ A"
            );
        }
        else
        {
            target_sprite.color = Color.white;
        }

    }

    private Vector2 ClosestPointOnSegment(
        Vector2 a,
        Vector2 b,
        Vector2 point)
    {
        Vector2 ab =
            b - a;

        float length =
            ab.sqrMagnitude;

        if (length < 0.0001f)
            return a;

        float t =
            Vector2.Dot(
                point - a,
                ab
            ) /
            length;

        t =
            Mathf.Clamp01(t);

        return
            a +
            ab * t;
    }
    public void setAngle(float value)
    {
        rayAngle = value;
    }
}