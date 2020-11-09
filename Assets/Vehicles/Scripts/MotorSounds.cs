using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class MotorSoundAtSpeed
{
    public AudioClip Sound;
    public float Speed;
}
public class MotorSounds : MonoBehaviour
{
    public MotorSoundAtSpeed[] sounds;
    private AudioSource[] sources = new AudioSource[2];
    private int mainSource = 0;
    private int secondarySource = 1;
    private float prevIndex = 0;
    private void Awake()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].loop = true;
        }
        Array.Sort(sounds, delegate (MotorSoundAtSpeed sound1, MotorSoundAtSpeed sound2)
        {
            return sound1.Speed.CompareTo(sound2.Speed);
        });
    }
    public void MakeSound(float speed)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying)
            {
                sources[i].Play();
            }
        }
        speed = Mathf.Abs(speed);
        int index = GetSoundIndex(speed);
        sources[mainSource].clip = CalcAudioClip(index);
        sources[secondarySource].clip = CalcAudioClip(index - 1);
        sources[mainSource].pitch = CalcPitch(speed, index);
        sources[secondarySource].pitch = CalcPitch(speed, index - 1);
        sources[mainSource].volume = CalcVolume(speed, index);
        sources[secondarySource].volume = CalcVolume(speed, index - 1);
    }
    private int GetSoundIndex(float speed)
    {
        int index = sounds.Length;
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].Speed > speed)
            {
                index = i;
                break;
            }
        }
        if (prevIndex != index)
        {
            int tempSource = mainSource;
            mainSource = secondarySource;
            secondarySource = tempSource;
            prevIndex = index;
        }
        return index;
    }
    private AudioClip CalcAudioClip(int index)
    {
        if (index < 0 || index >= sounds.Length)
            return null;
        return sounds[index].Sound;
    }
    private float CalcPitch(float speed, int index)
    {
        if (index < 0 || index >= sounds.Length)
            return 1f;
        return speed / sounds[index].Speed;
    }
    private float CalcVolume(float speed, int index)
    {
        if (index < 0 || index >= sounds.Length)
            return 0f;

        if (speed >= sounds[index].Speed)
        {
            if (index + 1 >= sounds.Length)
            {
                return 1f;
            }
            else
            {
                return (sounds[index + 1].Speed - speed) / (sounds[index + 1].Speed - sounds[index].Speed);
            }
        }
        else
        {
            if (index - 1 < 0)
            {
                return speed / sounds[index].Speed;
            }
            else
            {
                return (speed - sounds[index - 1].Speed) / (sounds[index].Speed - sounds[index - 1].Speed);
            }
        }
    }
}
