using Creatures.Eggs;
using Creatures.Roosters.Components;
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

        private RoosterEntity _currentChicken;
        private Egg _currentEgg;

        public Transform SpawnTransform => spawnTransform;
        public RoosterEntity CurrentChicken => _currentChicken;
        public Egg CurrentEgg => _currentEgg; 

        /// <summary>
        /// Server-only: assign both chicken and egg to this nest.
        /// </summary>
        [Server]
        public void Assign(uint chickenNetId)
        {
            if (chickenNetId == 0)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: invalid chickenNetId=0");
                return;
            }

            if (_occupiedChickenNetId != 0)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: nest already has chicken {_occupiedChickenNetId}");
                return;
            }

           
            if (!NetworkServer.spawned.TryGetValue(chickenNetId, out var chObj))
            {
                Debug.LogError($"[Nest:{name}] Assign failed: chicken not found for netId={chickenNetId}");
                return;
            }
            var chickenEnt = chObj.GetComponent<RoosterEntity>();
            if (chickenEnt == null)
            {
                Debug.LogError($"[Nest:{name}] Assign failed: object {chickenNetId} has no RoosterEntity");
                return;
            }

            _occupiedChickenNetId = chickenNetId;

            _currentChicken = chickenEnt;

            chickenEnt.AssignNest(netId);
        }

        /// <summary>
        /// Server-only: remove the egg (e.g. after hatching).
        /// Clears egg reference and notifies the chicken to clear its nest.
        /// </summary>
        [Server]
        public void RemoveEgg()
        {
            if (_occupiedEggNetId == 0)
                return;

            _occupiedEggNetId = 0;
            _currentEgg       = null;

            if (_occupiedChickenNetId != 0 &&
                NetworkServer.spawned.TryGetValue(_occupiedChickenNetId, out var chObj))
            {
                var chickenEnt = chObj.GetComponent<RoosterEntity>();
                chickenEnt?.ClearNest();
            }
        }

        /// <summary>
        /// Server-only: clear both chicken and egg from the nest (e.g., if chicken leaves).
        /// </summary>
        [Server]
        public void ClearNest()
        {
            _occupiedEggNetId     = 0;
            _occupiedChickenNetId = 0;
            _currentEgg           = null;
            _currentChicken       = null;
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
