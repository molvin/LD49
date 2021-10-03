using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Button))]
public class ButtonAudio : MonoBehaviour
{

    private AudioSource myAudioSource;
    private string pathToHoverClip = "Audio/MenuHover";
    private string pathToClickClip = "Audio/MenuClick";

    private AudioClip hoverSoundClip;
    private AudioClip clickSoundClip;

    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(onClick);
        myAudioSource = this.GetComponent<AudioSource>();
        hoverSoundClip = Resources.Load<AudioClip>(pathToHoverClip);
        clickSoundClip = Resources.Load<AudioClip>(pathToClickClip);
    }

    public void OnHover()
    {
        myAudioSource.PlayOneShot(hoverSoundClip);
    }
    private void onClick()
    {
        myAudioSource.PlayOneShot(clickSoundClip);

    }

}
