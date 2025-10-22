using UnityEngine;
using UnityEngine.UI;

public class IngredientItem : MonoBehaviour
{
    public enum ItemType
    {
        Heart,
        Frog,
        Raven,
        None
    }
    
    private Image _image;
    private ItemType _itemType;

    void OnValidate()
    {
        _image = GetComponent<Image>();
    }
    
    public void SetItem(ItemType itemType)
    {
        _itemType = itemType;

        switch (itemType)
        {
            case ItemType.Heart:
                SetSprite(GameManager.Instance.heartSprite);
                break;
            case ItemType.Frog:
                SetSprite(GameManager.Instance.frogSprite);
                break;
            case ItemType.Raven:
                SetSprite(GameManager.Instance.ravenSprite);
                break;
        }
    }

    public void SetItemType(ItemType itemType)
    {
        _itemType = itemType;
    }

    public void SetSprite(Sprite sprite) { _image.sprite = sprite; }
    
    public bool GetSprite() { return _image.sprite; }
    
    public void DisableImage() { _image.enabled = false; }
    public void EnableImage() { _image.enabled = true; }
    
    public ItemType GetItemType() { return _itemType; }
}
