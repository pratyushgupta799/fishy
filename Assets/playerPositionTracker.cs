using UnityEngine;

public class trackpos : MonoBehaviour
{
    private GameObject tracker;

    private Material mat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        tracker = GameObject.Find("FishCharacterRb");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 trackerPos = tracker.GetComponent<Transform>().position;

        mat.SetVector("_PlayerPosition", trackerPos);
    }
}