using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSound[] AudioSounds;

    private void Awake()
    {
        foreach (AudioSound s in AudioSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.GetSoundClip();
            s.source.volume = s.GetSoundVolume();
            s.source.pitch = s.GetSoundPitch();
            s.source.loop = s.IsSoundLooping();
            s.source.playOnAwake = false;
        }
    }

    private void Start() { DontDestroyOnLoad(gameObject); }

    private void FixedUpdate()
    {
        foreach (AudioSound s in AudioSounds)
        {
            s.source.volume = s.GetSoundVolume();
            s.source.pitch = s.GetSoundPitch();
            s.source.loop = s.IsSoundLooping();
        }
    }

    public void Play(string name)
    {
        AudioSound s = Array.Find(AudioSounds, AudioSound => AudioSound.GetSoundName() == name);

        if (s == null)
        {
            Debug.LogWarning("AudioSound: " + name + " not found");
            return;
        }

        //Debug.Log("playing AudioSound: " + name);

        s.SetSoundPlaying(true);
        s.source.Play();
    }

    public void PlayWithPitch(string name, float pitch)
    {
        AudioSound s = Array.Find(AudioSounds, AudioSound => AudioSound.GetSoundName() == name);

        if (s == null)
        {
            Debug.LogWarning("AudioSound: " + name + " not found");
            return;
        }

        //if (PlayerData.GetDevMode()) { Debug.Log("playing AudioSound : " + name + " with pitch: " + pitch); }

        s.SetSoundPitch(Mathf.Clamp(pitch, 0.1f, 3f));
        s.source.Play();
    }


    public void PlayOnce(string name)
    {
        AudioSound s = Array.Find(AudioSounds, AudioSound => AudioSound.GetSoundName() == name);

        if (s == null)
        {
            Debug.LogWarning("AudioSound: " + name + " not found");
            return;
        }

        //Debug.Log("playing AudioSound once: " + name);

        if (!s.source.isPlaying) s.source.Play();
    }



    public void PlayOneShot(string name)
    {
        AudioSound s = Array.Find(AudioSounds, AudioSound => AudioSound.GetSoundName() == name);

        if (s == null)
        {
            Debug.LogWarning("AudioSound: " + name + " not found");
            return;
        }

        //Debug.Log("playing AudioSound one shot: " + name);

        s.source.PlayOneShot(s.GetSoundClip());
    }


    public void StopPlaying(string AudioSound)
    {
        AudioSound s = Array.Find(AudioSounds, item => item.GetSoundName() == AudioSound);
        if (s == null)
        {
            Debug.LogWarning("AudioSound: " + name + " not found!");
            return;
        }

        if (s.IsSoundPlaying())
        {
            //Debug.Log("stopping AudioSound: " + AudioSound);

            s.SetSoundPlaying(false);
            s.source.Stop();
        }
    }

    public bool IsAudioSoundPlaying(string name)
    {
        AudioSound s = Array.Find(AudioSounds, item => item.GetSoundName() == name);
        return s.IsSoundPlaying();
    }


    public void VolumeChange(float inc)
    {
        //add volume change here
    }

    public void VolumeSet(float set)
    {
        //add volume change here
    }


    public void VolumeFadeIn()
    {
        //add volume change here
    }



    public void VolumeFadeOut(string AudioSoundName)
    {
        //Debug.Log("Fading " + AudioSoundName);
        AudioSound s = Array.Find(AudioSounds, AudioSound => AudioSound.GetSoundName() == AudioSoundName);
        //Debug.Log(s.name + " volume: " + s.source.volume);
        float volume = s.source.volume;
        if (volume >= 0.1f) { StartCoroutine(Lower(volume, s)); }
    }

    IEnumerator Lower(float volume, AudioSound s)
    {
        volume -= 0.1f;
        s.source.volume = volume;
        //Debug.Log(s.name + " volume: " + s.source.volume);
        yield return new WaitForSeconds(0.1f);
        if (volume >= 0.1f) { StartCoroutine(Lower(volume, s)); }
    }
}
