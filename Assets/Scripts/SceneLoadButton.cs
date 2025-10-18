using UnityEngine;

public class SceneLoadButton : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        // SoundManager.Instance.Play(SoundManager.SoundType.left_click);
        SceneManager.Instance.LoadScene(sceneName);
    }
}
