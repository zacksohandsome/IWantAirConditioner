using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource backgroundPlayer;
    public AudioSource[] soundEffectPlayers;
    public int playerIndex = 0;


    public AudioClip win;
    public AudioClip lose;
    public AudioClip foundMaster;
    public AudioClip setFlag;
    public AudioClip ahs;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayWin()
    {
        PlaySound(win);
    }

    public void PlayLose()
    {
        PlaySound(lose);
    }

    public void PlayFoundMaster()
    {
        PlaySound(foundMaster);
    }

    public void PlaySetFlag()
    {
        PlaySound(setFlag);
    }

    public void PlayAhs()
    {
        PlaySound(ahs);
    }

    public void PlaySound(AudioClip getSound)
    {
        int index = playerIndex % soundEffectPlayers.Length;
        soundEffectPlayers[index].PlayOneShot(getSound);
        playerIndex++;
    }

    public void StopAllSoundEffect()
    {
        for(int i = 0; i < soundEffectPlayers.Length; i++)
        {
            soundEffectPlayers[i].Stop();
        }
    }

    public void SetBackgroundPlayer(bool isOn)
    {
        if(isOn)
        {
            backgroundPlayer.Play();
            backgroundPlayer.mute = false;
        }
        else
        {
            backgroundPlayer.Stop();
            backgroundPlayer.mute = true;
        }
    }

    public void PauseBGM()
    {
        backgroundPlayer.Pause();
    }

    public void CallFadeInBGM()
    {
        StartCoroutine(FadeInBGM());
    }

    IEnumerator FadeInBGM()
    {
        if(backgroundPlayer.isPlaying)
        {
            yield break;
        }

        backgroundPlayer.volume = 0;
        float nowVolume = 0;

        backgroundPlayer.Play();
        while(nowVolume < 1)
        {
            //Debug.Log("nowVolume = " + nowVolume);
            nowVolume = backgroundPlayer.volume + 0.005f;
            backgroundPlayer.volume = nowVolume;
            yield return 0;
        }

        yield return 0;
    }
}
