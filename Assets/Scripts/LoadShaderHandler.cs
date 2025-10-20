using UnityEngine;

public class LoadShaderHandler : MonoBehaviour
{
    void Update()
    {
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
    }
}
