using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal static class HitStop
    {
        internal static async UniTaskVoid Play(float duration)
        {
            Time.timeScale = 0;
            await UniTask.Delay(TimeSpan.FromSeconds(duration), true);
            Time.timeScale = 1;
        }
    }
}
