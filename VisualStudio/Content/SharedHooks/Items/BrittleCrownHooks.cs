using RoR2;
using R2API;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine;
using TMPro;

namespace LunarsOfExiguity;
public class BrittleCrownHooks
{
    private static readonly string InternalName = "BrittleCrownHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public static GameObject ProvidenceSymbol;
    public static GameObject SymbolExplodeEffect;
    private static readonly Color BaseSymbolColor = new(0f, 93f / 255f, 85f / 255f);

    public BrittleCrownHooks()
    {
        ReworkItemEnabled = BrittleCrownRework.Rework_Enabled.Value;
        PureItemEnabled = PureCrownItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += DisableGold;
            IL.RoR2.HealthComponent.TakeDamageProcess += ModifyDamage;
            On.RoR2.UI.ScoreboardStrip.UpdateMoneyText += FixNegativeMoney;
            IL.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += AllowDebt;
            IL.RoR2.ConvertPlayerMoneyToExperience.FixedUpdate += PreventMoney;
        }
        if (PureItemEnabled)
        {
            CreateFreeVisual();
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += RefundFreeUnlock;
            On.RoR2.PurchaseInteraction.Awake += AddFreeComponent;
        }
    }

    private void PreventMoney(ILContext il)
    {
        ILCursor cursor = new(il);
        int masterIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdloc(3),
            x => x.MatchCallOrCallvirt<GameObject>(nameof(GameObject.GetComponent)),
            x => x.MatchStloc(out masterIndex)
        );

        if (masterIndex != -1 && cursor.TryGotoNext(x => x.MatchLdarg(0)))
        {
            cursor.Emit(OpCodes.Ldloc, masterIndex);
            cursor.EmitDelegate<Action<CharacterMaster>>(self =>
            {
                if ((int)self.money < 0) self.money = 0;
            });
        }
    }

    private void CreateFreeVisual()
    {
        ProvidenceSymbol = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineChance/ShrineChance.prefab").WaitForCompletion().InstantiateClone("TempShrinePrefab", true).transform.FindChild("Symbol").gameObject;
        MeshRenderer mesh = ProvidenceSymbol.GetComponent<MeshRenderer>();
        Material newMat = new(mesh.sharedMaterial);

        newMat.mainTexture = LoEPlugin.Bundle.LoadAsset<Sprite>("PureCrownSymbol").texture;
        newMat.SetColor("_TintColor", BaseSymbolColor);
        newMat.SetFloat("_AlphaBoost", 0.6f);
        mesh.sharedMaterial = newMat;

        ProvidenceSymbol.transform.localScale *= 1.5f;

        SymbolExplodeEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/MonstersOnShrineUse"), "PureCrownExplode", true);
        UnityEngine.Object.Destroy(SymbolExplodeEffect.transform.FindChild("Chunks01_Ps").gameObject);
        UnityEngine.Object.Destroy(SymbolExplodeEffect.transform.FindChild("Chunks01_Ps (1)").gameObject);
        UnityEngine.Object.Destroy(SymbolExplodeEffect.transform.FindChild("Rings").gameObject);

        foreach (ParticleSystemRenderer particle in SymbolExplodeEffect.transform.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            if (!particle.sharedMaterial) continue;
            Material particleMat = new(particle.sharedMaterial);
            particleMat.DisableKeyword("VERTEXCOLOR");
            particleMat.SetColor("_TintColor", BaseSymbolColor);
            particle.sharedMaterial = particleMat;
        }

        SymbolExplodeEffect.transform.GetComponentInChildren<Light>().color = BaseSymbolColor;
        SymbolExplodeEffect.transform.FindChild("BlastDark_Ps").localScale = Vector3.one * 1.25f;

        new EffectDef()
        {
            prefab = SymbolExplodeEffect,
            prefabName = "PureCrownExplode",
            prefabEffectComponent = SymbolExplodeEffect.GetComponent<EffectComponent>()
        };

        ContentAddition.AddEffect(SymbolExplodeEffect);
    }

    private void DisableGold(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchNewobj(typeof(RoR2.Orbs.GoldOrb)),
            x => x.MatchStloc(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchLdarg(1)
        ))
        {
            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdflda<DamageInfo>(nameof(DamageInfo.procChainMask)),
                x => x.MatchLdcI4(out _)
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
        }
        else Log.Warning(InternalName + " - #1 (DisableGold) Failure");
    }
    private void ModifyDamage(ILContext il)
    {
        ILCursor cursor = new(il);
        int damageIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
            x => x.MatchStloc(out damageIndex)
        );

        if (damageIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdloc(out _),
            x => x.MatchLdarg(0),
            x => x.MatchCall<HealthComponent>("get_fullCombinedHealth"),
            x => x.MatchDiv(),
            x => x.MatchLdloc(out _),
            x => x.MatchCallOrCallvirt<CharacterMaster>("get_money")
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldloc, damageIndex);

            cursor.EmitDelegate<Func<HealthComponent, float, float>>((self, damage) =>
            {
                float damageMod = damage;
                int debtMoney = (int) self.body.master.money;

                if (debtMoney < 0)
                {
                    Log.Debug("Damage Before Debt: " + damageMod);

                    damageMod *= 1f + Mathf.Abs(debtMoney * BrittleCrownRework.Debt_Damage.Value) / 100f / Run.instance.difficultyCoefficient;

                    Log.Debug("Damage After Debt: " + damageMod);

                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.loseCoinsImpactEffectPrefab, new EffectData()
                    {
                        origin = self.body.corePosition,
                        scale = self.body.radius * 3f
                    }, true);
                }

                return damageMod;
            });

            cursor.Emit(OpCodes.Stloc, damageIndex);

            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.ItemCounts.goldOnHurt))
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
        }
        else Log.Warning(InternalName + " - #1 (ModifyDamage) Failure");
    }
    private void FixNegativeMoney(On.RoR2.UI.ScoreboardStrip.orig_UpdateMoneyText orig, RoR2.UI.ScoreboardStrip self)
    {
        if (self.master && self.master.money != self.previousMoney)
        {
            self.previousMoney = self.master.money;
            string cashPlacement = (int) self.previousMoney >= 0 ? "$" : "-$";
            self.moneyText.text = string.Format("{0}{1}", cashPlacement, Mathf.Abs((int) self.previousMoney));
        }
    }
    private void AllowDebt(ILContext il)
    {
        ILCursor cursor = new(il);
        int numIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.cost)),
            x => x.MatchStloc(out numIndex)
        );

        if (numIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.costType)),
            x => x.MatchCallOrCallvirt(typeof(CostTypeCatalog), nameof(CostTypeCatalog.GetCostTypeDef))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 1);
            cursor.Emit(OpCodes.Ldloc, numIndex);

            cursor.EmitDelegate<Func<Interactor, int, int>>((activator, cost) =>
            {
                int modCost = cost;
                if (activator.GetComponent<CharacterBody>().inventory?.GetItemCount(RoR2Content.Items.GoldOnHit) > 0) modCost = 0;
                return modCost;
            });

            cursor.Emit(OpCodes.Stloc, numIndex);
        }
        else Log.Warning(InternalName + " - #1 (AllowDebt) Failure");
    }

    private void RefundFreeUnlock(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchLdcI4(out _),
            x => x.MatchStloc(out _),
            x => x.MatchLdarg(1),
            x => x.MatchCallOrCallvirt<Component>(nameof(Component.GetComponent)),
            x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.FreeUnlocks))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldarg, 1);

            cursor.EmitDelegate<Action<PurchaseInteraction, Interactor>>((self, activator) =>
            {
                if (self.cost == 0) activator.GetComponent<CharacterBody>().AddBuff(DLC2Content.Buffs.FreeUnlocks);
            });
        }
        else Log.Warning(InternalName + " - #1 (RefundFreeUnlock) Failure");
    }
    private void AddFreeComponent(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
    {
        orig(self);
        if (self.costType == CostTypeIndex.Money) self.gameObject.AddComponent<CrownFreePurchase>();
    }

    public class CrownFreePurchase : NetworkBehaviour
    {
        [SyncVar]
        private bool ShowIcon;
        private PurchaseInteraction Self;

        private GameObject ProvIcon;
        private GameObject Hologram;

        public void Awake()
        {
            Self = GetComponent<PurchaseInteraction>();
            int itemCount = Util.GetItemCountGlobal(PureCrownItem.ItemDef.itemIndex, false, false);

            if (itemCount > 0 && Util.CheckRoll(PureCrownItem.Chance_Free.Value, itemCount - 1))
            {
                CreateVisual();
                ShowIcon = true;
                Hologram = transform.FindChild("HologramPivot")?.gameObject;
            }
            else
            {
                enabled = false;
                Destroy(this);
            }
        }
        public void LateUpdate()
        {
            if (!ProvIcon || !Self) return;

            if (Self.Networkcost != 0) Self.Networkcost = 0;
            if (Self.cost != 0) Self.cost = 0;

            if (ShowIcon != Self.available)
            {
                ShowIcon = Self.available;
                ProvIcon.SetActive(ShowIcon);

                if (!ShowIcon)
                {
                    EffectManager.SpawnEffect(SymbolExplodeEffect, new EffectData
                    {
                        origin = ProvIcon.transform.position,
                        rotation = UnityEngine.Random.rotation
                    }, true);
                }
            }

            if (Hologram && Hologram.transform.GetComponentInChildren<TextMeshPro>())
            {
                TextMeshPro text = Hologram.transform.GetComponentInChildren<TextMeshPro>();
                text.text = "...";
                text.color = BaseSymbolColor;
            }
        }
        private void CreateVisual()
        {
            ProvIcon = Instantiate(ProvidenceSymbol);
            ProvIcon.name = "PureHonorSymbol";
            ProvIcon.transform.parent = gameObject.transform;
            ProvIcon.transform.localPosition = new Vector3(0f, 5f, 0f);
        }
    }
}