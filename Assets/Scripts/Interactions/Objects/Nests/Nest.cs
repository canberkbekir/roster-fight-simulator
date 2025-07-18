using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Creatures.Chickens.Eggs.Components;
using Creatures.Chickens.Hens.Components;

namespace Interactions.Objects.Nests
{
    public class SyncListEgg : SyncList<EggEntity> {}

    public class Nest : NetworkBehaviour
    {
        #region Spawn Settings
        [SerializeField] private Transform spawnTransform;
        [SerializeField] private int maxEggCount = 5;
        #endregion

        #region SyncVars
        [SyncVar(hook = nameof(OnHenChanged))]       private HenEntity _currentHen;
        [SyncVar(hook = nameof(OnIncubatingChanged))] private bool      _isIncubating;
        #endregion

        #region Egg Collection
        public readonly SyncListEgg eggs = new SyncListEgg();
        #endregion

        #region Events
        public event Action<Nest> OnEggHatched;
        #endregion

        #region Public Properties
        public HenEntity             CurrentHen         => _currentHen;
        public bool                  IsOccupied         => _currentHen != null || eggs.Count > 0;
        public IEnumerable<EggEntity> CurrentEggEntities => eggs;
        public Transform             SpawnTransform     => spawnTransform;
        public int                  MaxEggCount        => maxEggCount;
        #endregion

        #region Server Methods
        [Server]
        public void Assign(uint henNetId)
        {
            if (_currentHen != null) return;
            if (!NetworkServer.spawned.TryGetValue(henNetId, out var obj)) return;
            if (!obj.TryGetComponent(out HenEntity hen)) return;
            _currentHen = hen;
        }

        [Server]
        public bool AssignEgg(uint eggNetId)
        {
            if (eggs.Count >= maxEggCount) return false;
            if (!NetworkServer.spawned.TryGetValue(eggNetId, out var obj)) return false;
            if (!obj.TryGetComponent(out EggEntity egg)) return false;

            eggs.Add(egg);
            obj.transform.position = spawnTransform.position;
            egg.OnHatched += HandleEggHatched;
            return true;
        }

        [Server]
        public void SetIncubating(bool incubate) => _isIncubating = incubate;

        [Server]
        public void ClearNest()
        {
            foreach (var egg in eggs)
                egg.OnHatched -= HandleEggHatched;

            eggs.Clear();
            _currentHen    = null;
            _isIncubating = false;
        }
        #endregion

        #region SyncVar Hooks
        private void OnHenChanged(HenEntity oldHen, HenEntity newHen)
        {
            _currentHen = newHen;
        }

        private void OnIncubatingChanged(bool oldValue, bool incubating)
        {
            foreach (var egg in eggs.Where(e => e.IsFertilized))
            {
                if (incubating) egg.StartIncubation();
                else               egg.StopIncubation();
            }
        }
        #endregion

        #region Internal Handlers
        private void HandleEggHatched(EggEntity egg)
        {
            egg.OnHatched -= HandleEggHatched;
            eggs.Remove(egg);
            OnEggHatched?.Invoke(this);
        }
        #endregion

        #region Editor Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (TryGetComponent<Collider>(out var col))
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                switch (col)
                {
                    case BoxCollider box:
                        Gizmos.DrawWireCube(box.center, box.size);
                        break;
                    case SphereCollider sph:
                        Gizmos.DrawWireSphere(sph.center, sph.radius);
                        break;
                    default:
                        Gizmos.DrawWireSphere(transform.position, 1f);
                        break;
                }
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
        }
        #endregion
    }
}
