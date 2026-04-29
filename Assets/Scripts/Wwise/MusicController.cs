using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AkSoundEngine.LoadBank("Music", out _);
        AkSoundEngine.PostEvent("Play_Level01_Music", gameObject);
        SetState("Exploration");
    }

    public void SetState(string stateName)
    {
        AkSoundEngine.SetState("MusicState", stateName);
    }
}