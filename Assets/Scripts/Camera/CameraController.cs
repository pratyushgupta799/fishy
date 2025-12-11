using Unity.Cinemachine;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private CinemachineBrain cinemachineBrain;
    [SerializeField] private GameObject vCam;
    [SerializeField] private Transform fishy;


    [SerializeField] private float moveTime = 0.5f;
    private bool isMoving;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float t;

    private bool revertingBack;
  
    void Start()
    {
        cinemachineBrain = GetComponent<CinemachineBrain>();
        FishyEvents.OnCamSnapZoneEntered += SnapTo;
        FishyEvents.OnCamSnapZoneExit += RevertBack;
    }


    void Update()
    {
        if (isMoving)
        {
            t += Time.deltaTime / moveTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.LookAt(fishy);

            if (t >= 1f)
            {
                isMoving = false;
                if (revertingBack)
                {
                    // now restore the vCam
                    vCam.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value =
                        transform.rotation.eulerAngles.y;
                    revertingBack = false;
                }
            }
        }
    }

    public void SnapTo(Vector3 position)
    {
        startPos = transform.position;
        targetPos = position;
        t = 0f;
        isMoving = true;
      
        transform.position = position;
        transform.LookAt(fishy);
    }

    public void RevertBack()
    {
        startPos = transform.position;
        targetPos = vCam.transform.position;
        t = 0f;
        isMoving = true;
        
        revertingBack = true;
    }
}