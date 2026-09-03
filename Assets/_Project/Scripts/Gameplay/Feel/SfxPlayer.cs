using UnityEngine;

namespace Game.Gameplay
{
    public class SfxPlayer : MonoBehaviour
    {
        [SerializeField]
        AudioSource source;

        private static SfxPlayer instance;

        private void Awake()
        {
            if(source == null)
            {
                Debug.LogWarning($"{gameObject.name}: source 미지정 — 효과음이 하나도 안 난다", this);
                enabled = false;
                return;
            }
            if (instance == null)
                instance = this;
        }

        internal static void Play(AudioClip[] clips)    
        {
            if (instance == null)
                return;
            if (clips == null || clips.Length == 0)
                return;

            instance.PlayOne(clips[Random.Range(0, clips.Length)]);
        }

        private void PlayOne(AudioClip clip)
        {
            if (clip == null)
                return;
            source.PlayOneShot(clip);
        }

        private void OnDestroy()
        {
            if(instance == this)
                instance = null;
        }
    }
}
