using UnityEngine;
using UnityEngine.UI;

public class CollectibleHUD : MonoBehaviour
{
    [SerializeField] private Text label;
    private int count;

    public void SetLabel(Text value)
    {
        label = value;
        Refresh();
    }

    private void Awake() => Refresh();

    public void AddCollectible()
    {
        count++;
        Refresh();
    }

    private void Refresh()
    {
        if (label != null) label.text = "Collectible: " + count;
    }
}
