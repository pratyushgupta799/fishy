using Unity.Cinemachine;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineOrbitalFollow vCam;
    [SerializeField] private Transform fishy;
  
    void Start()
    {
        FishyEvents.OnCamSnapZoneExit += RevertBack;
    }

    public void RevertBack()
    {
        vCam.HorizontalAxis.Value = fishy.rotation.eulerAngles.y;
    }
}