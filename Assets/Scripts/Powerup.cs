using UnityEngine;
[System.Serializable]
public class Powerup
{
    public Vector3 position;
    public Vector3 size = Vector3.one;
    public string type;
    public bool active = true;

    public Powerup(string t, Vector3 pos)
    {
        type = t;
        position = pos;
    }
}
