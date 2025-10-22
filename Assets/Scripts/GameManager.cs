using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UnityEngine.UI.Image inkSabotage; //check this

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
    
    // RECIPE OBJECTS
    public Recipe tellTaleTonic;
    
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
    
    // SABOTAGE SPINNING VARS
    private float _spinTime; // spinning lasts 4 seconds
    private int _spinnerID; // 0 if player A spun, 1 if player B spun
    private bool _spinning; // if true, wheels cannot be re-spun

    public RectTransform sabotageWheel;
    public RectTransform paymentWheel;
    
    void Start()
    {
        // initializing stuffs
        Instance = this;
        playerA.SetID(0);
        playerB.SetID(1);
        
        SoundManager.Instance.Play(SoundManager.SoundType.BG_Music);
        
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
        
        // poorly randomizing item placements but wtv right
        System.Random rng = new System.Random();
        
        IngredientItem.ItemType[] itemsToStore = new IngredientItem.ItemType[] 
            { IngredientItem.ItemType.Heart, IngredientItem.ItemType.Frog, IngredientItem.ItemType.Raven };

        int initialSize = itemsToStore.Length;
        for (int i = 0; i < initialSize; i++)
        {
            int j = rng.Next(0, itemsToStore.Length);
        }

        RandomizeIngredients(upItemA, rightItemA, leftItemA, itemsToStore, rng);
        RandomizeIngredients(upItemB, rightItemB, leftItemB, itemsToStore, rng);
    }

    void Update()
    {
        // 4:00.50 , tempo 117 -> 181
        float period = 60f;
        float bgm_t = SoundManager.Instance.GetMusicTime();
        float tempo = (bgm_t < 240.5) ? 117f : 181f;
        _bgmCrotchet = period / tempo;
        bgm_t += _bgmCrotchet / 2f;
        
        float n = Mathf.Floor(bgm_t / _bgmCrotchet);
        float f = (bgm_t - n * _bgmCrotchet) / _bgmCrotchet;
        float offset = 100f * (1f - Mathf.Pow(-1f, n)) / 2f;
        float angle = Mathf.Pow(-1f, n) * 100f * f + offset - 50f;
        metronomePivotA.rotation = Quaternion.Euler(0f, 0f,angle);
        metronomePivotB.rotation = Quaternion.Euler(0f, 0f,angle);
        
        // clock
        float clockAngle = (Time.time / 300f) * -360f;
        clockPivot.rotation = Quaternion.Euler(0f, 0f, clockAngle);
        
        // wheels
        if (_spinning)
        {
            if (_spinTime < 5)
            {
                _spinTime += Time.deltaTime;
                float k = 0.25f;
                float spinSpeed = 2000 * (1 - Mathf.Exp(-k * Mathf.Pow(5-_spinTime, 2)));
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
        bgm_t += _bgmCrotchet / 2f;
        float f = (bgm_t % _bgmCrotchet) / _bgmCrotchet; // rounding % of how close stir tick was to being on tempo
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

    public void InkBlockScreen(float duration = 10f) 
    { 
        StartCoroutine(InkBlockCoroutine(duration)); 
    } 
    
    private IEnumerator InkBlockCoroutine(float duration) 
    { 
        if(inkSabotage == null) 
        {
            Debug.LogWarning("InkSabotage isn't assigned in GameManager");
            yield break;
        } 

        //ink screen
        inkSabotage.enabled = true;
        yield return new WaitForSeconds(duration);

        //hide ink
        inkSabotage.enabled = false;
    }
    

    public void SpinWheel(int id)
    {
        if (_spinning) return;
        
        _spinnerID = id;
        _spinning = true;
        _spinTime = 0f;
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
        
        // sabotage range
        if ((sabotageAngle >= 0 && sabotageAngle < 45) || (sabotageAngle >= 180 && sabotageAngle < 225))
            Debug.Log("time sabotage");
        else if ((sabotageAngle >= 45 && sabotageAngle < 90) || (sabotageAngle >= 225 && sabotageAngle < 270))
            Debug.Log("swap sabotage");
        else if ((sabotageAngle >= 90 && sabotageAngle < 135) || (sabotageAngle >= 270 && sabotageAngle < 315))
            Debug.Log("ink sabotage");
        else if ((sabotageAngle >= 135 && sabotageAngle <= 180) || (sabotageAngle >= 315 && sabotageAngle <= 360))
            Debug.Log("mash sabotage");

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
        return tellTaleTonic;
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
