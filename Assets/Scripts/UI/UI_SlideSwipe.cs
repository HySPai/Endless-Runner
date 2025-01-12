using GG.Infrastructure.Utils.Swipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SlideSwipe : MonoBehaviour
{
    [SerializeField] private SwipeListener swipeListener;
    private Player player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void OnEnable()
    {
        swipeListener.OnSwipe.AddListener(OnSwipe);
    }

    private void OnSwipe(string swipe)
    {
        player.SlideButton();
    }

    private void OnDisable()
    {
        swipeListener.OnSwipe.RemoveListener(OnSwipe);
    }
}
