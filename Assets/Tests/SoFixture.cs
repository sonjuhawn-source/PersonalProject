using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Tests
{
    internal static class SoFixture
    {
        // 콤보 각 타의 데미지를 배열로 받는다. damages.Length 가 곧 콤보 수다.
        internal static WeaponData Weapon(int[] damages,
                                          float startup = 0.2f,
                                          float activeTime = 0.1f,
                                          float recovery = 0.2f,
                                          float swapCooldown = 3.3f)
        {
            WeaponData so = ScriptableObject.CreateInstance<WeaponData>();
            so.name = "TestWeapon";

            SerializedObject sw = new SerializedObject(so);

            SerializedProperty combo = sw.FindProperty("combo");
            combo.arraySize = damages.Length;

            for (int i = 0; i < damages.Length; i++)
            {
                SerializedProperty a = combo.GetArrayElementAtIndex(i);
                a.FindPropertyRelative("damage").intValue = damages[i];
                a.FindPropertyRelative("startup").floatValue = startup;
                a.FindPropertyRelative("activeTime").floatValue = activeTime;
                a.FindPropertyRelative("recovery").floatValue = recovery;
            }

            sw.FindProperty("swapCooldown").floatValue = swapCooldown;

            sw.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }
    }
}