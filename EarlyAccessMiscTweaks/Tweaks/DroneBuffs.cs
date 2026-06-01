using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EarlyAccessMiscTweaks.Tweaks
{
    internal class DroneBuffs
    {
        public DroneBuffs()
        {
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/Turret1Body");
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/Drone1Body");
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/Drone2Body");
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/MissileDroneBody");
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/MegaDroneBody");
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/BackupDroneBody");
            AddAmbientLevelToGameObject("Prefabs/CharacterBodies/BackupDroneOldBody");
        }

        private void AddAmbientLevelToGameObject(string path)
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (!prefab)
            {
                Debug.LogError("EarlyAccessMiscTweaks.Tweaks.DroneBuffs.AddAmbientLevelToGameObject - No prefab found at path " + path);
                return;
            }

            CharacterBody body = prefab.GetComponent<CharacterBody>();
            if (!body)
            {
                Debug.LogError("EarlyAccessMiscTweaks.Tweaks.DroneBuffs.AddAmbientLevelToGameObject - No CharacterBody for prefab at path " + path);
                return;
            }

            prefab.AddComponent<AmbientLevelScaling>();

            //Extra per-body modifications
            switch (path)
            {
                case "Prefabs/CharacterBodies/Turret1Body":
                    body.baseMaxHealth = 200f;
                    body.levelMaxHealth = 60f;
                    body.baseRegen = 20f * 0.6f;
                    body.levelRegen = 4f * 0.6f;
                    body.baseDamage = 18f;
                    body.levelDamage = 3.6f;
                    body.baseArmor = 30f;
                    break;
                case "Prefabs/CharacterBodies/Drone1Body":
                case "Prefabs/CharacterBodies/Drone2Body":
                    body.baseMaxHealth = 150f;
                    body.levelMaxHealth = 45f;
                    body.baseRegen = 5f * 0.6f;
                    body.levelRegen = 1f * 0.6f;
                    body.baseDamage = 10f;
                    body.levelDamage = 2f;
                    body.moveSpeed = 17f;
                    body.baseAcceleration = 24f;
                    break;
                case "Prefabs/CharacterBodies/MissileDroneBody":
                    body.baseMaxHealth = 225f;
                    body.levelMaxHealth = 68f;
                    body.baseRegen = 7.5f * 0.6f;
                    body.levelRegen = 1.5f * 0.6f;
                    body.baseDamage = 14f;
                    body.levelDamage = 2.8f;
                    body.moveSpeed = 12f;
                    body.baseAcceleration = 24f;
                    break;
                case "Prefabs/CharacterBodies/BackupDroneBody":
                case "Prefabs/CharacterBodies/BackupDroneOldBody":
                    body.baseMaxHealth = 225f;
                    body.levelMaxHealth = 68f;
                    body.baseRegen = 7.5f * 0.6f;
                    body.levelRegen = 1.5f * 0.6f;
                    body.baseDamage = 7f;
                    body.levelDamage = 1.4f;
                    body.moveSpeed = 20f;
                    body.baseAcceleration = 32f;
                    break;
                case "Prefabs/CharacterBodies/MegaDroneBody":
                    body.baseMaxHealth = 1200f;
                    body.levelMaxHealth = 360f;
                    body.baseRegen = 30f * 0.6f;
                    body.levelRegen = 8f * 0.6f;
                    body.baseArmor = 50f;
                    body.baseMoveSpeed = 20f;
                    body.baseDamage = 14f;
                    body.levelDamage = 4.2f; //5.6 for modern RoR2, 40% scaling
                    break;
                default:
                    break;
            }
        }
    }

    [RequireComponent(typeof(CharacterBody))]
    public class AmbientLevelScaling : MonoBehaviour
    {
        private CharacterBody body;
        private float internalCooldown = 0f;

        private void Start()
        {
            body = GetComponent<CharacterBody>();
            if (body.isPlayerControlled)
            {
                Destroy(this);
                return;
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active || !body || !body.inventory) return;

            if (internalCooldown > 0f)
            {
                internalCooldown -= Time.fixedDeltaTime;
                return;
            }

            int ambientLevel = Mathf.FloorToInt(Run.instance.targetMonsterLevel);
            int currentLevel = Mathf.FloorToInt(body.level);

            if (ambientLevel > currentLevel)
            {
                int diff = ambientLevel - currentLevel;
                body.inventory.GiveItem(ItemIndex.LevelBonus, diff);
                internalCooldown = 1f;
            }
            else if (ambientLevel < currentLevel)
            {
                int diff = currentLevel - ambientLevel;
                int currentLevelBonusCount = body.inventory.GetItemCount(ItemIndex.LevelBonus);
                if (currentLevelBonusCount > 0)
                {
                    body.inventory.RemoveItem(ItemIndex.LevelBonus, Mathf.Min(diff, currentLevelBonusCount));
                }
                internalCooldown = 1f;
            }
        }
    }
}
