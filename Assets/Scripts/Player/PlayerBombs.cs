using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerBombs : MonoBehaviour
{
    public Sprite circleSprite;
    [SerializeField, Min(1)] private int chestBombs = 5;
    public bool IsUnlocked { get; private set; }
    public int BombCount { get; private set; }
    public event System.Action<int> CountChanged;
    private float nextPlacement;
    private PlayerMovement movement;
    private PlayerMelee melee;
    private void Awake() { movement = GetComponent<PlayerMovement>(); melee = GetComponent<PlayerMelee>(); }
    public void Unlock()
    {
        if (IsUnlocked) return;
        IsUnlocked = true;
        BombCount = chestBombs;
        gameObject.AddComponent<BombHUD>().Initialize(this);
        CountChanged?.Invoke(BombCount);
    }
    public bool AddBombs(int amount)
    {
        if (!IsUnlocked || amount <= 0) return false;
        BombCount += amount;
        CountChanged?.Invoke(BombCount);
        return true;
    }
    private void Update()
    {
        if (!IsUnlocked || BombCount <= 0 || !movement.enabled || Time.timeScale <= 0 ||
            FirstRoomEncounter.BlocksGameplayInput || Keyboard.current == null) return;
        if (Keyboard.current.qKey.wasPressedThisFrame && Time.time >= nextPlacement && circleSprite != null)
        {
            nextPlacement = Time.time + 0.4f;
            var bomb = new GameObject("Bomb");
            bomb.transform.position = transform.position;
            bomb.AddComponent<PlacedBomb>().Initialize(circleSprite, melee != null ? melee.AttackRadius : 1.9f);
            BombCount--;
            CountChanged?.Invoke(BombCount);
        }
    }
}
