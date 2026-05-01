using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicZoneTrigger : MonoBehaviour
{
    public enum ZoneType
    {
        FirstRoom,
        BossRoom
    }

    [SerializeField] private ZoneType zoneType;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool wasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasTriggered && triggerOnlyOnce)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (MusicController.Instance == null)
        {
            Debug.LogWarning("MusicController not found in scene.");
            return;
        }

        switch (zoneType)
        {
            case ZoneType.FirstRoom:
                MusicController.Instance.EnterExplorationRoom();
                break;
            case ZoneType.BossRoom:
                MusicController.Instance.EnterBossRoom();
                break;
        }

        wasTriggered = true;
    }
}
