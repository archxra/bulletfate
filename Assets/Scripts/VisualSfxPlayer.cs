using UnityEngine;
using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VisualSfxPlayer : MonoBehaviour
{
    private const string VisualSoundsStreamingFolder = "Audio/SFX/VisualSounds";
    private const string VisualSoundsFolder = "Assets/Audio/SoundForGame/SoundForGame/VisualSounds";

    private static VisualSfxPlayer instance;

    private AudioSource audioSource;
    private AudioClip playerDeathClip;
    private AudioClip playerDamageClip;
    private AudioClip bossKilledClip;
    private float lastDamagePlayTime;

    public static void PlayPlayerDeath()
    {
        EnsureInstance().PlayClip(EnsureInstance().playerDeathClip);
    }

    public static void PlayPlayerDamage()
    {
        VisualSfxPlayer sfx = EnsureInstance();
        if (Time.time - sfx.lastDamagePlayTime < 0.08f) return;
        sfx.lastDamagePlayTime = Time.time;
        sfx.PlayClip(sfx.playerDamageClip);
    }

    public static void PlayBossKilled()
    {
        EnsureInstance().PlayClip(EnsureInstance().bossKilledClip);
    }

    public static void LoadMainMenuAfterDelay(float delaySeconds)
    {
        EnsureInstance().StartCoroutine(EnsureInstance().LoadSceneDelayed("MainMenu", delaySeconds));
    }

    private static VisualSfxPlayer EnsureInstance()
    {
        if (instance != null) return instance;

        instance = FindFirstObjectByType<VisualSfxPlayer>();
        if (instance != null) return instance;

        GameObject go = new GameObject("VisualSfxPlayer");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<VisualSfxPlayer>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        audioSource.ignoreListenerVolume = true;

        StartCoroutine(LoadVisualClips());
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        AudioListener.pause = false;
        if (AudioListener.volume <= 0f) AudioListener.volume = 1f;
        audioSource.PlayOneShot(clip);
    }

    private IEnumerator LoadVisualClips()
    {
        yield return LoadFromStreamingAssets("ЗвукСмерти.wav", clip => playerDeathClip = clip);
        yield return LoadFromStreamingAssets("ПолучениеУрона.wav", clip => playerDamageClip = clip);
        yield return LoadFromStreamingAssets("УбилБосса.wav", clip => bossKilledClip = clip);

#if UNITY_EDITOR
        if (playerDeathClip == null) playerDeathClip = LoadEditorClip("ЗвукСмерти.wav");
        if (playerDamageClip == null) playerDamageClip = LoadEditorClip("ПолучениеУрона.wav");
        if (bossKilledClip == null) bossKilledClip = LoadEditorClip("УбилБосса.wav");
#endif
    }

    private IEnumerator LoadFromStreamingAssets(string fileName, Action<AudioClip> assign)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, VisualSoundsStreamingFolder, fileName);
        string fileUri = new Uri(fullPath).AbsoluteUri;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.WAV))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    clip.name = Path.GetFileNameWithoutExtension(fileName);
                    assign(clip);
                }
            }
        }
    }

#if UNITY_EDITOR
    private static AudioClip LoadEditorClip(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>($"{VisualSoundsFolder}/{fileName}");
    }
#endif

    private IEnumerator LoadSceneDelayed(string sceneName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
