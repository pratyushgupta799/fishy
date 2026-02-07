using System;
using System.Collections;
using FishyUtilities;
using UnityEngine;

public class Level1Hint : MonoBehaviour
{
    [SerializeField] private HintText upText;
    [SerializeField] private HintText moveCamText;
    [SerializeField] private HintText moveText;
    [SerializeField] private HintText jumpText;
    [SerializeField] private HintText downText;
    [SerializeField] private HintText stallText;
    [SerializeField] private HintText reloadText;
    [SerializeField] private HintText spillText;
    [SerializeField] private HintText dashText;
    
    private int hintPhase = 0;
    
    public static Level1Hint Instance;
    private FishControllerRB fishy;

    // phase 1
    private bool moved;
    private bool camMoved;
    
    // phase 2
    private bool duckieCollected;

    private void Awake()
    {
        Instance = this;
        fishy = GameObject.FindGameObjectWithTag("Player").GetComponent<FishControllerRB>();
    }

    public void SetHintPhase(int hint)
    {
        Debug.Log(hintPhase);
        hintPhase = hint;
    }

    void Update()
    {
        if (fishy.IsMoving() && !moved)
        {
            StartCoroutine(Delay(1, () => { moved = true; }));
        }

        if (!camMoved && (InputManager.Instance.Look.ReadValue<Vector2>() != Vector2.zero))
        {
            StartCoroutine(Delay(1, () => { camMoved = true; }));
        }
        
        HideAll();
        ShowText();
    }

    private void ShowText()
    {
        if (hintPhase == 1)
        {
            if (!camMoved)
            {
                moveCamText.Show();
            }
            else if (!moved)
            {
                moveText.Show();
            }
            else if (fishy.GetFishyState() == FishyStates.InWater)
            {
                upText.Show();
            }
            else if (fishy.GetFishyState() == FishyStates.OnSurface)
            {
                jumpText.Show();
            }
        }

        if (hintPhase == 2)
        {
            if (!duckieCollected)
            {
                downText.Show();
            }
        }
        
        if (hintPhase == 3)
        {
            if ((fishy.GetFishyJumpState() == FishyJumpState.JumpingFromWater) && fishy.CanTwirl())
            {
                stallText.Show();
            }
        }

        if (hintPhase == 4)
        {
            if (fishy.GetFishyState() == FishyStates.OnSurface)
            {
                spillText.Show();
            }
        }

        if (hintPhase == 5)
        {
            if (fishy.GetFishyState() == FishyStates.InWater)
            {
                dashText.Show();
            }
        }
    }
    
    private void HideAll()
    {
        upText.Hide();
        moveCamText.Hide();
        moveText.Hide();
        jumpText.Hide();
        downText.Hide();
        stallText.Hide();
        reloadText.Hide();
        spillText.Hide();
        dashText.Hide();
    }

    public void DuckieCollected()
    {
        duckieCollected = true;
    }
    
    IEnumerator Delay(float t, Action a)
    {
        yield return new WaitForSeconds(t);
        a();
    }
}
