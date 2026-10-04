using UnityEngine;

public class ElasticCollision3D : MonoBehaviour
{
    public Rigidbody object1;
    public Rigidbody object2;

    [Header("Параметры")]
    public float mass1 = 1f;
    public float mass2 = 1f;

    public Vector3 velocity1 = new Vector3(5f, 0f, 0f);
    public Vector3 velocity2 = Vector3.zero;

    public bool isNonCentral = false;

    private Vector3 startPosition1;
    private Vector3 startPosition2;

    private bool collisionHappened = false;

    private void Start()
    {
        startPosition1 = object1.position;
        startPosition2 = object2.position;

        ResetSimulation(0);
    }

    private void FixedUpdate()
    {
        if (collisionHappened)
            return;

        // Проверяем расстояние между объектами
        float distance = Vector3.Distance(
            object1.position,
            object2.position
        );

        float radius1 = object1.GetComponent<SphereCollider>().radius;
        float radius2 = object2.GetComponent<SphereCollider>().radius;

        if (distance <= radius1 + radius2)
        {
            CalculateCollision();
            collisionHappened = true;
        }
    }

    public void StartSimulation()
    {
        collisionHappened = false;

        object1.mass = mass1;
        object2.mass = mass2;

        object1.linearVelocity = velocity1;
        object2.linearVelocity = velocity2;
    }

    public void ResetSimulation(int task)
    {
        collisionHappened = false;


        if(task==0){
        object1.position = startPosition1+Vector3.forward*0.1f;}
        else
        {
            object1.position = startPosition1;
        }
        object2.position = startPosition2;

        object1.linearVelocity = Vector3.zero;
        object2.linearVelocity = Vector3.zero;

        object1.angularVelocity = Vector3.zero;
        object2.angularVelocity = Vector3.zero;

        object1.mass = mass1;
        object2.mass = mass2;
    }

    private void CalculateCollision()
    {
        // Направление от первого объекта ко второму
        Vector3 normal =
            (object2.position - object1.position).normalized;

        // Скорости до столкновения
        Vector3 v1 = object1.linearVelocity;
        Vector3 v2 = object2.linearVelocity;

        // Проекция скорости на направление столкновения
        float v1n = Vector3.Dot(v1, normal);
        float v2n = Vector3.Dot(v2, normal);

        // Формулы упругого столкновения
        float v1nAfter =
            ((mass1 - mass2) * v1n +
             2f * mass2 * v2n)
            / (mass1 + mass2);

        float v2nAfter =
            ((mass2 - mass1) * v2n +
             2f * mass1 * v1n)
            / (mass1 + mass2);

        // Тангенциальные составляющие
        Vector3 v1T = v1 - v1n * normal;
        Vector3 v2T = v2 - v2n * normal;

        // Итоговые скорости
        Vector3 resultV1 =
            v1T + v1nAfter * normal;

        Vector3 resultV2 =
            v2T + v2nAfter * normal;

        object1.linearVelocity = resultV1;
        object2.linearVelocity = resultV2;

        Debug.Log("=== УПРУГИЙ УДАР ===");
        Debug.Log("Объект 1 после удара: " + resultV1);
        Debug.Log("Объект 2 после удара: " + resultV2);
    }
}