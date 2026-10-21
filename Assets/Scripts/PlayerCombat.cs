using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private static PlayerCombat _instance;
    public static PlayerCombat Instance
    {
        get
        {
            if (_instance == null) _instance = Object.FindFirstObjectByType<PlayerCombat>();
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Weapon Config")]
    public bool hasWeapon = false;
    public Sprite weaponSprite;
    public GameObject weaponVisual;

    private Unit unit;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(this);
        unit = GetComponent<Unit>();
        SetupWeaponVisual();
    }

    public void SetupWeaponVisual()
    {
        if (weaponVisual == null)
        {
            Transform existing = transform.Find("WeaponVisual");
            if (existing != null) weaponVisual = existing.gameObject;
        }

        if (weaponVisual == null && weaponSprite != null)
        {
            weaponVisual = new GameObject("WeaponVisual");
            weaponVisual.transform.SetParent(transform, false);
            weaponVisual.transform.localPosition = new Vector3(0.35f, 0f, 0f);
            weaponVisual.transform.localScale = Vector3.one * 0.75f;

            SpriteRenderer sr = weaponVisual.AddComponent<SpriteRenderer>();
            sr.sprite = weaponSprite;
            sr.sortingOrder = 15;
        }

        if (weaponVisual != null)
        {
            weaponVisual.SetActive(hasWeapon);
        }
    }

    public void ResetCombat()
    {
        hasWeapon = false;
        if (weaponVisual != null) weaponVisual.SetActive(false);
    }

    public void EquipWeapon()
    {
        hasWeapon = true;
        if (weaponVisual != null)
        {
            weaponVisual.SetActive(true);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponPickup();
        }
    }

    public bool PerformAttack()
    {
        if (!hasWeapon)
        {
            Debug.Log("<color=yellow>You cannot attack without a weapon! Collect the sword first with 'E'.</color>");
            return false;
        }

        if (unit == null) unit = GetComponent<Unit>();
        if (unit == null) return false;

        Vector2Int pPos = unit.GridPosition;
        Vector2Int[] dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        bool hitAny = false;
        CombatEnemy[] enemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e != null && !e.IsDead)
            {
                foreach (var dir in dirs)
                {
                    if (e.GetGridPosition() == pPos + dir)
                    {
                        e.Die();
                        hitAny = true;
                        break;
                    }
                }
            }
        }

        if (hitAny)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackHit();
            }
            StartCoroutine(WeaponSwingVisual());
        }
        else
        {
            Debug.Log("Swung weapon, but no enemy in adjacent range.");
        }

        return hitAny;
    }

    private IEnumerator WeaponSwingVisual()
    {
        if (weaponVisual != null)
        {
            weaponVisual.transform.localRotation = Quaternion.Euler(0, 0, -45f);
            yield return new WaitForSeconds(0.12f);
            weaponVisual.transform.localRotation = Quaternion.identity;
        }
    }
}
