using UnityEngine;
[System.Serializable]

public class Fireball
{
    public int id;
    public Vector3 direction;
    public float timer;

    public Fireball(int id, Vector3 direction, float lifetime)
    {
        this.id = id;
        this.direction = direction;
        this.timer = lifetime;
    }
}