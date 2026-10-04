using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LensMesh : MonoBehaviour
{
    public LensShape lensShape;

    private void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        if (lensShape == null)
        {
            Debug.LogError(
                "LensMesh: Lens Shape не назначен!"
            );

            return;
        }

        PolygonCollider2D polygon =
            lensShape.GetComponent<PolygonCollider2D>();

        if (polygon == null)
        {
            Debug.LogError(
                "LensMesh: PolygonCollider2D не найден!"
            );

            return;
        }

        Vector2[] points =
            polygon.points;

        if (points.Length < 4)
        {
            Debug.LogError(
                "LensMesh: недостаточно точек!"
            );

            return;
        }

        int sideCount =
            points.Length / 2;

        Vector3[] vertices =
            new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] =
                new Vector3(
                    points[i].x,
                    points[i].y,
                    0f
                );
        }

        int stripCount =
            sideCount - 1;

        int[] triangles =
            new int[stripCount * 6];

        int t = 0;

        for (int i = 0; i < stripCount; i++)
        {
            int leftBottom = i;
            int leftTop = i + 1;

            int rightBottom =
                points.Length - 1 - i;

            int rightTop =
                points.Length - 2 - i;

            triangles[t++] =
                leftBottom;

            triangles[t++] =
                rightBottom;

            triangles[t++] =
                leftTop;

            triangles[t++] =
                leftTop;

            triangles[t++] =
                rightBottom;

            triangles[t++] =
                rightTop;
        }

        Mesh mesh =
            new Mesh();

        mesh.name =
            "Lens Mesh";

        mesh.vertices =
            vertices;

        mesh.triangles =
            triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh =
            mesh;
    }
}