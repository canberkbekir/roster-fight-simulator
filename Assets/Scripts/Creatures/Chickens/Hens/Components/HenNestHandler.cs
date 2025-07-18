using Creatures.Chickens.Base;
using Creatures.Chickens.Base.Components;
using Interactions.Objects.Nests;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Hens.Components
{
    public class HenNestHandler : ChickenComponentBase
    {
        [SyncVar(hook = nameof(OnNestIdChanged))]
        private uint _assignedNestNetId;

        public Nest AssignedNest { get; private set; }

        public override void Init(ChickenEntity owner)
        {
            base.Init(owner);
        }

        [Server]
        public void AssignHenToNest(Nest nest)
        {
            if (nest == null || nest.IsOccupied) return;
            nest.Assign(Owner.netId);
            _assignedNestNetId = nest.netId;
        }

        [Server]
        public void UnassignNest()
        {
            if (_assignedNestNetId == 0) return;
            if (NetworkServer.spawned.TryGetValue(_assignedNestNetId, out var ni) &&
                ni.TryGetComponent<Nest>(out var nest))
            {
                nest.ClearNest();
            }
            _assignedNestNetId = 0;
        }

        [Server]
        public void Incubate()
        {
            if (AssignedNest == null)
            {
                Debug.LogError("[HenNestHandler] Cannot incubate: No assigned nest.");
                return;
            }
            
            AssignedNest.SetIncubating(true);
        }
        
        [Server]
        public void StopIncubating()
        {
            if (AssignedNest == null)
            {
                Debug.LogError("[HenNestHandler] Cannot stop incubating: No assigned nest.");
                return;
            }
            
            AssignedNest.SetIncubating(false);
        }

        private void OnNestIdChanged(uint oldId, uint newId)
        {
            if (NetworkClient.spawned.TryGetValue(newId, out var ni) &&
                ni.TryGetComponent<Nest>(out var nest))
            {
                AssignedNest = nest;
            }
            else
            {
                AssignedNest = null;
            }
            
        }
    }

}