using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UnityEngine.UI.Image inkSabotage; //check this

    public static float GrabTimerMax = 0.15f;
    public static float MixFrameMax = 0.1f;
    
    public Sprite heartSprite;
    public Sprite frogSprite;
    public Sprite ravenSprite;
    public Sprite emptySprite;
    
    public enum Items
    {
        Heart,
        Frog,
        Raven
    }
    
    

    public PlayerController playerA;
    public PlayerController playerB;

    public IngredientItem upItemA;
    public IngredientItem rightItemA;
    public IngredientItem leftItemA;

    public IngredientItem upItemB;
    public IngredientItem rightItemB;
    public IngredientItem leftItemB;
    
    void Start()
    {
        // initializing stuffs
        Instance = this;
        playerA.SetID(0);
        playerB.SetID(1);
        
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
        return;
    }
}
