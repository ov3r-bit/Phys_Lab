using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(OpticalMaterial))]
public class Lens : MonoBehaviour
{
    [Header("Тип линзы")]
    public LensType lensType = LensType.Convex;

    public enum LensType
    {
        Convex,
        Concave,
        PlanoConvex,
        PlanoConcave
    }

    private MeshCollider meshCollider;
    private OpticalMaterial opticalMaterial;

    private void Awake()
    {
        Setup();
    }

    private void Reset()
    {
        Setup();
    }

    private void Setup()
    {
        meshCollider = GetComponent<MeshCollider>();
        opticalMaterial = GetComponent<OpticalMaterial>();

        /*
         * Используем реальную геометрию готовой модели.
         */
        meshCollider.convex = false;
        meshCollider.isTrigger = false;
    }

    private void OnValidate()
    {
        meshCollider = GetComponent<MeshCollider>();

        if (meshCollider != null)
        {
            meshCollider.convex = false;
            meshCollider.isTrigger = false;
        }
    }
}