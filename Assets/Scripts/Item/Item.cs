using System.Collections;
using UnityEngine;

public enum ItemType
{
    WingedShoe,
    Shield,
    Magnet
}

public class Item : MonoBehaviour
{
    [SerializeField] private ItemType itemType;
    [SerializeField] private float effectDuration;

    private Player player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            AudioManager.instance.PlaySFX(0);
            Destroy(gameObject);
            ActivateItem();
        }
    }

    private void ActivateItem()
    {
        switch (itemType)
        {
            case ItemType.WingedShoe:
                StartCoroutine(WingedShoeEffect());
                break;
            case ItemType.Shield:
                StartCoroutine(ShieldEffect());
                break;
            case ItemType.Magnet:
                StartCoroutine(MagnetEffect());
                break;
        }
    }

    private IEnumerator WingedShoeEffect()
    {
        player.isWingedshoes = true;
        yield return new WaitForSeconds(effectDuration);
        player.isWingedshoes = false;
    }

    private IEnumerator ShieldEffect()
    {
        player.isShield = true;
        yield return new WaitForSeconds(effectDuration);
        player.isShield = false;
    }

    private IEnumerator MagnetEffect()
    {
        player.attractionRadius += 5f;
        player.speedToAttract += 15f;
        yield return new WaitForSeconds(effectDuration);
        player.attractionRadius -= 5f;
        player.speedToAttract -= 15f;
    }
}
