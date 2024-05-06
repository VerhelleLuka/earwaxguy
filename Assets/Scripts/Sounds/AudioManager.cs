using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] Sounds;
    public static AudioManager Instance { get; private set; }
    private AudioSource _Music = null;

    [SerializeField] float _FadeSpeed = 0.00001f;
    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        //set sounds to defined settings in inspector
        //foreach (Sound s in Sounds)
        //{
        //    DefineSound(s,Target);
        //}
    }

    public void PlayClip(string name, Vector3 Location, bool loop)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
            return;
        //s.Source.Play();
        if (loop)
            s.Loop = true;
        PlayClipAt(s, Location);
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
            return;

        GameObject tempGameObj = new GameObject("TempAudio");

        if (_Music != null)
            _Music.Stop();

        _Music = DefineSound(s, tempGameObj);
        _Music.Play();
        _Music.loop = true;
    }
    public IEnumerator FadeOutMusic(string name, float fadeSpeed)
    {
        _FadeSpeed = fadeSpeed;
        if (_Music == null)
            yield return null;
        for (; _Music.volume > 0; )
        {
            _Music.volume -= _FadeSpeed;
            yield return null;
        }

        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
            yield return null;
        GameObject tempGameObj = new GameObject("TempAudio");
        _Music.Stop();
        _Music = DefineSound(s, tempGameObj);
        _Music.Play();
        _Music.loop = true;
        yield return null;

    }
    public void FadeMusic(string name, float fadeSpeed)
    {
        StartCoroutine(FadeOutMusic(name, fadeSpeed));
    }
    //public void PlaySound(string name, Vector3 Location)
    //{
    //    Sound s = Array.Find(Sounds, sound => sound.Name == name);
    //    if (s == null)
    //        return;
    //    //s.Source.Play();
    //    AudioSource.PlayClipAtPoint(s.Clip, Location);
    //}

    public void Stop(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
            return;
        s.Source.Stop();
    }

    public void StopMusic(string name)
    {
        _Music.Stop();
    }

    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
            return false;
        if (s.Source.isPlaying)
            return true;
        return false;
    }

    AudioSource PlayClipAt(Sound s, Vector3 pos)
    {
        GameObject tempGameObj = new GameObject("TempAudio");
        tempGameObj.transform.position = pos;
        AudioSource source = DefineSound(s, tempGameObj);
        source.Play();
        Destroy(tempGameObj, s.Clip.length);
        return source;
    }

    private static AudioSource DefineSound(Sound s, GameObject Target)
    {
        s.Source = Target.AddComponent<AudioSource>();
        s.Source.clip = s.Clip;
        s.Source.volume = s.Volume;
        s.Source.pitch = s.Pitch;
        s.Source.loop = s.Loop;
        s.Source.spatialBlend = s.SpatialBlend;
        s.Source.maxDistance = s.MaxDistance;
        s.Source.minDistance = 0;
        s.Source.rolloffMode = AudioRolloffMode.Linear;
        return s.Source;
    }
}