using UnityEngine;

public class VisibilityScaling : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (transform.position - cam.position).normalized;
        float dot = Vector3.Dot(cam.forward, dir);

        if (dot < 0.3f)
            transform.localScale = Vector3.zero;
        else
            transform.localScale = Vector3.one;
    }
}
