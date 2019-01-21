using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Match3Game
{
    public class BangItemController : MonoBehaviour
    {

        Animation anim;
        // Use this for initialization
        void Awake()
        {
            anim = GetComponent<Animation>();
            if (anim != null)
            {
                anim.Play();
            }
            else
            {
                Destroy(gameObject);
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (!anim.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}