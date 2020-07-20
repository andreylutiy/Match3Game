using UnityEngine;

namespace Match3Game
{
    public class BangItemController : MonoBehaviour
    {
        [SerializeField] private Animation Anim;

        void Awake()
        {
            if (Anim == null)
            {
                Destroy(gameObject);
                return;
            }

            Anim.Play();
        }

        void Update()
        {
            if (!Anim.isPlaying)
                Destroy(gameObject);
        }
    }
}