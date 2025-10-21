using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSprite", menuName = "ScriptableObjects/CharacterSprite")]
public class CharacterSprite : ScriptableObject
{
    [SerializeField]
    private Texture spriteTexture;
    
    public Sprite[] sprites;
    
    void InitializeSprites()
    {
        if (!spriteTexture) return;
        sprites = Resources.LoadAll<Sprite>("Sprites/"+spriteTexture.name);
    }
}
