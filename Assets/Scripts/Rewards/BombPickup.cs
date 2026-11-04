using UnityEngine;

public class BombPickup : MonoBehaviour
{
    private bool collected;
    private float collectAfter;
    public static void Spawn(Vector3 position, Sprite circle)
    {
        if (circle == null) return;
        var obj = new GameObject("Bomb Pickup");
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * 0.6f;
        var visual = obj.AddComponent<SpriteRenderer>();
        visual.sprite = circle;
        visual.color = Color.black;
        visual.sortingOrder = 25;
        var trigger = obj.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.65f;
        obj.AddComponent<BombPickup>().collectAfter = Time.time + 0.25f;
        var textObject = new GameObject("Bomb Pickup Label");
        textObject.transform.SetParent(obj.transform, false);
        textObject.transform.localPosition = new Vector3(0, 1, 0);
        var text = textObject.AddComponent<TextMesh>();
        text.text = "+1 Bomb";
        text.fontSize = 48;
        text.characterSize = 0.12f;
        text.anchor = TextAnchor.LowerCenter;
        text.color = Color.white;
        textObject.GetComponent<MeshRenderer>().sortingOrder = 30;
    }
    private void OnTriggerEnter2D(Collider2D other) => TryCollect(other);
    private void OnTriggerStay2D(Collider2D other) => TryCollect(other);
    private void TryCollect(Collider2D other)
    {
        if (collected || Time.time < collectAfter || Time.timeScale <= 0) return;
        var inventory = other.GetComponentInParent<PlayerBombs>();
        if (inventory == null || !inventory.AddBombs(1)) return;
        collected = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
