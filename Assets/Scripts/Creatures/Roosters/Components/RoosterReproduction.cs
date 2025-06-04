// RoosterReproduction.cs
using System;
using AI.Base;
using AI.Roosters;
using Creatures.Eggs;
using Creatures.Roosters.Components;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using UnityEngine;

namespace Creatures.Roosters.Components
{
    /// <summary>
    /// Handles everything related to pregnancy, nest assignment, and incubation/sitting on eggs.
    /// </summary>
    public class RoosterReproduction : NetworkBehaviour, IRoosterComponent
    {
        [SyncVar] private uint _currentNestNetId = 0;
        [SyncVar] private uint _pregnantByNetId;

        private RoosterEntity _owner;

        /// <summary>
        /// Call from RoosterEntity.Init(...) so we can wire this up.
        /// </summary>
        public void Init(RoosterEntity owner)
        {
            _owner = owner;
        }

        [field: SyncVar]
        public bool IsPregnant { get; private set; } = false;

        public Nest CurrentNest
        {
            get
            {
                if (_currentNestNetId == 0)
                {
                    Debug.Log($"[RoosterReproduction:{name}] CurrentNest: null (no nest assigned)");
                    return null;
                }

                if (NetworkServer.spawned.TryGetValue(_currentNestNetId, out var nestObj))
                {
                    var nest = nestObj.GetComponent<Nest>();
                    Debug.Log($"[RoosterReproduction:{name}] CurrentNest: {nest?.name}");
                    return nest;
                }

                Debug.Log($"[RoosterReproduction:{name}] CurrentNest: null (nest not found in spawned objects)");
                return null;
            }
        }

        public RoosterEntity PregnantBy
        {
            get
            {
                if (_pregnantByNetId == 0)
                {
                    Debug.Log($"[RoosterReproduction:{name}] PregnantBy: null (no rooster assigned)");
                    return null;
                }

                if (NetworkServer.spawned.TryGetValue(_pregnantByNetId, out var roosterObj))
                {
                    var rooster = roosterObj.GetComponent<RoosterEntity>();
                    Debug.Log($"[RoosterReproduction:{name}] PregnantBy: {rooster?.name}");
                    return rooster;
                }

                Debug.Log($"[RoosterReproduction:{name}] PregnantBy: null (rooster not found in spawned objects)");
                return null;
            }
        }

        #region Server‐Only Methods

        [Server]
        public void MarkPregnant(uint fatherNetId)
        {
            if (IsPregnant) return;
            IsPregnant = true; 
            _pregnantByNetId = fatherNetId;
            _currentNestNetId = 0;  
        }

        [Server]
        public void UnmarkPregnant()
        {
            if (!IsPregnant) return;
            IsPregnant = false;
        }

        [Server]
        public void AssignNest(uint nestNetId)
        {
            _currentNestNetId = nestNetId;
        }
        
        [Server]
        public void UnassignNest()
        {
            if (_currentNestNetId == 0)
            {
                Debug.LogError($"[RoosterReproduction:{name}] UnassignNest failed: no current nest assigned.");
                return;
            }

            _currentNestNetId = 0;
        }

        [Server]
        public void ClearNestReferences()
        {
            IsPregnant = false;
            _currentNestNetId = 0;
        }

        [Server]
        public void SitOnEgg()
        {
            var nest = CurrentNest;
            if (!nest)
            {
                Debug.LogError($"[RoosterReproduction:{name}] SitOnEgg failed: no current nest assigned.");
                return;
            }

            if (nest.CurrentChicken != _owner)
            {
                Debug.LogError($"[RoosterReproduction:{name}] SitOnEgg failed: current nest is occupied by another chicken.");
                return;
            }

            if (!nest.CurrentEgg)
            {
                Debug.LogError($"[RoosterReproduction:{name}] SitOnEgg failed: no egg in the nest.");
                return;
            }

            if (!IsPregnant) return;
            Debug.LogError($"[RoosterReproduction:{name}] SitOnEgg failed: already pregnant.");
        }

        public override void OnStartServer()
        {
            base.OnStartServer(); 
            if (_currentNestNetId == 0 || IsPregnant) return;
            if (!NetworkServer.spawned.TryGetValue(_currentNestNetId, out var nestObj)) return;
            var nest = nestObj.GetComponent<Nest>();
            if (!nest || !nest.CurrentEgg) return;
            var ai = _owner.GetComponent<ChickenAI>();
            if (ai)
                ai.ForceSetNestAndIncubate(nest);
            else
                SitOnEgg();
        }

        #endregion 
        [Server]
        public bool TryBreedWith(RoosterReproduction otherRepro)
        {
            if (!otherRepro) return false;
            if (otherRepro == this) return false;  
 
            var selfGender = _owner.Gender;
            var otherGender = otherRepro._owner.Gender;
            if (selfGender == otherGender) return false;

            RoosterReproduction maleSide, femaleSide;
            switch (selfGender)
            {
                case RoosterGender.Male when otherGender == RoosterGender.Female:
                    maleSide = this;
                    femaleSide = otherRepro;
                    break;
                case RoosterGender.Female when otherGender == RoosterGender.Male:
                    maleSide = otherRepro;
                    femaleSide = this;
                    break;
                default:
                    return false;
            }
 
            if (femaleSide.IsPregnant) return false; 
 
            femaleSide.MarkPregnant(maleSide._owner.netId);
 
            maleSide.ForceResetToWander();
 
            femaleSide.ForcePregnantSeekNest();

            Debug.Log($"[RoosterReproduction] {maleSide._owner.name} bred with {femaleSide._owner.name}");
            return true;
        }

        #region Utility Methods for AI

        [Server]
        public void ForceResetToWander()
        {
            var ai = _owner.GetComponent<RoosterAI>();
            if (ai != null)
                ai.ForceToWander();
        }

        [Server]
        public void ForcePregnantSeekNest()
        {
            var ai = _owner.GetComponent<ChickenAI>();
            if (ai != null)
                ai.ForceSetPregnantSeekNest();
        }

        #endregion
    }
}
