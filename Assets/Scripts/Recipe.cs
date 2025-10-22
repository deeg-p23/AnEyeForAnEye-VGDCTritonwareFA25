using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    [Range(0.0f, 5.0f)] public float heartPortion;
    [Range(0.0f, 5.0f)] public float ravenPortion;
    [Range(0.0f, 5.0f)] public float frogPortion;
    [Range(0.0f, 100.0f)] public float stirAmount;
}
