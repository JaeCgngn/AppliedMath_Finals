using UnityEngine;

[DisallowMultipleComponent]
public class CustomCollider : MonoBehaviour
{
    public Vector3 size = Vector3.one;
    public bool isDynamic = true;

    [HideInInspector]
    public int colliderID = -1;

    void Start()
    {
        // Register with your CollisionManager
        colliderID = CollisionManager.Instance.RegisterCollider(transform.position, size, isDynamic);
    }

    void Update()
    {
        // Keep the collider in sync with GameObject transform
        if (colliderID != -1)
        {
            CollisionManager.Instance.UpdateCollider(colliderID, transform.position, size);
            CollisionManager.Instance.UpdateMatrix(colliderID, Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale));
        }
    }

    public bool CheckCollision(Vector3 newPosition)
    {
        if (colliderID == -1) return false;
        return CollisionManager.Instance.CheckCollision(colliderID, newPosition, out _);
    }


    // void OnDestroy()
    // {
    //     if (colliderID != -1)
    //     {
    //         CollisionManager.Instance.RemoveCollider(colliderID);
    //     }
    // }








}