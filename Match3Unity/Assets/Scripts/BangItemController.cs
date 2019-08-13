using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BangItemController : MonoBehaviour
{
    Animation anim;

    void Awake()
    {
        anim = GetComponent<Animation>();

        if (anim != null)
            anim.Play();
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (!anim.isPlaying)
            Destroy(gameObject);
    }
}
