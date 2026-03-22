using UnityEngine;

[System.Serializable]
public class Enemy
{
    public int id;
    public Vector3 position;
    public Vector3 size;

    public float speed;
    public float leftLimit;
    public float rightLimit;

    public int matrixIndex; // index in matrices list

    public Vector3 velocity;
    public bool isGrounded;

    public Enemy(int id, Vector3 pos, Vector3 size, float range)
    {
        this.id = id;
        this.position = pos;
        this.size = size;

        speed = 2f;

        // Set movement bounds (small area)
        leftLimit = pos.x - range;
        rightLimit = pos.x + range;
    }

    public void UpdateMovement(float deltaTime)
    {
        position.x += speed * deltaTime;

        // Reverse direction when reaching limits
        if (position.x > rightLimit)
        {
            position.x = rightLimit;
            speed *= -1;
        }
        else if (position.x < leftLimit)
        {
            position.x = leftLimit;
            speed *= -1;
        }
    }
}
