﻿using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ArmyArrowCounter
{
    class ArrowCounter
    {
        public event Action<int> RemainingArrowsUpdateEvent;
        public event Action<int> MaxArrowsUpdateEvent;
        public int RemainingArrows { get; private set; }
        public int MaxArrows { get; private set; }

        private readonly AacMissionBehavior AacMissionBehavior;
        private readonly Dictionary<int, short> AgentHashCodeToCurrentArrows = new Dictionary<int, short>();

        public ArrowCounter(AacMissionBehavior aacMissionBehavior)
        {
            AacMissionBehavior = aacMissionBehavior;
            aacMissionBehavior.SiegeBattleStartEvent += OnSiegeBattleStart;
            aacMissionBehavior.HideoutBattleStartEvent += OnHideoutBattleStart;
            aacMissionBehavior.PlayerBuiltEvent += OnPlayerBuilt;
            aacMissionBehavior.AllyAgentBuiltEvent += OnAllyAgentBuilt;
            aacMissionBehavior.AllyAgentRemovedEvent += OnAllyAgentRemoved;
            aacMissionBehavior.AllyFiredMissileEvent += OnAllyFiredMissile;
            aacMissionBehavior.OnAllyPickedUpAmmoEvent += OnAllyPickedUpAmmo;
        }

        private void OnSiegeBattleStart()
        {
            CountAllAlliedAgents();
        }

        private void OnHideoutBattleStart()
        {
            CountAllAlliedAgents();
        }

        private void OnPlayerBuilt()
        {
            CountAllAlliedAgents();
        }

        private void OnAllyAgentBuilt(Agent agent)
        {
            AddAgent(agent);
        }

        private void OnAllyAgentRemoved(Agent agent)
        {
            RemoveAgent(agent);
        }

        private void OnAllyFiredMissile(Agent agent)
        {
            if (AgentHashCodeToCurrentArrows.ContainsKey(agent.GetHashCode()))
            {
                AgentHashCodeToCurrentArrows[agent.GetHashCode()]--;
            }
            AddToRemainingArrows(-1);
        }

        private void OnAllyPickedUpAmmo(Agent agent, SpawnedItemEntity item)
        {
            if (!AgentHashCodeToCurrentArrows.ContainsKey(agent.GetHashCode()))
            {
                return;
            }

            short lastKnownAmmoOnAgent = AgentHashCodeToCurrentArrows[agent.GetHashCode()];
            short newAmmoOnAgent = CalculateRemainingAmmo(agent);
            short amountPickedUp = (short) (newAmmoOnAgent - lastKnownAmmoOnAgent);
            AgentHashCodeToCurrentArrows[agent.GetHashCode()] += amountPickedUp;
            AddToRemainingArrows(amountPickedUp);
        }

        internal void CountAllAlliedAgents()
        {
            foreach (Agent agent in AacMissionBehavior.Mission.Agents)
            {
                if (Utils.IsPlayerAlly(agent, AacMissionBehavior.PlayerAgent))
                {
                    AddAgent(agent);
                }
            }
        }

        internal void AddToRemainingArrows(int deltaRemainingArrows)
        {
            RemainingArrows += deltaRemainingArrows;
            RemainingArrowsUpdateEvent?.Invoke(RemainingArrows);
        }

        internal void AddToMaxArrows(int deltaMaxArrows)
        {
            MaxArrows += deltaMaxArrows;
            MaxArrowsUpdateEvent?.Invoke(MaxArrows);
        }

        internal bool AddAgent(Agent agent)
        {
            short spawnAmmo = CalculateMaxAmmo(agent);
            if (!AgentHashCodeToCurrentArrows.ContainsKey(agent.GetHashCode()))
            {
                AgentHashCodeToCurrentArrows.Add(agent.GetHashCode(), spawnAmmo);
            }
            AddToRemainingArrows(spawnAmmo);
            AddToMaxArrows(spawnAmmo);
            return spawnAmmo != 0;
        }

        internal bool RemoveAgent(Agent agent)
        {
            short remainingAmmo = CalculateRemainingAmmo(agent);
            short maxAmmo = CalculateMaxAmmo(agent);
            AddToRemainingArrows(-remainingAmmo);
            AddToMaxArrows(-maxAmmo);
            return maxAmmo != 0;
        }

        private static short CalculateRemainingAmmo(Agent agent)
        {
            MissionWeapon weaponFromSlot0 = agent.Equipment[EquipmentIndex.Weapon0];
            short ammoFromSlot0 = weaponFromSlot0.Equals(MissionWeapon.Invalid) || weaponFromSlot0.IsShield() ? (short) 0 : weaponFromSlot0.Amount;
            MissionWeapon weaponFromSlot1 = agent.Equipment[EquipmentIndex.Weapon1];
            short ammoFromSlot1 = weaponFromSlot1.Equals(MissionWeapon.Invalid) || weaponFromSlot1.IsShield() ? (short) 0 : weaponFromSlot1.Amount;
            MissionWeapon weaponFromSlot2 = agent.Equipment[EquipmentIndex.Weapon2];
            short ammoFromSlot2 = weaponFromSlot2.Equals(MissionWeapon.Invalid) || weaponFromSlot2.IsShield() ? (short) 0 : weaponFromSlot2.Amount;
            MissionWeapon weaponFromSlot3 = agent.Equipment[EquipmentIndex.Weapon3];
            short ammoFromSlot3 = weaponFromSlot3.Equals(MissionWeapon.Invalid) || weaponFromSlot3.IsShield() ? (short) 0 : weaponFromSlot3.Amount;
            MissionWeapon weaponFromSlot4 = agent.Equipment[EquipmentIndex.Weapon4];
            short ammoFromSlot4 = weaponFromSlot4.Equals(MissionWeapon.Invalid) || weaponFromSlot4.IsShield() ? (short) 0 : weaponFromSlot4.Amount;

            return (short) (ammoFromSlot0 + ammoFromSlot1 + ammoFromSlot2 + ammoFromSlot3 + ammoFromSlot4);
        }

        private static short CalculateMaxAmmo(Agent agent)
        {
            short ammoFromSlot0 = agent.SpawnEquipment[EquipmentIndex.Weapon0].Ammo;
            short ammoFromSlot1 = agent.SpawnEquipment[EquipmentIndex.Weapon1].Ammo;
            short ammoFromSlot2 = agent.SpawnEquipment[EquipmentIndex.Weapon2].Ammo;
            short ammoFromSlot3 = agent.SpawnEquipment[EquipmentIndex.Weapon3].Ammo;
            short ammoFromSlot4 = agent.SpawnEquipment[EquipmentIndex.Weapon4].Ammo;

            return (short) (ammoFromSlot0 + ammoFromSlot1 + ammoFromSlot2 + ammoFromSlot3 + ammoFromSlot4);
        }
    }
}
