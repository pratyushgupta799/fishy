using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CircleTransition : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float duration = 0.6f;

    private Material mat;
    
    public static CircleTransition Instance;
    
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        mat = Instantiate(image.material);
        image.material = mat;
    }

    Vector2 ToUV(Vector2 screenPos)
    {
        return new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );
    }

    public async Task CircleIn(Vector2 screenPos)
    {
        image.enabled = true;
        mat.SetVector("_Center", ToUV(screenPos));
        mat.SetFloat("_Radius", 1.2f);
        await mat.DOFloat(0f, "_Radius", duration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
    }
    
    public async Task CircleOut(Vector2 screenPos)
    {
        mat.SetVector("_Center", ToUV(screenPos));
        mat.SetFloat("_Radius", 0f);
        await mat.DOFloat(1.2f, "_Radius", duration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
        image.enabled = false;
    }
}
