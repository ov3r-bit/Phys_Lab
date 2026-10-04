using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonSwitcher : MonoBehaviour
{
    public GameObject[] materials;
    public GameObject lens;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Switch(int index)
    {
        if (index == 1)
        {
            lens.SetActive(true);
            foreach (var material in materials)
            {
                material.SetActive(false);
            }
        }
        else
        {
            lens.SetActive(false);
            foreach (var material in materials)
            {
                material.SetActive(true);
            }
        }
        
    }
}
