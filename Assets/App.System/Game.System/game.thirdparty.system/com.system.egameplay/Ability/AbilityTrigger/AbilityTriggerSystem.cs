using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtils;
using ECS;
using EGamePlay.Combat;
using System;
using ET;

namespace EGamePlay
{
    public class AbilityTriggerSystem : AEntitySystem<AbilityTrigger>,
        IAwake<AbilityTrigger>,
        IEnable<AbilityTrigger>,
        IDisable<AbilityTrigger>,
        IUpdate<AbilityTrigger>
    {
        public void Awake(AbilityTrigger entity)
        {

        }

        public void Update(AbilityTrigger entity)
        {
            if (entity.GetComponent<TimeStateObserveComponent>() is { Enable: true } component)
            {
                TimeStateObserveSystem.Update(entity, component);
            }
        }

        public void Enable(AbilityTrigger entity)
        {
            /// ��������
            if (entity.TriggerConfig.TriggerType == EffectTriggerType.AutoTrigger)
            {
                /// ��������ʱ����
                if (entity.TriggerConfig.AutoTriggerType == EffectAutoTriggerType.Instant)
                {
                    OnTrigger(entity, new TriggerContext() { Target = entity.ParentEntity });
                }
                /// ���ж����¼�����
                if (entity.TriggerConfig.AutoTriggerType == EffectAutoTriggerType.Action)
                {
                    entity.AddComponent<BehaviourObserveComponent>();
                }
                /// ����ʱ״̬�¼�����
                if (entity.TriggerConfig.AutoTriggerType == EffectAutoTriggerType.Condition)
                {
                    var conditionType = entity.TriggerConfig.ConditionType;
                    var paramObj = entity.ConditionParamValue;
                    if (float.TryParse((string)paramObj, out var time))
                    {
                        var condition = entity.AddComponent<TimeStateObserveComponent>(x => x.Timer = new GameTimer(time));
                        TimeStateObserveSystem.StartListen(entity, conditionType);
                    }
                }
            }
        }

        public void Disable(AbilityTrigger entity)
        {
        }

        public static AbilityTrigger Create(Ability ability, TriggerConfig triggerConfig)
        {
            var abilityTrigger = ability.AddChild<AbilityTrigger>(x => x.TriggerConfig = triggerConfig);

            if (triggerConfig.StateCheckList != null && triggerConfig.StateCheckList.Count > 0)
            {
                abilityTrigger.AddComponent<TriggerStateCheckComponent>();
            }
            return abilityTrigger;
        }

        public static void OnTrigger(AbilityTrigger entity, TriggerContext context)
        {
            var newContext = context;
            newContext.AbilityTrigger = entity;
            context = newContext;
            var abilityTrigger = entity;

            var source = context.TriggerSource;
            var target = context.Target;
            if (target == null && source != null)
            {
                target = source;
            }
            if (target == null)
            {
                target = entity.ParentEntity;
            }

            var stateCheckResult = true;

            /// ������״̬�жϣ�״̬�ж����ж�Ŀ���״̬�Ƿ�����������������ܴ���Ч��
            if (abilityTrigger.GetComponent<TriggerStateCheckComponent>() is { } component)
            {
                stateCheckResult = TriggerStateCheckSystem.CheckTargetState(abilityTrigger, target);
            }

            /// ���������򴥷�Ч��
            if (stateCheckResult)
            {
                foreach (var item in entity.TriggerConfig.TriggerEffects)
                {
                    var effects = entity.OwnerAbility.AbilityEffects;
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (i == (int)item.EffectApplyType - 1 || item.EffectApplyType == EffectApplyType.AllEffects)
                        {
                            var abilityEffect = effects[i];

                            if (entity.OwnerEntity.EffectAssignAbility.TryMakeAction(out var effectAssign))
                            {
                                effectAssign.AbilityEffect = abilityEffect;
                                effectAssign.AssignTarget = target;
                                effectAssign.SourceAbility = entity.OwnerAbility;
                                effectAssign.TriggerContext = context;
                                AssignActionSystem.Execute(effectAssign);
                            }
                        }
                    }
                }
            }
        }
    }
}