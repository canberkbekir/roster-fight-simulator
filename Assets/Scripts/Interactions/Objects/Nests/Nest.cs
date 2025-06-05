using System;
using Creatures.Chickens.Eggs;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components;
using Mirror;
using UnityEngine;

namespace Interactions.Objects.Nests
{
    /// <summary>
    /// Placed on any “nest” GameObject.  
    /// Tracks which chicken has claimed it and which egg is inside.  
    /// Provides explicit methods to assign or remove both chicken and egg.
    /// </summary>
    public class Nest : NetworkBehaviour
    {
        [SerializeField] private Transform spawnTransform;

        [SyncVar] private uint _occupiedChickenNetId = 0;
        [SyncVar] private uint _occupiedEggNetId = 0;

        private HenEntity _currentHen;
        private Egg _currentEgg;

        public Transform SpawnTransform => spawnTransform;
        public HenEntity CurrentHen => _currentHen;
        public Egg CurrentEgg => _currentEgg; 
        public bool IsOccupied => _occupiedChickenNetId != 0 || _occupiedEggNetId != 0;

        /// <summary>
        /// Yumurtanın kuluçka süresi dolup hatch olduğunda
        /// diğer sistemlerin (örn. ChickenAI) da haberdar olması için.
        /// </summary>
        public event Action<Nest> OnEggHatched;  // <<< **YENİ**

        /// <summary>
        /// Server-only: assign both chicken and egg to this nest.
        /// (Yalnızca tavuğu atmak için kullanılıyordu; şimdi egg atma da ekleniyor.)
        /// </summary>
        [Server]
        public void Assign(uint henNetId)
        {
            if (henNetId == 0)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: invalid chickenNetId=0");
                return;
            }

            if (_occupiedChickenNetId != 0)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: nest already has chicken {_occupiedChickenNetId}");
                return;
            }

            if (!NetworkServer.spawned.TryGetValue(henNetId, out var chObj))
            {
                Debug.LogError($"[Nest:{name}] Assign failed: chicken not found for netId={henNetId}");
                return;
            }
            var henEnt = chObj.GetComponent<HenEntity>();
            if (!henEnt)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: object {henNetId} has no RoosterEntity");
                return;
            }

            _occupiedChickenNetId = henNetId;
            _currentHen = henEnt;
            henEnt.Reproduction.AssignNest(netId);
        }

        /// <summary>
        /// Server-only: assign edilmiş bir egg objesini bu yuva üzerine yerleştirir.
        /// </summary>
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
            var eggComp = eggObj.GetComponent<Egg>();
            if (!eggComp)
            {
                Debug.LogError($"[Nest:{name}] AssignEgg failed: object {eggNetId} has no Egg component");
                return;
            }

            _occupiedEggNetId = eggNetId;
            _currentEgg = eggComp;
 
            eggObj.transform.position = spawnTransform.position; 
            eggComp.OnHatched += HandleEggHatched;  
        }
 
        [Server]
        private void HandleEggHatched(Egg egg)
        { 
            if (_currentEgg)
            {
                _currentEgg.OnHatched -= HandleEggHatched;
            }
 
            _occupiedEggNetId = 0;
            _currentEgg = null;
 
            if (_occupiedChickenNetId != 0 &&
                NetworkServer.spawned.TryGetValue(_occupiedChickenNetId, out var chObj))
            {
                var chickenEnt = chObj.GetComponent<RoosterEntity>();
                chickenEnt?.Reproduction.ClearNestReferences();
            }
            _occupiedChickenNetId = 0;
            _currentHen = null; 
 
            OnEggHatched?.Invoke(this);
        } 
        [Server]
        public void ClearNest()
        { 
            if (_currentEgg != null)
            {
                _currentEgg.OnHatched -= HandleEggHatched;
            }

            _occupiedEggNetId = 0;
            _occupiedChickenNetId = 0;
            _currentEgg = null;
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
