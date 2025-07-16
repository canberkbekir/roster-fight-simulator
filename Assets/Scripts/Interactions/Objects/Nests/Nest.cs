using System;
using Creatures.Chickens.Eggs;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components;
using Mirror;
using UnityEngine;

namespace Interactions.Objects.Nests
{ 
    public class Nest : NetworkBehaviour
    {
        [SerializeField] private Transform spawnTransform;

        [SyncVar] private uint _occupiedChickenNetId = 0;
        [SyncVar] private uint _occupiedEggNetId = 0;

        private HenEntity _currentHen;
        private EggEntity _currentEggEntity;

        public Transform SpawnTransform => spawnTransform;
        public HenEntity CurrentHen => _currentHen;
        public EggEntity CurrentEggEntity => _currentEggEntity; 
        public bool IsOccupied => _occupiedChickenNetId != 0 || _occupiedEggNetId != 0;
        
        public event Action<Nest> OnEggHatched;  // <<< **YENİ**
        
        [Server]
        public void Assign(uint henNetId)
        {
            if (henNetId == 0 || _occupiedChickenNetId != 0)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: " +
                               (henNetId == 0 ? "invalid chickenNetId=0" : $"nest already has chicken {_occupiedChickenNetId}"));
                return;
            }

            if (!NetworkServer.spawned.TryGetValue(henNetId, out var chObj) || !chObj.TryGetComponent(out HenEntity henEnt))
            {
                Debug.LogError($"[Nest:{name}] Assign failed: object {henNetId} is not a valid HenEntity");
                return;
            }

            _occupiedChickenNetId = henNetId;
            _currentHen = henEnt; 
        }  
        [Server]
        public void AssignEgg(uint eggNetId)
        {
            if (eggNetId == 0)
            {
                Debug.LogError($"[Nest:{name}] AssignEgg failed: invalid eggNetId=0");
                return;
            }

            if (_occupiedEggNetId != 0)
            {
                Debug.LogError($"[Nest:{name}] AssignEgg failed: nest already has egg {_occupiedEggNetId}");
                return;
            }

            if (!NetworkServer.spawned.TryGetValue(eggNetId, out var eggObj))
            {
                Debug.LogError($"[Nest:{name}] AssignEgg failed: egg not found for netId={eggNetId}");
                return;
            }
            var eggComp = eggObj.GetComponent<EggEntity>();
            if (!eggComp)
            {
                Debug.LogError($"[Nest:{name}] AssignEgg failed: object {eggNetId} has no Egg component");
                return;
            }

            _occupiedEggNetId = eggNetId;
            _currentEggEntity = eggComp;
 
            eggObj.transform.position = spawnTransform.position; 
            eggComp.OnHatched += HandleEggHatched;  
        }
 
        [Server]
        private void HandleEggHatched(EggEntity eggEntity)
        { 
            if (_currentEggEntity)
            {
                _currentEggEntity.OnHatched -= HandleEggHatched;
            }
 
            _occupiedEggNetId = 0;
            _currentEggEntity = null;
 
            if (_occupiedChickenNetId != 0 &&
                NetworkServer.spawned.TryGetValue(_occupiedChickenNetId, out var chObj))
            {
                var chickenEnt = chObj.GetComponent<HenEntity>();
                chickenEnt?.HenNestHandler.UnassignNest();
            }
            _occupiedChickenNetId = 0;
            _currentHen = null; 
 
            OnEggHatched?.Invoke(this);
        } 
        [Server]
        public void ClearNest()
        { 
            if (_currentEggEntity)
            {
                _currentEggEntity.OnHatched -= HandleEggHatched;
            }

            _occupiedEggNetId = 0;
            _occupiedChickenNetId = 0;
            _currentEggEntity = null;
            _currentHen = null;
        }

        #region Gizmo Visualization
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (TryGetComponent<Collider>(out var col))
            {
                switch (col)
                {
                    case BoxCollider box:
                        Gizmos.matrix = transform.localToWorldMatrix;
                        Gizmos.DrawWireCube(box.center, box.size);
                        break;
                    case SphereCollider sph:
                        Gizmos.matrix = transform.localToWorldMatrix;
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
