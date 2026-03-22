using UnityEngine;

[System.Serializable]
public class Fireball
{
    public Vector3 position;
    public Vector3 size;
    public float speed;
    public int matrixIndex;

    public Fireball(Vector3 pos, Vector3 size, float speed, int matrixIndex)
    {
        this.position = pos;
        this.size = size;
        this.speed = speed;
        this.matrixIndex = matrixIndex;
    }
}
