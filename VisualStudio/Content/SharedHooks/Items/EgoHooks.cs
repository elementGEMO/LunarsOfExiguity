using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using R2API;
using UnityEngine.PlayerLoop;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;
public class EgoHooks
{
    private static readonly string InternalName = "LunarSunHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public EgoHooks()
    {
        ReworkItemEnabled = EgoRework.Rework_Enabled.Value;
        //PureItemEnabled = PureFocusItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            IL.RoR2.CharacterBody.OnInventoryChanged += DisableBehavior;
        }
    }
    private static void DisableBehavior(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<CharacterBody>("get_inventory"),
            x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.LunarSun))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.EmitDelegate<Action<CharacterBody>>(self =>
            {
                self.AddItemBehavior<EgoReworkBehavior>(self.inventory.GetItemCount(DLC1Content.Items.LunarSun));
            });

            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_inventory"),
                x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.VoidMegaCrabItem))
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                //cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
            else Log.Warning(InternalName + " - #2 (DisableBehavior) Failure");
        }
        else Log.Warning(InternalName + " - #1 (DisableBehavior) Failure");
    }

    public class EgoReworkBehavior : CharacterBody.ItemBehavior
    {
        private GameObject OrbPrefab;
        private bool HasOrb;
        public void Awake()
        {
            Log.Debug("Hi");
            OrbPrefab = Addressables.LoadAsset<GameObject>("RoR2/DLC1/LunarSun/LunarSunProjectile.prefab").WaitForCompletion();
        }
        public void FixedUpdate()
        {
            if (!HasOrb)
            {
                HasOrb = true;

                FireProjectileInfo orbProjectile = new()
                {
                    projectilePrefab = OrbPrefab,
                    damageColorIndex = DamageColorIndex.Luminous,
                    damage = body.damage,
                    crit = body.RollCrit(),
                    force = 0f,
                    owner = gameObject,
                    position = body.transform.position,
                    rotation = Quaternion.identity
                };

                ProjectileManager.instance.FireProjectile(orbProjectile);
            }

            /*
            this.projectileTimer = 0f;
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = this.projectilePrefab,
                crit = this.body.RollCrit(),
                damage = this.body.damage * 3.6f,
                damageColorIndex = DamageColorIndex.Item,
                force = 0f,
                owner = base.gameObject,
                position = this.body.transform.position,
                rotation = Quaternion.identity
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            */
        }
    }
}