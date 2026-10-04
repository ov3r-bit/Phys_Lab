using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RayEmitter : MonoBehaviour
{
    [Header("Излучатель")]
    public Transform emitter;

    [Header("Параметры луча")]
    public float rayLength = 50f;
    public int maxInterfaces = 20;

    [Header("Смещение от поверхности")]
    public float surfaceOffset = 0.001f;

    [Header("Визуализация")]
    public float lineWidth = 0.05f;
    public Color rayColor = Color.magenta;

    [Header("Отладка")]
    public bool showDebug = true;
    public float markerSize = 0.12f;
    public float labelOffset = 0.25f;

    private LineRenderer line;

    private readonly List<Vector3> points =
        new List<Vector3>();

    private readonly List<InterfaceInfo> interfaces =
        new List<InterfaceInfo>();

    private class InterfaceInfo
    {
        public Vector3 position;

        public float n1;
        public float n2;

        public bool entering;
        public bool totalInternalReflection;

        public string materialName;

        public Vector3 directionBefore;
        public Vector3 directionAfter;
    }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.useWorldSpace = true;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.startColor = rayColor;
        line.endColor = rayColor;
    }

    private void Start()
    {
        if (emitter == null)
            emitter = transform;

        TraceRay();
    }

    private void Update()
    {
        TraceRay();
    }

    private void TraceRay()
    {
        points.Clear();
        interfaces.Clear();

        Vector3 origin = emitter.position;
        Vector3 direction = emitter.forward.normalized;

        points.Add(origin);

        float travelled = 0f;

        for (int i = 0; i < maxInterfaces; i++)
        {
            float remainingDistance =
                rayLength - travelled;

            if (remainingDistance <= 0f)
                break;

            /*
             * =====================================================
             * 1. ИЩЕМ ПЕРВУЮ ПОВЕРХНОСТЬ
             * =====================================================
             */

            if (!Physics.Raycast(
                    origin,
                    direction,
                    out RaycastHit hit,
                    remainingDistance))
            {
                points.Add(
                    origin +
                    direction * remainingDistance
                );

                break;
            }

            OpticalMaterial material =
                hit.collider.GetComponent<OpticalMaterial>();

            if (material == null)
            {
                material =
                    hit.collider
                        .GetComponentInParent<OpticalMaterial>();
            }

            /*
             * Если это не прозрачный материал,
             * продолжаем луч.
             */

            if (material == null)
            {
                points.Add(hit.point);

                travelled += hit.distance;

                origin =
                    hit.point +
                    direction * surfaceOffset;

                travelled += surfaceOffset;

                continue;
            }

            /*
             * =====================================================
             * 2. ВХОД В МАТЕРИАЛ
             * =====================================================
             */

            points.Add(hit.point);

            float n1 = 1.0f;
            float n2 = material.refractiveIndex;

            Vector3 normal = hit.normal;

            /*
             * Нормаль должна смотреть против луча.
             */

            if (Vector3.Dot(direction, normal) > 0f)
                normal = -normal;

            Vector3 refractedDirection;

            bool totalInternalReflection;

            bool refracted =
                TryRefract(
                    direction,
                    normal,
                    n1,
                    n2,
                    out refractedDirection,
                    out totalInternalReflection
                );

            InterfaceInfo enterInfo =
                new InterfaceInfo();

            enterInfo.position = hit.point;
            enterInfo.n1 = n1;
            enterInfo.n2 = n2;
            enterInfo.entering = true;
            enterInfo.materialName =
                material.materialName;
            enterInfo.directionBefore =
                direction;

            enterInfo.totalInternalReflection =
                totalInternalReflection;

            /*
             * =====================================================
             * 3. ЕСЛИ ПОЛНОЕ ВНУТРЕННЕЕ ОТРАЖЕНИЕ
             * =====================================================
             */

            if (!refracted)
            {
                direction =
                    Vector3.Reflect(
                        direction,
                        normal
                    ).normalized;

                enterInfo.directionAfter =
                    direction;

                interfaces.Add(enterInfo);

                origin =
                    hit.point +
                    direction * surfaceOffset;

                travelled +=
                    hit.distance +
                    surfaceOffset;

                continue;
            }

            /*
             * Сохраняем новое направление.
             */

            direction = refractedDirection;

            enterInfo.directionAfter =
                direction;

            interfaces.Add(enterInfo);

            travelled += hit.distance;

            /*
             * =====================================================
             * 4. ИЩЕМ ВЫХОД ИЗ ЭТОГО ЖЕ COLLIDER
             * =====================================================
             *
             * ВАЖНО:
             *
             * Мы НЕ делаем Physics.Raycast изнутри.
             *
             * Создаём точку ЗА объектом и запускаем
             * Collider.Raycast назад.
             */

            Collider materialCollider =
                hit.collider;

            Bounds bounds =
                materialCollider.bounds;

            float outsideDistance =
                bounds.extents.magnitude * 2f + 1f;

            Vector3 outsidePoint =
                hit.point +
                direction * outsideDistance;

            Ray reverseRay =
                new Ray(
                    outsidePoint,
                    -direction
                );

            RaycastHit exitHit;

            bool foundExit =
                materialCollider.Raycast(
                    reverseRay,
                    out exitHit,
                    outsideDistance + rayLength
                );

            /*
             * =====================================================
             * 5. ВЫХОД НАЙДЕН
             * =====================================================
             */

            if (foundExit)
            {
                /*
                 * exitHit.point — задняя поверхность.
                 */

                points.Add(exitHit.point);

                /*
                 * Нормаль поверхности выхода.
                 */

                Vector3 exitNormal =
                    exitHit.normal;

                /*
                 * Для выхода нормаль должна быть
                 * направлена против направления
                 * луча внутри материала.
                 */

                if (Vector3.Dot(
                        direction,
                        exitNormal) > 0f)
                {
                    exitNormal = -exitNormal;
                }

                /*
                 * =============================================
                 * МАТЕРИАЛ → ВОЗДУХ
                 * =============================================
                 */

                float exitN1 =
                    material.refractiveIndex;

                float exitN2 =
                    1.0f;

                Vector3 exitDirection;

                bool exitTotalReflection;

                bool exitRefracted =
                    TryRefract(
                        direction,
                        exitNormal,
                        exitN1,
                        exitN2,
                        out exitDirection,
                        out exitTotalReflection
                    );

                InterfaceInfo exitInfo =
                    new InterfaceInfo();

                exitInfo.position =
                    exitHit.point;

                exitInfo.n1 =
                    exitN1;

                exitInfo.n2 =
                    exitN2;

                exitInfo.entering =
                    false;

                exitInfo.materialName =
                    material.materialName;

                exitInfo.directionBefore =
                    direction;

                exitInfo.totalInternalReflection =
                    exitTotalReflection;

                /*
                 * =============================================
                 * ПОЛНОЕ ВНУТРЕННЕЕ ОТРАЖЕНИЕ
                 * =============================================
                 */

                if (!exitRefracted)
                {
                    direction =
                        Vector3.Reflect(
                            direction,
                            exitNormal
                        ).normalized;
                }
                else
                {
                    direction =
                        exitDirection;
                }

                exitInfo.directionAfter =
                    direction;

                interfaces.Add(exitInfo);

                /*
                 * Расстояние до выхода.
                 */

                float distanceInside =
                    Vector3.Distance(
                        hit.point,
                        exitHit.point
                    );

                travelled +=
                    distanceInside;

                /*
                 * Теперь луч находится СНАРУЖИ материала.
                 */

                origin =
                    exitHit.point +
                    direction * surfaceOffset;

                travelled +=
                    surfaceOffset;

                /*
                 * Продолжаем цикл.
                 *
                 * Следующий Physics.Raycast уже
                 * находится снаружи материала.
                 */

                continue;
            }

            /*
             * =====================================================
             * ЕСЛИ ВЫХОД НЕ НАЙДЕН
             * =====================================================
             */

            Debug.LogWarning(
                "Не удалось найти выход из материала: " +
                material.materialName
            );

            origin =
                hit.point +
                direction * surfaceOffset;

            travelled +=
                hit.distance +
                surfaceOffset;
        }

        /*
         * =========================================================
         * LINE RENDERER
         * =========================================================
         */

        line.positionCount =
            points.Count;

        line.SetPositions(
            points.ToArray()
        );
    }

    /*
     * =============================================================
     * ЗАКОН СНЕЛЛИУСА
     * =============================================================
     */

    private bool TryRefract(
        Vector3 incident,
        Vector3 normal,
        float n1,
        float n2,
        out Vector3 refracted,
        out bool totalInternalReflection)
    {
        incident.Normalize();
        normal.Normalize();

        float cosI =
            Mathf.Clamp(
                -Vector3.Dot(
                    incident,
                    normal
                ),
                -1f,
                1f
            );

        float eta =
            n1 / n2;

        float sinT2 =
            eta * eta *
            (1f - cosI * cosI);

        /*
         * Полное внутреннее отражение.
         */

        if (sinT2 > 1f)
        {
            refracted = Vector3.zero;

            totalInternalReflection = true;

            return false;
        }

        float cosT =
            Mathf.Sqrt(
                1f - sinT2
            );

        refracted =
            eta * incident +
            (
                eta * cosI -
                cosT
            ) * normal;

        refracted.Normalize();

        totalInternalReflection = false;

        return true;
    }

#if UNITY_EDITOR

    /*
     * =============================================================
     * ОТЛАДОЧНЫЕ ТОЧКИ И НАДПИСИ
     * =============================================================
     */

    private void OnDrawGizmosSelected()
    {
        if (!showDebug)
            return;

        foreach (InterfaceInfo info in interfaces)
        {
            /*
             * Зелёный = вход
             * Красный = выход
             */

            UnityEditor.Handles.color =
                info.entering
                    ? Color.green
                    : Color.red;

            UnityEditor.Handles.SphereHandleCap(
                0,
                info.position,
                Quaternion.identity,
                markerSize,
                EventType.Repaint
            );

            string type;

            if (info.totalInternalReflection)
            {
                type =
                    "ПОЛНОЕ ВНУТРЕННЕЕ ОТРАЖЕНИЕ";
            }
            else if (info.entering)
            {
                type = "ВХОД";
            }
            else
            {
                type = "ВЫХОД";
            }

            string text =
                type +
                "\n" +
                info.materialName +
                "\n" +
                $"n1 → n2: " +
                $"{info.n1:F4} → {info.n2:F4}";

            UnityEditor.Handles.Label(
                info.position +
                Vector3.up * labelOffset,
                text
            );

            /*
             * Жёлтая стрелка —
             * направление луча после границы.
             */

            UnityEditor.Handles.color =
                Color.yellow;

            if (info.directionAfter != Vector3.zero)
            {
                UnityEditor.Handles.ArrowHandleCap(
                    0,
                    info.position,
                    Quaternion.LookRotation(
                        info.directionAfter
                    ),
                    0.4f,
                    EventType.Repaint
                );
            }
        }
    }

#endif
}