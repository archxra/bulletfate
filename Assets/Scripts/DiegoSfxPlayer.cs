using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiegoSfxPlayer : MonoBehaviour
{
    private const string DiegoStreamingFolder = "Audio/SFX/Diego";
    private const string DiegoAssetsFolder = "Assets/Audio/SoundForGame/SoundForGame/SoundsOfCharacters/Diego";

    private AudioSource audioSource;
    private AudioClip shootClip;
    private AudioClip abilityClip;
    private AudioClip ultimateClip;
    private bool loadingStarted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        audioSource.ignoreListenerVolume = true;
        audioSource.mute = false;
    }

    private void Start()
    {
        if (!loadingStarted)
        {
            loadingStarted = true;
            StartCoroutine(LoadDiegoClips());
        }
    }

    public void PlayShoot()
    {
        PlayIfLoaded(shootClip, "LMB");
    }

    public void PlayAbility()
    {
        PlayIfLoaded(abilityClip, "RMB");
    }

    public void PlayUltimate()
    {
        PlayIfLoaded(ultimateClip, "R");
    }

    private IEnumerator LoadDiegoClips()
    {
        // Build-safe path: clips copied into StreamingAssets.
        yield return LoadFromStreamingAssets("ДиегоВыстрел.wav", clip => shootClip = clip);
        yield return LoadFromStreamingAssets("СпособностьДиего.wav", clip => abilityClip = clip);
        yield return LoadFromStreamingAssets("УльтаДиего.wav", clip => ultimateClip = clip);
        if (ultimateClip == null)
            yield return LoadFromStreamingAssets("УльтаДиего.mp3", clip => ultimateClip = clip);

#if UNITY_EDITOR
        // Editor fallback if files are not present in StreamingAssets yet.
        if (shootClip == null) shootClip = LoadEditorClip("ДиегоВыстрел.wav");
        if (abilityClip == null) abilityClip = LoadEditorClip("СпособностьДиего.wav");
        if (ultimateClip == null) ultimateClip = LoadEditorClip("УльтаДиего.wav");
        if (ultimateClip == null) ultimateClip = LoadEditorClip("УльтаДиего.mp3");
#endif

        Debug.Log(
            $"[DiegoSfx] Loaded clips | Shoot: {(shootClip != null)} | Ability: {(abilityClip != null)} | Ultimate: {(ultimateClip != null)}");
    }

    private void PlayIfLoaded(AudioClip clip, string actionName)
    {
        if (clip != null)
        {
            // Force-unmute runtime audio path before playback.
            AudioListener.pause = false;
            if (AudioListener.volume <= 0f) AudioListener.volume = 1f;
            audioSource.mute = false;
            if (!audioSource.enabled) audioSource.enabled = true;

            audioSource.PlayOneShot(clip);
            Debug.Log(
                $"[DiegoSfx] Played: {clip.name} ({actionName}) | srcVol={audioSource.volume} srcMute={audioSource.mute} listenerVol={AudioListener.volume} listenerPause={AudioListener.pause}");
            return;
        }

        Debug.LogWarning($"[DiegoSfx] Clip not loaded for action: {actionName}");
    }

#if UNITY_EDITOR
    private static AudioClip LoadEditorClip(string fileName)
    {
        string assetPath = $"{DiegoAssetsFolder}/{fileName}";
        return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
    }
#endif

    private IEnumerator LoadFromStreamingAssets(string fileName, Action<AudioClip> assign)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, DiegoStreamingFolder, fileName);
        string fileUri = new Uri(fullPath).AbsoluteUri;
        AudioType audioType = GetAudioTypeByExtension(Path.GetExtension(fileName));

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUri, audioType))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(request);
                if (loadedClip != null)
                {
                    loadedClip.name = Path.GetFileNameWithoutExtension(fileName);
                    assign(loadedClip);
                }
            }
        }
    }

    private static AudioType GetAudioTypeByExtension(string extension)
    {
        string normalized = extension.ToLowerInvariant();
        if (normalized == ".wav") return AudioType.WAV;
        if (normalized == ".mp3") return AudioType.MPEG;
        return AudioType.UNKNOWN;
    }
}
