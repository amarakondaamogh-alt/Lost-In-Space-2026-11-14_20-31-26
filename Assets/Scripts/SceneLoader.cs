using UnityEngine;
using UnityEngine.SceneManagement;

// Small helper so any UI button can load a scene by name.
public class SceneLoader : MonoBehaviour
{
    public void Load(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
