using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;

namespace MyAttackSpeedMod
{
    public class AttackSpeedAndBurstFireBehavior : MissionBehavior
    {
        private Queue<float> _arrowFiringQueue = new Queue<float>();

        protected override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Agent.Main != null && Agent.Main.IsHumanControlled())
            {
                var mountAndBladeAgent = Agent.Main;
                var weaponComponent = mountAndBladeAgent.Equipment[EquipmentIndex.Weapon];
                if (weaponComponent.Item?.WeaponClass == WeaponClass.Arrows)
                {
                    // Adjust attack speed
                    mountAndBladeAgent.SetAttackSpeedMultiplier(MyAttackSpeedModSubModule.GetAttackSpeedMultiplier());

                    // Implement burst fire logic
                    if (MyAttackSpeedModSubModule.IsBurstFireEnabled() && mountAndBladeAgent.HasWeaponInSlot(EquipmentIndex.Weapon) && mountAndBladeAgent.GetCurrentActionId() == AttackCollisionData.ActionIndex.Ranged)
                    {
                        var currentFrame = Mission.Current.Frame;

                        // Schedule three arrows to be fired in sequence
                        if (_arrowFiringQueue.Count == 0)
                        {
                            _arrowFiringQueue.Enqueue(currentFrame + 10);   // First arrow at frame + 10
                            _arrowFiringQueue.Enqueue(currentFrame + 20);   // Second arrow at frame + 20
                            _arrowFiringQueue.Enqueue(currentFrame + 30);   // Third arrow at frame + 30
                        }

                        while (_arrowFiringQueue.Count > 0 && _arrowFiringQueue.Peek() <= currentFrame)
                        {
                            mountAndBladeAgent.SetCurrentAction(new AgentRangedAttackAction(mountAndBladeAgent, mountAndBladeAgent.GetCurrentTarget(), _arrowFiringQueue.Dequeue()));
                        }
                    }
                    else
                    {
                        // Clear the queue if burst fire is disabled or not in ranged attack
                        _arrowFiringQueue.Clear();
                    }
                }
            }
        }

        protected override void OnMissionTickLate(float dt)
        {
            base.OnMissionTickLate(dt);
        }
    }
}
