using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace EarlyAccessMiscTweaks.Tweaks
{
    internal class ArtificerTweaks
    {
        public ArtificerTweaks()
        {
            //Primary attack sped scaling
            On.RoR2.GenericSkill.RecalculateFinalRechargeInterval += PrimaryReloadAttackSpeed;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            //Flamethrower buffs
            On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += IncreaseFlamethrowerRange;
            IL.EntityStates.Mage.Weapon.Flamethrower.FixedUpdate += Flamethrower_FixedUpdate;
        }

        private void Flamethrower_FixedUpdate(MonoMod.Cil.ILContext il)
        {
            bool error = true;
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<EntityStates.Mage.Weapon.Flamethrower>("tickFrequency")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.Mage.Weapon.Flamethrower, float>>((tickFrequency, self) =>
                {
                    return tickFrequency * self.attackSpeedStat;
                });

                if (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<EntityStates.Mage.Weapon.Flamethrower>("tickFrequency")))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<float, EntityStates.Mage.Weapon.Flamethrower, float>>((tickFrequency, self) =>
                    {
                        return tickFrequency * self.attackSpeedStat;
                    });
                    error = false;
                }
            }

            if (error)
            {
                Debug.LogError("EarlyAccessMiscTweaks ArtificerTweaks Flamethrower Scaling IL Hook failed.");
            }
        }

        private void IncreaseFlamethrowerRange(On.EntityStates.Mage.Weapon.Flamethrower.orig_OnEnter orig, EntityStates.BaseState self)
        {
            EntityStates.Mage.Weapon.Flamethrower.maxDistance = 30f;
            orig(self);
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.skillLocator && self.skillLocator.primary && self.skillLocator.primary.skillNameToken == "MAGE_PRIMARY_NAME")
            {
                self.skillLocator.primary.RecalculateFinalRechargeInterval();
            }
        }

        private void PrimaryReloadAttackSpeed(On.RoR2.GenericSkill.orig_RecalculateFinalRechargeInterval orig, GenericSkill self)
        {
            orig(self);

            //Pre-SkillDef/SkillIndex name matching, woohoo!
            //Pre LanguageAPI tokens, woohoo!
            if (self.characterBody && self.skillNameToken == "MAGE_PRIMARY_NAME" && self.characterBody.attackSpeed > 0f)
            {
                self.finalRechargeInterval = self.finalRechargeInterval / self.characterBody.attackSpeed;
            }
        }
    }
}
