using UnityEngine;
using TMPro;


public class OpticalMaterial : MonoBehaviour
{
    [Min(1.0f)]
    public float refractiveIndex = 1.5163f;

    [Tooltip("Optional: used only for display in the inspector.")]
    public string materialName = "K8 (Крон)";

    public TMP_InputField thicknessInputField;


    void OnEnable()
    {
        if (thicknessInputField != null)
        {
            thicknessInputField.onValueChanged.AddListener(OnTextChanged);
        }
    }
    void OnDisable()
    {
        if (thicknessInputField != null)
        {
            thicknessInputField.onValueChanged.RemoveListener(OnTextChanged);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var c = GetComponent<Collider>();
        if (c != null) Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
    }


    public void SetThickness(string thickness)
    {
        if (int.TryParse(thickness, out int result))
        {
            Vector3 newscale = transform.localScale;
            newscale.z = result;
            transform.localScale = newscale;

            

        }
        else{
            Debug.LogWarning($"Invalid thickness value: {thickness}");
        }
    }
    void OnTextChanged(string text)
    {
        SetThickness(text);
    }


}
