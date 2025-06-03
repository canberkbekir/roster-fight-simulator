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
        [SyncVar] private bool _isPregnant = false;
        [SyncVar] private uint _currentNestNetId = 0;

        private RoosterEntity _owner;

        /// <summary>
        /// Call from RoosterEntity.Init(...) so we can wire this up.
        /// </summary>
        public void Init(RoosterEntity owner)
        {
            _owner = owner;
        }

        public bool IsPregnant => _isPregnant;
        public Nest CurrentNest
        {
            get
            {
                if (_currentNestNetId == 0) return null;
                if (NetworkServer.spawned.TryGetValue(_currentNestNetId, out var nestObj))
                    return nestObj.GetComponent<Nest>();
                return null;
            }
        }

        #region Server‐Only Methods

        [Server]
        public void MarkPregnant(uint fatherNetId)
        {
            if (_isPregnant) return;
            _isPregnant = true;
            // Optionally store fatherNetId if you need it elsewhere
        }

        [Server]
        public void UnmarkPregnant()
        {
            if (!_isPregnant) return;
            _isPregnant = false;
        }

        [Server]
        public void AssignNest(uint nestNetId)
        {
            _currentNestNetId = nestNetId;
        }

        [Server]
        public void ClearNest()
        {
            _isPregnant = false;
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

            if (_isPregnant)
            {
                Debug.LogError($"[RoosterReproduction:{name}] SitOnEgg failed: already pregnant.");
                return;
            }

            // Trigger any sit animation or VFX here
            Debug.Log($"[RoosterReproduction:{name}] Sitting on egg at nest {nest.name}");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // If we still have a nest assigned and aren’t marked pregnant, try to re‐attach
            if (_currentNestNetId == 0 || _isPregnant) return;
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

        // ********** Aşağıya yeni eklenecek kısım **********

        /// <summary>
        /// İki tavuk (bu ve partner) bir araya geldiğinde çağrılır. 
        /// Eğer şartlar uygunsa hem erkek hem dişi tarafın AI state'lerini günceller
        /// ve dişiyi hamile bırakır. 
        /// </summary>
        [Server]
        public bool TryBreedWith(RoosterReproduction otherRepro)
        {
            if (otherRepro == null) return false;
            if (otherRepro == this) return false; // kendisiyle eşlenemez

            // Ensure one is male, the other female
            var selfGender = _owner.Gender;
            var otherGender = otherRepro._owner.Gender;
            if (selfGender == otherGender) return false;

            RoosterReproduction maleSide, femaleSide;
            if (selfGender == RoosterGender.Male && otherGender == RoosterGender.Female)
            {
                maleSide = this;
                femaleSide = otherRepro;
            }
            else if (selfGender == RoosterGender.Female && otherGender == RoosterGender.Male)
            {
                maleSide = otherRepro;
                femaleSide = this;
            }
            else
            {
                return false;
            }

            // If female already pregnant, abort
            if (femaleSide._isPregnant) return false;

            // Optional distance check (RoosterAI likely already did this)
            float dist = Vector3.Distance(maleSide._owner.transform.position, femaleSide._owner.transform.position);
            const float breedingDistance = 2f;
            if (dist > breedingDistance) return false;
 
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
