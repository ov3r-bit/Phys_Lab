using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmitterSlider : MonoBehaviour
{
    [Header("Излучатель")]
    public Transform emitter;

    [Header("Slider")]
    public Slider angleSlider;

    [Header("Текст")]
    public TMP_Text angleText;

    [Header("Диапазон")]
    public float minAngle = -60f;
    public float maxAngle = 60f;

    private void Start()
    {
        angleSlider.minValue = minAngle;
        angleSlider.maxValue = maxAngle;

        angleSlider.onValueChanged.AddListener(ChangeAngle);

        ChangeAngle(angleSlider.value);
    }

    private void ChangeAngle(float angle)
    {
        /*
         * Поворачиваем излучатель вокруг Y.
         */
        emitter.localRotation =
            Quaternion.Euler(
                0f,
                angle,
                0f
            );

        if (angleText != null)
        {
            angleText.text =
                $"Угол: {angle:F1}°";
        }
    }

    private void OnDestroy()
    {
        angleSlider.onValueChanged.RemoveListener(ChangeAngle);
    }
}