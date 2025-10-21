using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    public RectTransform clockPivot;
    
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
        
        _pourSliderA = pourMeterA.GetComponent<Slider>();
        _pourSliderB = pourMeterB.GetComponent<Slider>();
        
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

    public void SpinWheel(int id)
    {
        return;
    }

    public void ShowMetronome(int id)
    {
        if (id == 0) metronomeA.SetActive(true);
        else if (id == 1) metronomeB.SetActive(true); 
    }

    public void SetStirMeter(int id, float value)
    {
        if (id == 0) stirSliderA.value = value;
        else if (id == 1) stirSliderB.value = value;
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
}
