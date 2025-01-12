using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

public class EnemyDie : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Hit Damage info")]
    [SerializeField] private LayerMask layerToTakeDamage;
    [SerializeField] private float timeToDie;
    [HideInInspector] public bool canDie = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & layerToTakeDamage) != 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        canDie = true;

        yield return timeToDie;

        StartCoroutine(Invincibility());
    }

    private IEnumerator Invincibility()
    {
        Color originalColor = sr.color;
        Color darkenColor = new Color(sr.color.r, sr.color.g, sr.color.b, .5f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.1f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.1f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.15f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.15f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.25f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.25f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.3f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.35f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.4f);

        sr.color = originalColor;
        Destroy(this.gameObject);
    }
}
