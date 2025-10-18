using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("Animated Title Motion")]
    public RectTransform logo;            // If left null, uses this object

    [Header("X Motion")]
    [Range(0f, 0.3f)] public float xAmplitude = 0.08f;
    [Range(0.1f, 3f)] public float xFrequency = 0.9f; // cycles per second
    [Range(0f, 360f)] public float xPhaseDegrees = 0f; // phase offset in degrees

    [Header("Y Motion")]
    [Range(0f, 0.3f)] public float yAmplitude = 0.08f;
    [Range(0.1f, 3f)] public float yFrequency = 0.9f; // cycles per second
    [Range(0f, 360f)] public float yPhaseDegrees = 0f; // phase offset in degrees

    [Header("General Motion Shaping")]
    [Range(1f, 3f)] public float easePower = 1.7f;  // >1 flattens peaks, feels "bouncy"
    public bool useUnscaledTime = true;             // animate even if paused

    Vector3 _baseScale;

    public Button controlsButton;

    void Awake()
    {
        if (logo == null) logo = GetComponent<RectTransform>();
        _baseScale = logo.localScale;
        
        // reconnect controlsButton onClick to UIManager
        // controlsButton.onClick.AddListener(() => SceneManager.Instance.PauseMenuOpenControlModal());
    }

    void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        // Convert inspector degrees to radians
        float xPhase = xPhaseDegrees * Mathf.Deg2Rad;
        float yPhase = yPhaseDegrees * Mathf.Deg2Rad;

        // X wave
        float xWave = Mathf.Sin((time * xFrequency * Mathf.PI * 2f) + xPhase);
        xWave = Mathf.Sign(xWave) * Mathf.Pow(Mathf.Abs(xWave), easePower);

        // Y wave
        float yWave = Mathf.Sin((time * yFrequency * Mathf.PI * 2f) + yPhase);
        yWave = Mathf.Sign(yWave) * Mathf.Pow(Mathf.Abs(yWave), easePower);

        // Apply independent scaling
        float x = 1f + xAmplitude * xWave;
        float y = 1f + yAmplitude * yWave;

        logo.localScale = new Vector3(_baseScale.x * x, _baseScale.y * y, _baseScale.z);
    }

    void OnDisable()
    {
        if (logo != null) logo.localScale = _baseScale;
    }
}