using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellbookTypePageController : MonoBehaviour
{
    [SerializeField] private Image typeIcon;
    [SerializeField] private Image iconBackground;
    [SerializeField] private TMP_Text typeText;

    public void Wake(int pageNum)
    {
        switch(pageNum)
        {
            case 0:
                typeText.text = "Shapes";
                iconBackground.color = new Color32(255, 200, 0, 255); //yellow
                typeIcon.sprite = Resources.Load<Sprite>("TypeIcons/ShapeTypeIcon");
                break;
            case 1:
                typeText.text = "Effects";
                iconBackground.color = new Color32(0, 200, 255, 255); //cyan
                typeIcon.sprite = Resources.Load<Sprite>("TypeIcons/ShapeTypeIcon");
                break;
            case 2:
                typeText.text = "Elements";
                iconBackground.color = new Color32(255, 100, 255, 255); //magenta
                typeIcon.sprite = Resources.Load<Sprite>("TypeIcons/ShapeTypeIcon");
                break;
            default:
                Debug.Log("SpellbookTypePageController: unknown pageNum " + pageNum);
                break;
        }
    }
}
