using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioSource Music; 

    private void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.mute = s.mute;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.output;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }
        s.source.Play();
    }

    public void MuteSounds(bool mute)
    {
        foreach (Sound s in sounds)
        {
            s.source.mute = mute;
        }
    }

    public void MuteMusic(bool mute)
    {
        Music.mute = mute;
    }
}
