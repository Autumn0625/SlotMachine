using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetLetterSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
