using UnityEngine;
using UnityEngine.SceneManagement;

public class TempSceneLoader : MonoBehaviour
{
    public void SceneLoader(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    } 
}
