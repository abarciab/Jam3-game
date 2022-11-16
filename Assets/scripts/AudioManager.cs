using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public int ID;
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 1;
        [Range(-3, 3)]
        public float pitch = 1;
        public bool looping;
    }
    public List<Sound> sounds = new List<Sound>();

    public static AudioManager instance;

    public AudioSource music;
    public AudioSource global;
    int currentPriority = -1;

    private void Awake() {
        instance = this;
    }

    private Sound getSoundFromID(int soundID) {
        for (int i = 0; i < sounds.Count; i++) {
            if (sounds[i].ID == soundID) {
                return sounds[i];
            }
        }
        Debug.LogError("Tried to get sound with invalid ID. ID: " + soundID);
        return null;
    }

    public void SetMusicVolume(float vol, bool playIfPaused = true)     //set volume on music audiosource
    {
        music.volume = vol;
        if (playIfPaused && !music.isPlaying) {
            music.Play();
        }
    }

    public void StopSoundHere(int soundID, AudioSource source)      //stop given sound at given audiosource
    {
        Sound toPlay = getSoundFromID(soundID);

        if (source.clip == toPlay.clip && source.isPlaying) {
            source.Stop();
        }
    }

    public void PlayGlobal(int soundID, float volume = -1, bool restart = false, int _priority = -1)      //play given sound from global audiosource
    {

        PlayHere(soundID, global, volume, restart, priority: _priority);
    }

    public void PlayMusic(int soundID, float volume = -1, bool restart = false)      //play given sound from global audiosource
    {
        PlayHere(soundID, music, volume, restart);
    }

    public void PlayHere(int soundID, AudioSource source, float volume = -1, bool restart = false, int priority = -1)      //play given sound from given audiosource
    {
        if (priority != -1 && priority < currentPriority && source.isPlaying) {
            return;
        }
        currentPriority = priority;

        Sound toPlay = getSoundFromID(soundID);

        source.volume = volume == -1 ? toPlay.volume : volume;
        source.pitch = toPlay.pitch;
        source.loop = toPlay.looping;

        if (source.clip != toPlay.clip || !source.isPlaying || restart == true) {
            source.clip = toPlay.clip;
            source.Play();
        }
    }
}
