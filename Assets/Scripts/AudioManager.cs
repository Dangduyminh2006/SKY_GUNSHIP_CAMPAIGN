using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý phát nhạc nền (BGM) và hiệu ứng âm thanh (SFX) 8-bit
/// </summary>
public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Sound Clips")]
    public List<SoundEffect> sfxList;
    public AudioClip survivalBGM;
    public AudioClip bossBGM;

    private Dictionary<string, SoundEffect> sfxDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        sfxDictionary = new Dictionary<string, SoundEffect>(System.StringComparer.OrdinalIgnoreCase);
        if (sfxList != null)
        {
            foreach (var sfx in sfxList)
            {
                if (sfx != null && !string.IsNullOrEmpty(sfx.name) && !sfxDictionary.ContainsKey(sfx.name))
                {
                    sfxDictionary.Add(sfx.name, sfx);
                }
            }
        }
    }

    void Start()
    {
        PlayBGM("SurvivalBGM");
    }

    public void PlaySFX(string soundName)
    {
        if (sfxSource == null || sfxDictionary == null) return;

        if (sfxDictionary.TryGetValue(soundName, out SoundEffect sound))
        {
            if (sound != null && sound.clip != null)
            {
                sfxSource.PlayOneShot(sound.clip, sound.volume);
            }
        }
    }

    public void PlayBGM(string bgmType)
    {
        if (bgmSource == null) return;

        AudioClip clipToPlay = null;
        if (bgmType == "SurvivalBGM")
        {
            clipToPlay = survivalBGM;
        }
        else if (bgmType == "BossBGM")
        {
            clipToPlay = bossBGM;
        }

        if (clipToPlay != null && bgmSource.clip != clipToPlay)
        {
            bgmSource.clip = clipToPlay;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
}
