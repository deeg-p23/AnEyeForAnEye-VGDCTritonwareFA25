using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject loadingMask;

    public bool pauseScreenAccessable = false;
    public bool gameIsPaused = false;

    [SerializeField] private float fadeSpeed = 10f;

    // --- Pause Controls Modal ---
    [SerializeField] private GameObject PanelMenuImage;    // the main pause menu panel/image
    
    void Update()
    {
        // Set global shader param for loading screen
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
        
        if (!pauseScreenAccessable) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetGamePaused(!gameIsPaused);
        }
    }
    /*
    private void AcquirePlayerStates()
    {
        // Try common patterns: tag, name, or fallback
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj == null)
        {
            playerStates = FindObjectOfType<PlayerStates>(); // last resort
        }
        else
        {
            playerStates = playerObj.GetComponent<PlayerStates>();
        }
    }
    */
    
    public void SetGamePaused(bool isPaused)
    {
        gameIsPaused = isPaused;

        if (gameIsPaused)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
        }
    }

    void Start()
    {
        StartCoroutine(LateShaderUpdater());
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ProgressDelay()
    {
        print(Time.time);
        yield return new WaitForSecondsRealtime(5f);
        print(Time.time);
    }

    // very scuffed state-based setter
    private void HandleManagerStates(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                pauseScreenAccessable = false;
                SetGamePaused(false);

                pauseScreen.SetActive(false);
                // AcquirePlayerStates();

                break;
            default:
                pauseScreenAccessable = true;
                SetGamePaused(false);

                pauseScreen.SetActive(false);
                // playerStates = null;

                break;
        }
    }
    
    private async Task CloseSceneToLoad()
    {
        float duration = 1f;
        float elapsed = 0f;
        float startFade = 1f;   // fully opaque
        float endFade = 0f;     // fully transparent

        // Set initial fade
        loadingMask.GetComponent<Image>().material.SetFloat("_Fade", startFade);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float fadeValue = Mathf.Lerp(startFade, endFade, t);
            loadingMask.GetComponent<Image>().material.SetFloat("_Fade", fadeValue);
            await Awaitable.NextFrameAsync();
        }

        loadingMask.GetComponent<Image>().material.SetFloat("_Fade", endFade);
    }

    private async Task OpenSceneFromLoad()
    {
        float duration = 1f;
        float elapsed = 0f;
        float startFade = 0f;   // fully transparent
        float endFade = 1f;     // fully opaque
        
        // Set initial fade
        loadingMask.GetComponent<Image>().material.SetFloat("_Fade", startFade);
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float fadeValue = Mathf.Lerp(startFade, endFade, t);
            loadingMask.GetComponent<Image>().material.SetFloat("_Fade", fadeValue);
            await Awaitable.NextFrameAsync();
        }
        
        loadingMask.GetComponent<Image>().material.SetFloat("_Fade", endFade);
    }


    // wait helper: checks that frames are stable after completing a scene load before playing UI animations.
    private async Task WaitForStableFrame(float maxDelta = 0.05f, int stableFrames = 2, float timeout = 1f)
    {
        float start = Time.realtimeSinceStartup;
        int ok = 0;
        while (ok < stableFrames && (Time.realtimeSinceStartup - start) < timeout)
        {
            await Awaitable.NextFrameAsync();
            ok = (Time.unscaledDeltaTime <= maxDelta) ? ok + 1 : 0;
        }
    }

    public async Task LoadScene(string sceneName)
    {
        // player cannot toggle pause, and non-time-based processes in game remain paused
        pauseScreenAccessable = false;

        AsyncOperation scene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        scene.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        await CloseSceneToLoad();

        do
        {
            await Awaitable.NextFrameAsync();
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;

        while (!scene.isDone)
        {
            await Task.Yield(); 
        }

        HandleManagerStates(sceneName);

        await WaitForStableFrame();
        await OpenSceneFromLoad();

        loadingScreen.SetActive(false);
    }
    
    public async void CloseSceneLoad()
    {
        
    }
    
    private IEnumerator LateShaderUpdater()
    {
        while (true)
        {
            Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
            yield return null;
        }
    }
}
