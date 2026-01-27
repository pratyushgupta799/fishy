using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private RectTransform rect;
    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        startScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Inflate();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Deflate();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        Inflate();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Deflate();
    }

    private void Inflate()
    {
        rect.DOAnchorPosY(startPos.y + 12f, 0.2f).SetEase(Ease.OutBack);
        rect.DOScale(startScale * 1.05f, 0.2f).SetEase(Ease.OutBack);
    }

    private void Deflate()
    {
        rect.DOAnchorPosY(startPos.y, 0.2f).SetEase(Ease.OutBack);
        rect.DOScale(startScale, 0.2f).SetEase(Ease.OutBack);
    }
}
