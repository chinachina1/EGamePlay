using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using EGamePlay.Combat;
using ECS;

namespace EGamePlay.Combat
{
    public class EffectAssignAbility : EcsEntity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }


        public bool TryMakeAction(out EffectAssignAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<EffectAssignAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// ����Ч���ж�
    /// </summary>
    public class EffectAssignAction : EcsEntity, IActionExecute
    {
        /// �������Ч�������ж���Դ����
        public EcsEntity SourceAbility { get; set; }
        /// Ŀ���ж�
        public IActionExecute TargetAction { get; set; }
        public AbilityEffect AbilityEffect { get; set; }
        public Effect EffectConfig => AbilityEffect.EffectConfig;
        /// �ж�����
        public EcsEntity ActionAbility { get; set; }
        /// Ч�������ж�Դ
        public EffectAssignAction SourceAssignAction { get; set; }
        /// �ж�ʵ��
        public CombatEntity Creator { get; set; }
        /// Ŀ�����
        public EcsEntity Target { get; set; }
        /// ����Ŀ��
        public EcsEntity AssignTarget { get; set; }
        /// ����������
        public TriggerContext TriggerContext { get; set; }
    }
}