using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtils;
using ECS;
using EGamePlay.Combat;
using System;
using ET;
using ECSGame;

namespace EGamePlay
{
    public class AbilitySystem : AEntitySystem<Ability>,
        IAwake<Ability>,
        IEnable<Ability>,
        IDisable<Ability>,
        IUpdate<Ability>
    {
        public void Awake(Ability entity)
        {
            entity.Config = StaticConfig.Config.Get<AbilityConfig>(entity.ConfigObject.Id);
            entity.Name = entity.Config.Name;

            CheckAffectTargetType(entity);
            CheckTargetSelectType(entity);

            foreach (var item in entity.ConfigObject.Effects)
            {
                var abilityEffect = AbilityEffectSystem.Create(entity, item);
                abilityEffect.Init();
                entity.AbilityEffects.Add(abilityEffect);
            }

            foreach (var item in entity.ConfigObject.TriggerActions)
            {
                var abilityTrigger = AbilityTriggerSystem.Create(entity, item);
                abilityTrigger.Init();
                entity.AbilityTriggers.Add(abilityTrigger);
            }

            LoadExecution(entity);
        }

        private static void CheckAffectTargetType(Ability entity)
        {
            if (entity.Config.TargetGroup == "����")
            {
                entity.ConfigObject.AffectTargetType = SkillAffectTargetType.SelfTeam;
            }
            else if (entity.Config.TargetGroup == "�з�")
            {
                entity.ConfigObject.AffectTargetType = SkillAffectTargetType.EnemyTeam;
            }
            else if (entity.Config.TargetGroup == "����")
            {
                entity.ConfigObject.AffectTargetType = SkillAffectTargetType.Self;
            }
            else
            {
                Log.Error($"����Ŀ����Ӫ���� {entity.Config.TargetGroup}");
            }
        }

        private static void CheckTargetSelectType(Ability entity)
        {
            if (entity.IsSkill)
            {
                if (entity.Config.TargetSelect == "��ײ���")
                {
                    entity.ConfigObject.TargetSelectType = SkillTargetSelectType.CollisionSelect;
                }
                else if (entity.Config.TargetSelect == "����ָ��")
                {
                    entity.ConfigObject.TargetSelectType = SkillTargetSelectType.ConditionSelect;
                }
                else if (entity.Config.TargetSelect == "�ֶ�ָ��")
                {
                    entity.ConfigObject.TargetSelectType = SkillTargetSelectType.PlayerSelect;
                }
                else
                {
                    Log.Error($"Ŀ��ѡȡ���ʹ��� {entity.Config.TargetSelect}");
                }
            }
        }

        public void Enable(Ability entity)
        {
            foreach (var item in entity.AbilityEffects)
            {
                item.Enable = true;
            }
            foreach (var item in entity.AbilityTriggers)
            {
                item.Enable = true;
            }
        }

        public void Disable(Ability entity)
        {
            foreach (var item in entity.AbilityEffects)
            {
                item.Enable = false;
            }
            foreach (var item in entity.AbilityTriggers)
            {
                item.Enable = false;
            }
        }

        public void Update(Ability entity)
        {
            if (entity.GetComponent<AbilityDurationComponent>() is { } component)
            {
                AbilityDurationSystem.Update(entity, component);
            }
        }

        public static AbilityEffect GetEffect(Ability entity, int index = 0)
        {
            return entity.AbilityEffects[index];
        }

        public static AbilityTrigger GetTrigger(Ability entity, int index = 0)
        {
            return entity.AbilityTriggers[index];
        }

        /// <summary>
        /// �������������ܡ�������buff�ȶ�ͨ������ӿڹ���
        /// </summary>
        public static Ability Attach(CombatEntity entity, object configObject)
        {
            var component = entity.GetComponent<AbilityComponent>();
            var ability = entity.AddChild<Ability>(x => x.ConfigObject = configObject as AbilityConfigObject);
            ability.AddComponent<AbilityLevelComponent>();
            ability.Init();
            component.IdAbilities.Add(ability.Id, ability);
            return ability;
        }

        public static void Remove(CombatEntity entity, Ability ability)
        {
            var component = entity.GetComponent<AbilityComponent>();
            component.IdAbilities.Remove(ability.Id);
            EndAbility(ability);
        }

        public static void LoadExecution(Ability entity)
        {
            entity.ExecutionObject = AssetUtils.LoadObject<ExecutionObject>($"{AbilityManagerObject.ExecutionResFolder}/Execution_{entity.ConfigObject.Id}");
            if (entity.ExecutionObject == null)
            {
                return;
            }
        }

        public static void TryActivateAbility(Ability entity)
        {
            ActivateAbility(entity);
        }

        public static void ActivateAbility(Ability entity)
        {
            entity.Enable = true;
            //entity.GetComponent<AbilityEffectComponent>().Enable = true;
            //entity.GetComponent<AbilityTriggerComponent>().Enable = true;
        }

        public static void DeactivateAbility(Ability entity)
        {
            entity.Enable = false;
            //entity.GetComponent<AbilityEffectComponent>().Enable = false;
            //entity.GetComponent<AbilityTriggerComponent>().Enable = false;
        }

        public static void EndAbility(Ability entity)
        {
            DeactivateAbility(entity);
            EcsObject.Destroy(entity);
        }
    }
}