using UnityEngine;
using System.Collections;

public class TokenManager : MonoBehaviour 
{
    private AudioSource sound;

    void Start ()
    {
        sound = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {        
        sound.Play();         
    }
}
