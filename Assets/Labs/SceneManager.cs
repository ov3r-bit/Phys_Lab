using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public void LoadSceneByIndex(int index)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(index);
    }
}
