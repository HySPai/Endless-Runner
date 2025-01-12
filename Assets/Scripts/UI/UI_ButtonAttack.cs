using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ButtonAttack : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData) => GameManager.instance.player.PlayerAttack();
}
