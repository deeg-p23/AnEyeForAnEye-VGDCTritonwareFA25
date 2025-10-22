using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CalloutManager cm;
    
    // VITAL GAME-TIME STATES
    public bool gameIsRunning = false;
    public float gameRuntime = 0f;
    private const float evilHourKickoff = 135f; // EVIL HOUR has begun! SABOTAGE your opponent! [Or face the consequences...]
    private const float evilHourCloseoff = 165f; // EVIL HOUR has ended!
    private const float rushHourKickoff = 240f; // ONE MINUTE left! [Pick up the pace!]
    private const float rushHourCloseoff = 290f; // TEN SECONDS left! [Finish that potion!]
    private const float gameEndtime = 300f;

    private bool ReachedEvilHourKickoff = false;
    private bool ReachedEvilHourCloseoff = false;
    private bool ReachedRushHourKickoff = false;
    private bool ReachedRushHourCloseoff = false;
    
    public static float GrabTimerMax = 0.15f;
    public static float MixFrameMax = 0.1f;

    private float _bgmCrotchet; // time duration of a beat based on BGM's current tempo
    
    public Sprite heartSprite;
    public Sprite frogSprite;
    public Sprite ravenSprite;
    public Sprite emptySprite;

    public PlayerController playerA;
    public PlayerController playerB;

    public IngredientItem upItemA;
    public IngredientItem rightItemA;
    public IngredientItem leftItemA;
    public IngredientItem upItemB;
    public IngredientItem rightItemB;
    public IngredientItem leftItemB;

    public GameObject metronomeA;
    public RectTransform metronomePivotA;
    public GameObject metronomeB;
    public RectTransform metronomePivotB;

    public GameObject pourMeterA;
    public Image pourFillA;
    private Slider _pourSliderA;
    public Slider stirSliderA;
    public GameObject pourMeterB;
    public Image pourFillB; 
    private Slider _pourSliderB;
    public Slider stirSliderB;

    public Image eyeStem1A;
    public Image eyeStem2A;
    public Image eyeStem3A;
    public Image eyeStem1B;
    public Image eyeStem2B;
    public Image eyeStem3B;

    public CharacterSprite eyePlotSprites;
    public GameObject harvestMeterA;
    public GameObject harvestMeterB;
    private Slider _harvestSliderA;
    private Slider _harvestSliderB;

    public TMP_Text eyeCounterA;
    public TMP_Text eyeCounterB;
    public TMP_Text scoreCounterA;
    public TMP_Text scoreCounterB;
    
    // SABOTAGE references
    public GameObject inkSabotageA;
    public GameObject inkSabotageB;
    public Material ingredientMaterialA;
    public Material ingredientMaterialB;

    private float _tempoModifierA = 1f;
    private float _tempoModifierB = 1f;
    private float _bgmCrotchetA;
    private float _bgmCrotchetB;
    
    // RECIPE OBJECTS
    public Recipe[] recipes;
    
    int[][] _eyePlotSpriteIndices = new int[][]
    {
        new int[] {0, 0, 0}, // 0
        new int[] {1, 0, 0}, // 1
        new int[] {2, 0, 0}, // 2
        new int[] {3, 0, 0}, // 3
        new int[] {3, 1, 0}, // 4
        new int[] {3, 2, 0}, // 5
        new int[] {3, 3, 0}, // 6
        new int[] {3, 3, 1}, // 7
        new int[] {3, 3, 2}, // 8
        new int[] {3, 3, 3}  // 9
    };

    public RectTransform clockPivot;
    
    // SABOTAGE SPINNING / ROUTINE REFERENCE VARS
    private float _spinTime; // spinning lasts 4 seconds
    private int _spinnerID; // 0 if player A spun, 1 if player B spun
    private bool _spinning; // if true, wheels cannot be re-spun
    private float _spinVelConstant;
    private float _spinExpConstant;

    private bool _playerAIsEvil;
    private bool _playerBIsEvil;

    public RectTransform sabotageWheel;
    public RectTransform paymentWheel;

    private Coroutine slowStirRoutineA;
    private Coroutine slowStirRoutineB;
    private Coroutine inkRoutineA;
    private Coroutine inkRoutineB;
    private Coroutine randomSwapRoutineA;
    private Coroutine randomSwapRoutineB;
    private Coroutine stickyRoutineA;
    private Coroutine stickyRoutineB;

    private System.Random rng;
    
    // debuggers

    public Slider debugCrotchet;
    
    void Start()
    {
        // initializing stuffs
        Instance = this;
        playerA.SetID(0);
        playerB.SetID(1);
        
        metronomeA.SetActive(false);
        metronomeB.SetActive(false);
        pourMeterA.SetActive(false);
        pourMeterB.SetActive(false);
        harvestMeterA.SetActive(false);
        harvestMeterB.SetActive(false);
        
        _pourSliderA = pourMeterA.GetComponent<Slider>();
        _pourSliderB = pourMeterB.GetComponent<Slider>();
        _harvestSliderA = harvestMeterA.GetComponent<Slider>();
        _harvestSliderB = harvestMeterB.GetComponent<Slider>();
        
        // adding default player inputs to input dictionary
        playerA.Inputs = new Dictionary<string, KeyCode>();
        playerB.Inputs = new Dictionary<string, KeyCode>();
        
        playerA.Inputs.Add("GrabUp", KeyCode.W);
        playerA.Inputs.Add("GrabRight", KeyCode.D);
        playerA.Inputs.Add("GrabLeft",KeyCode.A);
        playerA.Inputs.Add("Mix", KeyCode.S);
        playerA.Inputs.Add("Harvest", KeyCode.E);
        playerA.Inputs.Add("Spin", KeyCode.R);

        playerB.Inputs.Add("GrabUp", KeyCode.I);
        playerB.Inputs.Add("GrabRight", KeyCode.L);
        playerB.Inputs.Add("GrabLeft",KeyCode.J);
        playerB.Inputs.Add("Mix", KeyCode.K);
        playerB.Inputs.Add("Harvest", KeyCode.O);
        playerB.Inputs.Add("Spin", KeyCode.P);
        
        playerA.SetRecipe(GenerateRecipe());
        playerB.SetRecipe(GenerateRecipe());

        // SoundManager.Instance.SetMusicTime(240f);
        
        // poorly randomizing item placements but wtv right
        rng = new System.Random();
        
        IngredientItem.ItemType[] itemsToStore = new IngredientItem.ItemType[] 
            { IngredientItem.ItemType.Heart, IngredientItem.ItemType.Frog, IngredientItem.ItemType.Raven };

        int initialSize = itemsToStore.Length;
        for (int i = 0; i < initialSize; i++)
        {
            int j = rng.Next(0, itemsToStore.Length);
        }

        RandomizeIngredients(upItemA, rightItemA, leftItemA, itemsToStore, rng);
        RandomizeIngredients(upItemB, rightItemB, leftItemB, itemsToStore, rng);

        BeginGame();
    }

    void setMetronome(int id, float tempo)
    {
        float refCrotchet = 60f / tempo;
        if (id == 0) _bgmCrotchetA = 60f / tempo;
        else if (id == 1) _bgmCrotchetB = 60f / tempo;
        
        float bgm_t = SoundManager.Instance.GetMusicTime();
        bgm_t += (bgm_t < 240) ? 0.33f : 0.6f;
        
        float n = Mathf.Floor(bgm_t / refCrotchet);
        float f = (bgm_t - n * refCrotchet) / refCrotchet;
        float offset = 100f * (1f - Mathf.Pow(-1f, n)) / 2f;
        float angle = Mathf.Pow(-1f, n) * 100f * f + offset - 50f;
        if (id == 0) metronomePivotA.rotation = Quaternion.Euler(0f, 0f,angle);
        else if (id == 1) metronomePivotB.rotation = Quaternion.Euler(0f, 0f,angle);
    }

    void BeginGame()
    {
        gameIsRunning = true;
        SoundManager.Instance.Play(SoundManager.SoundType.BG_Music);
    }

    void StartEvilHour()
    {
        cm.SpawnCallout(3, "EVIL HOUR has begun!\n<size=64>Sabotage your enemy...\nor be punished!", CalloutManager.evilHour);
        ReachedEvilHourKickoff = true;
        return;
    }
    
    void EndEvilHour()
    {
        if (!_playerAIsEvil)
        {
            cm.SpawnCallout(0, "-" + (playerA.GetTotalEyes() / 2) + " eyes", CalloutManager.negative);
            playerA.SetTotalEyes(playerA.GetTotalEyes() / 2);
            playerA.SetEvilPunished(true);
        }

        if (!_playerBIsEvil)
        {
            cm.SpawnCallout(1, "-" + (playerB.GetTotalEyes() / 2) + " eyes", CalloutManager.negative);
            playerB.SetTotalEyes(playerB.GetTotalEyes() / 2);
            playerB.SetEvilPunished(true);
            // append player B to evil hour announcement
        }

        string result = "";
        if (_playerAIsEvil && _playerBIsEvil) result = "No one was punished";
        if (!_playerAIsEvil && _playerBIsEvil) result = "Player 1 has been punished";
        if (_playerAIsEvil && !_playerBIsEvil) result = "Player 2 has been punished";
        if (!_playerAIsEvil && !_playerBIsEvil) result = "Both players have been punished";
        
        cm.SpawnCallout(3, "EVIL HOUR has ended!\n<size=64>"+result, CalloutManager.evilHour);
        
        ReachedEvilHourCloseoff = true;
    }

    void StartRushHour()
    {
        cm.SpawnCallout(3, "ONE MINUTE remains...\n<size=64>Pick up the pace!", CalloutManager.rushHour);
        ReachedRushHourKickoff = true;
    }

    void EndRushHour()
    {
        cm.SpawnCallout(3, "TEN SECONDS LEFT!", CalloutManager.rushHour);
        ReachedRushHourCloseoff = true;
    }

    void EndGame()
    {
        cm.SpawnCallout(3, "TIME'S UP!", Color.white);
        gameIsRunning = false;
    }
    
    void Update()
    {
        if (!gameIsRunning) return;

        if (gameRuntime < gameEndtime) gameRuntime += Time.deltaTime;
        else EndGame();

        if (gameRuntime >= evilHourKickoff && !ReachedEvilHourKickoff) StartEvilHour();
        if (gameRuntime >= evilHourCloseoff && !ReachedEvilHourCloseoff) EndEvilHour();
        if (gameRuntime >= rushHourKickoff && !ReachedRushHourKickoff) StartRushHour();
        if (gameRuntime >= rushHourCloseoff && !ReachedRushHourCloseoff) EndRushHour();
        
        // 4:00.50 , tempo 117 -> 181
        float period = 60f;
        float bgm_t = SoundManager.Instance.GetMusicTime();
        float tempo = (bgm_t < 240) ? 117f : 181f;
        
        setMetronome(0, tempo * _tempoModifierA);
        setMetronome(1, tempo * _tempoModifierB);
        
        // clock
        float clockAngle = (gameRuntime / 300f) * -360f;
        clockPivot.rotation = Quaternion.Euler(0f, 0f, clockAngle);
        
        // wheels
        if (_spinning)
        {
            if (_spinTime < 5)
            {
                _spinTime += Time.deltaTime;
                float spinSpeed = _spinVelConstant * (1 - Mathf.Exp(-_spinExpConstant * Mathf.Pow(5-_spinTime, 2)));
                sabotageWheel.rotation = Quaternion.Euler(0f, 0f, sabotageWheel.eulerAngles.z + spinSpeed * Time.deltaTime);
                paymentWheel.rotation = Quaternion.Euler(0f, 0f, paymentWheel.eulerAngles.z - spinSpeed * Time.deltaTime);
            }
            else
            {
                EndSpinEvent();
            }
        }
    }

    public float RegisterStirTick(int id)
    {
        float bgm_t = SoundManager.Instance.GetMusicTime();
        
        float refCrotchet = 0f;
        if (id == 0) refCrotchet = _bgmCrotchetA;
        else if (id == 1) refCrotchet = _bgmCrotchetB;
        
        bgm_t += (bgm_t < 240) ? 0.33f : 0.6f;
        float f = (bgm_t % refCrotchet) / refCrotchet; // rounding % of how close stir tick was to being on tempo
        // [0, 5) or (95, 100]: great +9 [10% space]
        // [5, 15) or (85, 95]: good +4.5 [20% space]
        // [15, 25) or (75, 85]: ok +1.5 [20% space]
        // [25, 75]: bad -3 [50% space]
        if ((f < 0.05) || (f > 0.95)) return 9f;
        else if ((f < 0.15) || (f > 0.85)) return 4.5f;
        else if ((f < 0.25) || (f > 0.75)) return 1.5f;
        else return -3f;
    }
    
    private void RandomizeIngredients(IngredientItem up, IngredientItem right, IngredientItem left, 
        IngredientItem.ItemType[] source, System.Random rng)
    {
        var items = (IngredientItem.ItemType[])source.Clone();

        for (int i = source.Length - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (source[i], source[j]) = (source[j], source[i]);
        }

        up.SetItem(items[0]);
        right.SetItem(items[1]);
        left.SetItem(items[2]);
    }

    public void StickySabotage(int id, float stickyGrabTime = 0.75f, float duration = 20f)
    {
        // cancel if sabotage is already active for this player
        if (id == 0 && stickyRoutineA != null)
            StopCoroutine(stickyRoutineA);
        else if (id == 1 && stickyRoutineB != null)
            StopCoroutine(stickyRoutineB);

        Coroutine newRoutine = StartCoroutine(StickySabotageCoroutine(id, stickyGrabTime, duration));

        if (id == 0) stickyRoutineA = newRoutine;
        else stickyRoutineB = newRoutine;
    }

    private IEnumerator StickySabotageCoroutine(int id, float stickyGrabTime, float duration)
    {
        // Get correct items
        IngredientItem up = (id == 0) ? upItemA : upItemB;
        IngredientItem right = (id == 0) ? rightItemA : rightItemB;
        IngredientItem left = (id == 0) ? leftItemA : leftItemB;

        // Activate first child (sticky image) on each ingredient
        up.transform.GetChild(0).gameObject.SetActive(true);
        right.transform.GetChild(0).gameObject.SetActive(true);
        left.transform.GetChild(0).gameObject.SetActive(true);

        // Set player grab timer
        PlayerController player = GetPlayerById(id);
        if (player != null)
            player.SetGrabTimerMax(stickyGrabTime);

        // Wait sabotage duration
        yield return new WaitForSeconds(duration);

        // Disable sticky images
        up.transform.GetChild(0).gameObject.SetActive(false);
        right.transform.GetChild(0).gameObject.SetActive(false);
        left.transform.GetChild(0).gameObject.SetActive(false);

        // Reset grab timer
        if (player != null)
            player.SetGrabTimerMax(0.1f);

        // clear reference
        if (id == 0) stickyRoutineA = null;
        else stickyRoutineB = null;
    }
    
    // Public caller
    public void RandomizeIngredientSabotage(int id)
    {
        // Prevent stacking sabotage
        if (id == 0 && randomSwapRoutineA != null)
            StopCoroutine(randomSwapRoutineA);
        else if (id == 1 && randomSwapRoutineB != null)
            StopCoroutine(randomSwapRoutineB);

        Coroutine newRoutine = StartCoroutine(RandomizeIngredientSabotageCoroutine(id));

        if (id == 0) randomSwapRoutineA = newRoutine;
        else randomSwapRoutineB = newRoutine;
    }
    
    private PlayerController GetPlayerById(int id)
    {
        return (id == 0) ? playerA : playerB;
    }
    
    // Coroutine logic
    private IEnumerator RandomizeIngredientSabotageCoroutine(int id)
    {
        IngredientItem up = (id == 0) ? upItemA : upItemB;
        IngredientItem right = (id == 0) ? rightItemA : rightItemB;
        IngredientItem left = (id == 0) ? leftItemA : leftItemB;

        // If player currently has an item, clear it
        PlayerController player = GetPlayerById(id);
        if (player != null && player.GetItem() != null)
        {
            player.ForceIdleState();
            player.EndPourEvent();
            player.SetItem(null);
        }

        // Rotate ingredient types (up → right → left → up)
        IngredientItem.ItemType upType = up.GetItemType();
        IngredientItem.ItemType rightType = right.GetItemType();
        IngredientItem.ItemType leftType = left.GetItemType();

        // Rotate the types
        right.SetItemType(upType);
        left.SetItemType(rightType);
        up.SetItemType(leftType);
        
        right.EnableImage();
        left.EnableImage();
        up.EnableImage();
        
        Material material = (id == 0) ? ingredientMaterialA : ingredientMaterialB;
        
        // Fade Out (0 → 1)
        float fadeDuration = 1f;
        yield return StartCoroutine(FadeMaterial(material, 0f, 1f, fadeDuration));

        // Update sprites after fade hides them
        up.SetItem(up.GetItemType());
        right.SetItem(right.GetItemType());
        left.SetItem(left.GetItemType());

        // Fade In (1 → 0)
        yield return StartCoroutine(FadeMaterial(material, 1f, 0f, fadeDuration));

        if (id == 0) randomSwapRoutineA = null;
        else randomSwapRoutineB = null;
    }

    // Fade utility for item swap sabotage
    private IEnumerator FadeMaterial(Material material, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(from, to, t);
            material.SetFloat("_Fade", value);
            yield return null;
        }
        material.SetFloat("_Fade", to);
    }
    
    public void SlowStirTempo(int id, float slowValue = 0.5f, float duration = 20f)
    {
        if (id == 0 && slowStirRoutineA != null)
            StopCoroutine(slowStirRoutineA);
        else if (id == 1 && slowStirRoutineB != null)
            StopCoroutine(slowStirRoutineB);

        Coroutine newRoutine = StartCoroutine(SlowStirTempoCoroutine(id, slowValue, duration));
        if (id == 0) slowStirRoutineA = newRoutine;
        else slowStirRoutineB = newRoutine;
    }

    private IEnumerator SlowStirTempoCoroutine(int id, float slowValue, float duration)
    {
        // Save original value (in case it's not always 1)
        float original = (id == 0) ? _tempoModifierA : _tempoModifierB;

        // Set to slow value
        if (id == 0) _tempoModifierA = slowValue;
        else _tempoModifierB = slowValue;

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Restore original
        if (id == 0)
        {
            _tempoModifierA = original;
            slowStirRoutineA = null;
        }
        else
        {
            _tempoModifierB = original;
            slowStirRoutineB = null;
        }
    }
    
    public void InkBlockScreen(int id, float duration = 15f) 
    { 
        // If already running, stop it first
        if (id == 0 && inkRoutineA != null)
            StopCoroutine(inkRoutineA);
        else if (id == 1 && inkRoutineB != null)
            StopCoroutine(inkRoutineB);

        Coroutine newRoutine = StartCoroutine(InkBlockCoroutine(id, duration));
        if (id == 0) inkRoutineA = newRoutine;
        else inkRoutineB = newRoutine;
    } 
    
    private IEnumerator InkBlockCoroutine(int id, float duration)
    {
        GameObject inkObj = (id == 0) ? inkSabotageA : inkSabotageB;
        Image img = inkObj.GetComponent<Image>();

        // make sure starting alpha is 0
        Color startColor = img.color;
        startColor.a = 0f;
        img.color = startColor;

        inkObj.SetActive(true);

        float fadeInTime = 1f;
        float fadeOutTime = 5f;

        // Fade In (0 → 1 alpha)
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            SetImageAlpha(img, alpha);
            yield return null;
        }
        SetImageAlpha(img, 1f);

        // Hold
        float holdTime = duration - fadeInTime - fadeOutTime;
        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        // Fade Out (1 → 0 alpha)
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            SetImageAlpha(img, alpha);
            yield return null;
        }
        SetImageAlpha(img, 0f);

        inkObj.SetActive(false);
        
        // clear reference after finish
        if (id == 0) inkRoutineA = null;
        else inkRoutineB = null;
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
    

    public void SpinWheel(int id, float spinCooldown)
    {
        if (_spinning)
        {
            cm.SpawnCallout(id, "Spin in use", CalloutManager.inactive);
            return;
        }

        if (spinCooldown > 0f)
        {
            cm.SpawnCallout(id, "Spin cooldown: " + Mathf.CeilToInt(spinCooldown) + "s", CalloutManager.inactive);
            return;
        }
        
        SoundManager.Instance.Play(SoundManager.SoundType.Spin);
        
        _spinnerID = id;
        
        if (_spinnerID == 0) cm.SpawnCallout(0, "Spinning . . .", CalloutManager.recipe);
        else if (_spinnerID == 1) cm.SpawnCallout(1, "Spinning . . .", CalloutManager.recipe);
        
        // player has successfully attempted sabotage during evil hour, safe from punishment
        if (ReachedEvilHourKickoff && !ReachedEvilHourCloseoff)
        {
            if (id == 0) _playerAIsEvil = true;
            else if (id == 1) _playerBIsEvil = true;
        }
        
        _spinning = true;
        _spinTime = 0f;

        _spinExpConstant = UnityEngine.Random.Range(0.15f, 0.3f);
        _spinVelConstant = UnityEngine.Random.Range(1750f, 2250f);

        if (id == 0) playerA.setSpinCooldown(playerA.GetTotalEyes());
        else if (id == 1) playerB.setSpinCooldown(playerB.GetTotalEyes());
    }

    public void EndSpinEvent()
    {
        _spinning = false;
        
        float sabotageAngle = sabotageWheel.eulerAngles.z;
        sabotageAngle = (sabotageAngle % 360f + 360f) % 360f;
        sabotageAngle = Mathf.Clamp(sabotageAngle, 0f, 360f);
        float paymentAngle = paymentWheel.eulerAngles.z;
        paymentAngle = (paymentAngle % 360f + 360f) % 360f;
        paymentAngle = Mathf.Clamp(paymentAngle, 0f, 360f);
        
        // payment range
        int eyeballCost = 30;
        if (paymentAngle >= 0 && paymentAngle < 45)
            eyeballCost = 18;
        else if (paymentAngle >= 45 && paymentAngle < 90)
            eyeballCost = 10;
        else if (paymentAngle >= 90 && paymentAngle < 135)
            eyeballCost = 8;
        else if (paymentAngle >= 135 && paymentAngle < 180)
            eyeballCost = 30;
        else if (paymentAngle >= 180 && paymentAngle < 225)
            eyeballCost = 4;
        else if (paymentAngle >= 225 && paymentAngle < 270)
            eyeballCost = 6;
        else if (paymentAngle >= 270 && paymentAngle < 315)
            eyeballCost = 20;
        else if (paymentAngle >= 315 && paymentAngle <= 360)
            eyeballCost = 15;
        Debug.Log($"Eyeball Cost: {eyeballCost}");

        int sabotagedID = (_spinnerID == 0) ? 1 : 0;
        if (_spinnerID == 0)
        {
            if (playerA.GetTotalEyes() >= eyeballCost) playerA.SetTotalEyes(playerA.GetTotalEyes() - eyeballCost); // remove cost if can afford
            else sabotagedID = 0; // self-sabotage if cant afford
        }
        else if (_spinnerID == 1)
        {
            if (playerB.GetTotalEyes() >= eyeballCost) playerB.SetTotalEyes(playerB.GetTotalEyes() - eyeballCost);
            else sabotagedID = 1; // self-sabotage if cant afford
        }

        Debug.Log($"Player Sabotaged: {sabotagedID}");
        
        // sabotage range
        if ((sabotageAngle >= 0 && sabotageAngle < 45) || (sabotageAngle >= 180 && sabotageAngle < 225))
        {
            SlowStirTempo(sabotagedID);
            cm.SpawnCallout(sabotagedID, "Tempo slowed", CalloutManager.negative);
            SoundManager.Instance.Play(SoundManager.SoundType.Sab_Slow);
        }
        else if ((sabotageAngle >= 45 && sabotageAngle < 90) || (sabotageAngle >= 225 && sabotageAngle < 270))
        {
            RandomizeIngredientSabotage(sabotagedID);    
            cm.SpawnCallout(sabotagedID, "Ingredients swapped", CalloutManager.negative);
            SoundManager.Instance.Play(SoundManager.SoundType.Sab_Swap);
        }
        else if ((sabotageAngle >= 90 && sabotageAngle < 135) || (sabotageAngle >= 270 && sabotageAngle < 315))
        {
            InkBlockScreen(sabotagedID);
            cm.SpawnCallout(sabotagedID, "Blinded by ink", CalloutManager.negative);
            SoundManager.Instance.Play(SoundManager.SoundType.Sab_Ink);
        }
        else if ((sabotageAngle >= 135 && sabotageAngle <= 180) || (sabotageAngle >= 315 && sabotageAngle <= 360))
        {
            StickySabotage(sabotagedID);
            cm.SpawnCallout(sabotagedID, "Ingredients stickied", CalloutManager.negative);
            SoundManager.Instance.Play(SoundManager.SoundType.Sab_Sticky);
        }

        if (sabotagedID != _spinnerID) cm.SpawnCallout(_spinnerID, "-" + eyeballCost + " eyes", CalloutManager.negative);
        
        // call sabotage on the sabotagedID
    }

    public void ShowMetronome(int id)
    {
        if (id == 0) metronomeA.SetActive(true);
        else if (id == 1) metronomeB.SetActive(true); 
    }

    public void SetStirMeter(int id, float value, float max)
    {
        if (id == 0)
        {
            stirSliderA.value = value;
            stirSliderA.maxValue = max;
        }
        else if (id == 1)
        {
            stirSliderB.value = value;
            stirSliderB.maxValue = max;
        }
    }

    public Recipe GenerateRecipe()
    {
        int index = UnityEngine.Random.Range(0, recipes.Length);
        return recipes[index];
    }

    public void HideMetronome(int id)
    {
        if (id == 0) metronomeA.SetActive(false);
        else if (id == 1) metronomeB.SetActive(false);
    }

    public void ShowPourMeter(int id, IngredientItem.ItemType itemType)
    {
        Color newColor = Color.white;
        switch (itemType)
        {
            case IngredientItem.ItemType.Heart:
                newColor = new Color(0.9607844f, 0.5647059f, 0.8862746f);
                break;
            case IngredientItem.ItemType.Frog:
                newColor = new Color(0.4431373f, 0.8156863f, 0.3686275f);
                break;
            case IngredientItem.ItemType.Raven:
                newColor = new Color(0.5450981f, 0.3176471f, 0.8588236f);
                break;
        }

        if (id == 0)
        {
            pourMeterA.SetActive(true);
            pourFillA.color = newColor;
        }
        else if (id == 1)
        {
            pourMeterB.SetActive(true);
            pourFillB.color = newColor;
        }
    }

    public void SetPourMeter(int id, float itemPortion)
    {
        if (id == 0) _pourSliderA.value = itemPortion;
        else if (id == 1) _pourSliderB.value = itemPortion;
    }
    
    public void HidePourMeter(int id)
    {
        if (id == 0) pourMeterA.SetActive(false);
        else if (id == 1) pourMeterB.SetActive(false);
    }

    public void ShowHarvestMeter(int id)
    {
        if (id == 0) harvestMeterA.SetActive(true);
        else if (id == 1) harvestMeterB.SetActive(true);
    }

    public void SetHarvestMeter(int id, float value)
    {
        if (id == 0) _harvestSliderA.value = value;
        else if (id == 1) _harvestSliderB.value = value;
    }
    
    public void HideHarvestMeter(int id)
    {
        if (id == 0) harvestMeterA.SetActive(false);
        else if (id == 1) harvestMeterB.SetActive(false);
    }

    public void AnimateEyeplot(int id, float eyeplotGrowth)
    {
        int idx = (int)Mathf.Round(Mathf.Clamp(eyeplotGrowth, 0f, 9f));
        int[] result = _eyePlotSpriteIndices[idx];
        if (id == 0)
        {
            eyeStem1A.sprite = eyePlotSprites.sprites[result[0]];
            eyeStem2A.sprite = eyePlotSprites.sprites[result[1]];
            eyeStem3A.sprite = eyePlotSprites.sprites[result[2]];
        }
        else if (id == 1)
        {
            eyeStem1B.sprite = eyePlotSprites.sprites[result[0]];
            eyeStem2B.sprite = eyePlotSprites.sprites[result[1]];
            eyeStem3B.sprite = eyePlotSprites.sprites[result[2]];       
        }
    }

    public void SetEyeCounter(int id, int value)
    {
        if (id == 0) eyeCounterA.text = "" + value;
        else if (id == 1) eyeCounterB.text = "" + value;
    }
    
    public void SetScoreCounter(int id, int value)
    {
        if (id == 0) scoreCounterA.text = "" + value;
        else if (id == 1) scoreCounterB.text = "" + value;
    }
}

/*
Holding changes as comment for now 

    //adding here for now, will move if causes issues
     pressedKey = GetComponent<Image>();
     

        //testing one key first 😞
        if (Input.GetKeyDown(KeyCode.W))
        {
            pressedKey.sprite = pressedW;
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            pressedKey.sprite = originalW;
        }

    //keycap(s)
    public Sprite originalW;
    public Sprite pressedW;
    private Image pressedKey;

*/