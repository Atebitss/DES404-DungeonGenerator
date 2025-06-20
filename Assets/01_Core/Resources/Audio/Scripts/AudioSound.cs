using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class AudioSound
{
    //sound source
    public AudioSource source;


    //sound name
    [SerializeField] private string name = "undefined";
    public string GetSoundName() { return name; }


    //sound clip
    [SerializeField] private AudioClip clip;
    public void SetSoundClip(AudioClip newClip) { clip = newClip; }
    public AudioClip GetSoundClip() { return clip; }


    //sound volume
    [Range(0f, 5f)]
    [SerializeField] private float volume = 1f;
    public void SetSoundVolume(float newVolume) { volume = newVolume; }
    public float GetSoundVolume() { return volume; }


    //sound pitch
    [Range(.1f, 5f)]
    [SerializeField] private float pitch = 1f;
    public void SetSoundPitch(float newPitch) { pitch = newPitch; }
    public void AlterSoundPitch(float newPitch) { pitch += newPitch; }
    public float GetSoundPitch() { return pitch; }


    //sound loop
    [SerializeField] private bool loop = false;
    public void SetSoundLooping(bool looping) { loop = looping; }
    public void SwitchSoundLooping() { if (loop) { loop = false; } else if (!loop) { loop = true; } }
    public bool IsSoundLooping() { return loop; }


    //sound play
    [SerializeField] private bool play = false;
    public void SetSoundPlaying(bool playing) { playing = play; }
    public bool IsSoundPlaying() { return play; }
}
