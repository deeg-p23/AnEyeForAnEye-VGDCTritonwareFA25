using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject loadingMask;
    [SerializeField] private GameObject resultsScreen;

    public bool pauseScreenAccessable = false;
    public bool gameIsPaused = false;

    [SerializeField] private float fadeSpeed = 10f;

    // --- Pause Controls Modal ---
    [SerializeField] private GameObject PanelMenuImage;    // the main pause menu panel/image
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image pauseBackground;
    
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
        GameManager.Instance.gameIsRunning = !isPaused;

        if (gameIsPaused)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
            SoundManager.Instance.PauseMusic();
            SoundManager.Instance.Play(SoundManager.SoundType.Pause);
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
            SoundManager.Instance.ResumeMusic();
            if (GameManager.Instance.gameRuntime > 0.5f) SoundManager.Instance.Play(SoundManager.SoundType.Unpause);
        }
    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeCountdownCoroutine());
    }

    private IEnumerator ResumeCountdownCoroutine()
    {
        // Disable pausing during countdown
        pauseScreenAccessable = false;

        // Disable menu UI
        PanelMenuImage.SetActive(false);

        // Enable countdown UI
        countdownText.gameObject.SetActive(true);

        // Ensure game is still paused during countdown
        gameIsPaused = true;
        GameManager.Instance.gameIsRunning = false;

        // Start BG fade
        float elapsed = 0f;
        Color bgColor = pauseBackground.color;
        
        // Count down 3..2..1
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            SoundManager.Instance.Play(SoundManager.SoundType.Clock_Tick);

            float segmentStart = elapsed;
            float segmentEnd = segmentStart + 1f;

            while (elapsed < segmentEnd)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 3f);
                bgColor.a = Mathf.Lerp(0.8f, 0f, t);
                pauseBackground.color = bgColor;
                yield return null;
            }
        }

        // Countdown finished → resume game
        countdownText.gameObject.SetActive(false);
        SetGamePaused(false);
        
        // Re-enable menu UI
        PanelMenuImage.SetActive(true);

        bgColor.a = 0.8f;
        pauseBackground.color = bgColor;
        
        // Re-enable pausing
        pauseScreenAccessable = true;
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
            Debug.Log("stuck in closer " + elapsed);
            SoundManager.Instance.SetMusicVolume(1f - elapsed / duration);
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
        
        Debug.Log("IM GOT THERE");
        
        while (elapsed < duration)
        {
            Debug.Log(elapsed);
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
        
        resultsScreen.SetActive(false);

        do
        {
            Debug.Log("stuck in next frame");
            await Awaitable.NextFrameAsync();
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        // Time.timeScale = 1f;

        Debug.Log("wtf");
        
        HandleManagerStates(sceneName);

        Debug.Log("IM GETTING THERE");
        
        await OpenSceneFromLoad();
        await WaitForStableFrame();

        loadingScreen.SetActive(false);
    }
    
    [Header("UI References")]
    [SerializeField] private RectTransform resultsPanel;
    [SerializeField] private RectTransform iconsPanel;
    [SerializeField] private RectTransform crown;
    [SerializeField] private TMP_Text playerTotalA;
    [SerializeField] private TMP_Text playerTotalB;
    [SerializeField] private TMP_Text playerATitle;
    [SerializeField] private TMP_Text playerBTitle;
    [SerializeField] private RectTransform buttonsPanel;

    [Header("Config")]
    [SerializeField] private float introDuration = 1f;
    [SerializeField] private float oscillationDuration = 5f;
    [SerializeField] private float crownOscillationAmplitude = 350f;
    [SerializeField] private float crownOscillationSpeed = 2f;
    [SerializeField] private float titlePopScaleVictor = 1.3f;
    [SerializeField] private float titlePopScaleLoser = 1.0f;
    [SerializeField] private float buttonExpandMaxY = 0.75f;

    private Coroutine oscillationCoroutineA;
    private Coroutine oscillationCoroutineB;

    private Color victorColor = new Color32(0xF5, 0xBA, 0x3F, 0xFF);
    private Color loserColor  = new Color32(0xBD, 0x27, 0x65, 0xFF);
    private Color tiedColor   = new Color32(0xAF, 0xAF, 0xAF, 0xFF);

    [SerializeField] private Image resultBG;
    [SerializeField] private Image resultVig;
    [SerializeField] private float bgTargetAlpha = 0.8f;
    [SerializeField] private float vigTargetAlpha = 1.0f;
    
    public IEnumerator ResultsCoroutine()
    {
        // compute end game scores
        int playerAScore = GameManager.Instance.playerA.GetTotalEyes() * 15 + GameManager.Instance.playerA.GetTotalEyes();
        int playerBScore = GameManager.Instance.playerB.GetTotalEyes() * 15 + GameManager.Instance.playerB.GetTotalEyes();

        resultsScreen.SetActive(true);
        
        yield return new WaitForSecondsRealtime(2f);
        
        StartCoroutine(FadeInBackgrounds(1f));
        
        // 1. Animate Results Panel bottom -> 0
        Vector2 startResultsPos = resultsPanel.anchoredPosition;
        startResultsPos.y = 500;
        resultsPanel.anchoredPosition = startResultsPos;
        yield return StartCoroutine(ExponentialMoveY(resultsPanel, 0, introDuration));

        // 2. Animate Icons Panel scale 0 -> 1
        iconsPanel.localScale = Vector3.zero;
        yield return StartCoroutine(ExponentialScale(iconsPanel, Vector3.one, introDuration));

        // 2.5 Play fanfare sound
        SoundManager.Instance.Play(SoundManager.SoundType.BG_Fanfare);

        // 3. Oscillate crown + count player scores for 5s
        float timer = 0f;
        while (timer < oscillationDuration)
        {
            // oscillate crown left-right
            float oscillation = Mathf.Sin(timer * crownOscillationSpeed) * crownOscillationAmplitude;
            Vector2 crownPos = crown.anchoredPosition;
            crownPos.x = oscillation;
            crown.anchoredPosition = crownPos;

            // count totals
            float lerpT = timer / oscillationDuration;
            playerTotalA.text = Mathf.FloorToInt(lerpT * playerAScore).ToString();
            playerTotalB.text = Mathf.FloorToInt(lerpT * playerBScore).ToString();

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        Debug.Log(playerAScore);
        Debug.Log(playerBScore);
        
        playerTotalA.text = playerAScore.ToString();
        playerTotalB.text = playerBScore.ToString();

        // 4. Determine winner
        if (playerAScore > playerBScore)
        {
            yield return StartCoroutine(InterpolateCrownAndTitles(-crownOscillationAmplitude, 
                "VICTOR!", "LOSER...", victorColor, loserColor, titlePopScaleVictor, titlePopScaleLoser));
        }
        else if (playerBScore > playerAScore)
        {
            yield return StartCoroutine(InterpolateCrownAndTitles(crownOscillationAmplitude, 
                "LOSER...", "VICTOR!", loserColor, victorColor, titlePopScaleLoser, titlePopScaleVictor));
        }
        else
        {
            yield return StartCoroutine(InterpolateCrownAndTitles(0, 
                "TIED", "TIED", tiedColor, tiedColor, 1f, 1f));
        }

        // 5. Post-animation idle oscillations
        oscillationCoroutineA = StartCoroutine(IdleScaleOscillation(playerATitle.rectTransform, titlePopScaleVictor, 1.6f, 1f));
        oscillationCoroutineB = StartCoroutine(IdleRotationOscillation(playerBTitle.rectTransform, -6f, 6f, 1.5f));

        // 6. Expand buttons panel max.y = 0 -> 0.75 exponential decay in 1s
        Vector2 btnSize = buttonsPanel.anchorMax;
        btnSize.y = 0f;
        buttonsPanel.anchorMax = btnSize;
        yield return StartCoroutine(ExponentialAnchorMaxY(buttonsPanel, buttonExpandMaxY, introDuration));
    }

    // ------------------------- HELPERS -------------------------
    private IEnumerator FadeInBackgrounds(float duration = 1f)
    {
        Color bgStart = resultBG.color;
        Color vigStart = resultVig.color;
        float elapsed = 0f;

        // make sure initial alpha is 0
        bgStart.a = 0f;
        vigStart.a = 0f;
        resultBG.color = bgStart;
        resultVig.color = vigStart;

        Color bgEnd = bgStart;
        Color vigEnd = vigStart;
        bgEnd.a = bgTargetAlpha;
        vigEnd.a = vigTargetAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Exp(-5f * (elapsed / duration)); // exponential ease
            resultBG.color = Color.Lerp(bgStart, bgEnd, t);
            resultVig.color = Color.Lerp(vigStart, vigEnd, t);
            yield return null;
        }

        resultBG.color = bgEnd;
        resultVig.color = vigEnd;
    }

    
    
    private IEnumerator ExponentialMoveY(RectTransform target, float endY, float duration)
    {
        Vector2 startPos = target.anchoredPosition;
        float startY = startPos.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Exp(-5f * elapsed / duration); // exponential ease
            startPos.y = Mathf.Lerp(startY, endY, t);
            target.anchoredPosition = startPos;
            yield return null;
        }
        startPos.y = endY;
        target.anchoredPosition = startPos;
    }

    private IEnumerator ExponentialScale(RectTransform target, Vector3 endScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Exp(-5f * elapsed / duration);
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        target.localScale = endScale;
    }

    private IEnumerator ExponentialAnchorMaxY(RectTransform target, float endY, float duration)
    {
        Vector2 startMax = target.anchorMax;
        float startY = startMax.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Exp(-5f * elapsed / duration);
            startMax.y = Mathf.Lerp(startY, endY, t);
            target.anchorMax = startMax;
            yield return null;
        }
        startMax.y = endY;
        target.anchorMax = startMax;
    }

    private IEnumerator InterpolateCrownAndTitles(
        float crownXTarget,
        string aTitleText,
        string bTitleText,
        Color aColor,
        Color bColor,
        float aScale,
        float bScale
    )
    {
        // Crown
        Vector2 startPos = crown.anchoredPosition;
        Vector2 endPos = startPos;
        endPos.x = crownXTarget;

        // Titles
        playerATitle.text = aTitleText;
        playerBTitle.text = bTitleText;
        playerATitle.color = aColor;
        playerBTitle.color = bColor;

        playerATitle.rectTransform.localScale = Vector3.zero;
        playerBTitle.rectTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            crown.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            playerATitle.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * aScale, t);
            playerBTitle.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * bScale, t);
            yield return null;
        }

        crown.anchoredPosition = endPos;
        playerATitle.rectTransform.localScale = Vector3.one * aScale;
        playerBTitle.rectTransform.localScale = Vector3.one * bScale;
    }

    private IEnumerator IdleScaleOscillation(RectTransform target, float minScale, float maxScale, float speed)
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
            float s = Mathf.Lerp(minScale, maxScale, t);
            target.localScale = Vector3.one * s;
            yield return null;
        }
    }

    private IEnumerator IdleRotationOscillation(RectTransform target, float minAngle, float maxAngle, float speed)
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
            float angle = Mathf.Lerp(minAngle, maxAngle, t);
            target.localEulerAngles = new Vector3(0, 0, angle);
            yield return null;
        }
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
