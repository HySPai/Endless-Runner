using UnityEngine;

public enum CoinType
{
    Silver,
    Gold,
    BlueDiamond,
    GreenDiamond,
    RedDiamond
}

public class Coin : MonoBehaviour
{
    [SerializeField] private CoinType coinType;

    private Player player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, player.transform.position) > 50)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            AudioManager.instance.PlaySFX(0);
            AddScore();
            Destroy(gameObject);
        }
        else if (collision.GetComponent<Enemy>() != null && !player.isAttract)
        {
            Destroy(gameObject);
        }
    }

    private void AddScore()
    {
        int coinValue = 0;

        switch (coinType)
        {
            case CoinType.Silver:
                coinValue = 1;
                break;
            case CoinType.Gold:
                coinValue = 2;
                break;
            case CoinType.BlueDiamond:
                coinValue = 3;
                break;
            case CoinType.GreenDiamond:
                coinValue = 4;
                break;
            case CoinType.RedDiamond:
                coinValue = 5;
                break;
        }

        GameManager.instance.AddCoins(coinValue);
    }
}
