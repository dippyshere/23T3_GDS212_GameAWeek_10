using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicTracks;
    public AudioClip desertMusic;
    private AudioSource audioSource;

    public GameObject[] settlementBoundaries; // Use GameObjects to represent the boundaries of settlements

    private int currentTrackIndex = -1;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        InvokeRepeating(nameof(CheckPlayerPosition), 0f, 1f);
    }

    private void CheckPlayerPosition()
    {
        bool nearSettlement = false;

        for (int i = 0; i < settlementBoundaries.Length; i++)
        {
            Collider settlementCollider = settlementBoundaries[i].GetComponent<Collider>();
            if (!settlementCollider.bounds.Contains(PlayerController.Instance.transform.position))
            {
                continue;
            }

            nearSettlement = true;
            if (i != currentTrackIndex)
            {
                currentTrackIndex = i;
                StartCoroutine(TransitionToSettlementTrack());
            }
            break;
        }

        if (nearSettlement || currentTrackIndex == -1)
        {
            return;
        }

        currentTrackIndex = -1;
        StartCoroutine(TransitionToDesertTrack());
    }

    private IEnumerator TransitionToSettlementTrack()
    {
        float fadeDuration = 2.0f; // You can adjust this value
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        SetMusicTrackForPlayer();
    }

    private IEnumerator TransitionToDesertTrack()
    {
        float fadeDuration = 2.0f; // You can adjust this value
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        SetDefaultMusicTrack();
    }

    private void SetMusicTrackForPlayer()
    {
        if (currentTrackIndex >= 0 && currentTrackIndex < musicTracks.Length)
        {
            AudioClip nextTrack = musicTracks[currentTrackIndex];

            StartCoroutine(Crossfade(nextTrack));
        }
        else
        {
            SetDefaultMusicTrack();
        }
    }

    private void SetDefaultMusicTrack()
    {
        AudioClip nextTrack = desertMusic;

        StartCoroutine(Crossfade(nextTrack));
    }

    private IEnumerator Crossfade(AudioClip nextTrack)
    {
        float fadeDuration = 2.0f; // You can adjust this value

        // Crossfade out the current track
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Set and play the next track
        audioSource.clip = nextTrack;
        audioSource.Play();

        // Crossfade in the next track
        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += Time.deltaTime / fadeDuration;
            yield return null;
        }
    }
}