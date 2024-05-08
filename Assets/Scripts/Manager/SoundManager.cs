using System.Collections.Generic;
using UnityEngine;

public class SoundManager : BaseManager
{
    public static SoundManager Instance;

    private Dictionary<FieldObject, Dictionary<string, AudioClip>> m_objAudioDictionary;
    private Dictionary<string, Dictionary<string, AudioClip>> m_BGMDictionary;
    private AudioSource[] m_audioSources;

    public override void Init()
    {
        if (Instance.IsNotNull())
            return;

        Instance = this;

        m_objAudioDictionary = new Dictionary<FieldObject, Dictionary<string, AudioClip>>();
        m_BGMDictionary = new Dictionary<string, Dictionary<string, AudioClip>>();
        m_audioSources = new AudioSource[(int)SoundTypeEnum.Count];

        InitSoundTypeObject();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void InitSoundTypeObject()
    {
        string[] soundNames = System.Enum.GetNames(typeof(SoundTypeEnum));
        for (int i = 0; i < soundNames.Length; i++)
        {
            if (soundNames[i] == DefineStrings.Count)
                continue;

            var go = new GameObject { name = soundNames[i] };
            m_audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.parent = gameObject.transform;
        }

        //bgm -> loop
        m_audioSources[(int)SoundTypeEnum.BGM].loop = true;
    }

    public void PlayEffectAudio(FieldObject obj, string audioName, float volume = 1.0f, float pitch = 1.0f)
    {
        if (audioName == string.Empty)
            return;

        if (m_objAudioDictionary.TryGetValue(obj, out var audioDic) == false)
            return;

        if (audioDic.TryGetValue(audioName, out var audioClip) == false)
            return;

        AudioSource audioSource = m_audioSources[(int)SoundTypeEnum.Effect];
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayEffectAudio(string audioName, float volume = 1.0f, float pitch = 1.0f)
    {
        var audioClip = GetAudioClip(audioName);
        if (audioClip.IsNull())
        {
            Debug.Log($"Cannot Found Audio!");
            return;
        }

        AudioSource audioSource = m_audioSources[(int)SoundTypeEnum.Effect];
        audioSource.pitch = pitch; 
        audioSource.volume = volume;
        audioSource.PlayOneShot(audioClip);
    }

    private AudioClip GetAudioClip(string audioName)
    {
        string path = DefineStrings.SoundPath + audioName;
        var audioClip = Resources.Load<AudioClip>(path);
        if (audioClip.IsNull())
            return null;

        return audioClip;
    }

    public void PlayBGM(string BGMName, float pitch = 1.0f)
    {
        var audioClip = GetAudioClip(BGMName);
        if (audioClip.IsNull())
        {
            Debug.Log($"Cannot Found Audio!");
            return;
        }

        AudioSource audioSource = m_audioSources[(int)SoundTypeEnum.BGM];
        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.pitch = pitch;
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void LoadObjAudioClip(FieldObject obj, Dictionary<string, string> audioDic)
    {
        var audioClipDic = new Dictionary<string, AudioClip>();
        foreach (var audio in audioDic)
        {
            string path = DefineStrings.SoundPath + audio.Value;
            AudioClip audioClip = Resources.Load<AudioClip>(path);
            if (audioClip.IsNull())
                continue;

            audioClipDic.Add(audio.Key, audioClip);
        }

        if (audioClipDic.Count == 0)
            return;

        if (m_objAudioDictionary.ContainsKey(obj))
            return;

        m_objAudioDictionary.Add(obj, audioClipDic);
    }

    public void RemoveObjectInfo(FieldObject obj)
    {
        if (m_objAudioDictionary.ContainsKey(obj) == false)
            return;

        m_objAudioDictionary.Remove(obj);
    }

    public override void UpdateManager()
    {
    }

    public override void Clear()
    {
        foreach (var audioSource in m_audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }

        m_objAudioDictionary.Clear();
        m_BGMDictionary.Clear();
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}