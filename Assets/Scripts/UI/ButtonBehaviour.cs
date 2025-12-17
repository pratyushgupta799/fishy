using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        rect.DOAnchorPosY(startPos.y + 12f, 0.2f).SetEase(Ease.OutBack);
        rect.DOScale(startScale * 1.05f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.DOAnchorPosY(startPos.y, 0.2f).SetEase(Ease.OutBack);
        rect.DOScale(startScale, 0.2f).SetEase(Ease.OutBack);
    }
}
