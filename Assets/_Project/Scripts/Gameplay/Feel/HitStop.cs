using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay
{
    internal static class HitStop
    {
        private static float remaining;
        private static bool running;

        internal static void Play(float duration)
        {
            remaining = Mathf.Max(remaining, duration);
            if (running)
                return;
            RunAsync().Forget();
        }
        private static async UniTaskVoid RunAsync()
        {
            running = true;
            Time.timeScale = 0f;
            try
            {
                while (remaining > 0)
                {
                    await UniTask.Yield();
                    remaining -= Time.unscaledDeltaTime;
                }
            }
            finally
            {
                remaining = 0;
                Time.timeScale = 1;
                running = false;
            }
        }
    }
}
