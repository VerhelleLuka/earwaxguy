using UnityEngine;

[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;
    //public GameObject Target;

    [Range(0f, 1f)]
    public float Volume;

    [Range(0.1f, 3f)]
    public float Pitch;

    public bool Loop;

    [Range(0f, 1f)]
    public float SpatialBlend;

    public float MaxDistance;

    [HideInInspector]
    public AudioSource Source;

    public Sound()
    {
        this.Volume = 1;
        this.Pitch = 1;
        this.Loop = false;
        this.SpatialBlend = 0;
        this.MaxDistance = 500;
    }
}