using Creatures.Chickens.Base;
using Creatures.Chickens.Base.Components;
using Interactions.Objects.Nests;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Hens.Components
{
    public class NestHandler : ChickenComponentBase
    {
        [Header("Settings")]
        [SerializeField] private uint assignedNestUid;
        
        public uint AssignedNest => assignedNestUid;
        public override void Init(ChickenEntity owner)
        {
            base.Init(owner); 
            
        }
        
        [Server]
        public void AssignHenToNest(Nest nest)
        {
            if (nest == null || nest.IsOccupied)
            {
                Debug.LogError("Cannot assign nest: either null or already occupied.");
                return;
            }
            
            nest.Assign(Owner.netId);
            assignedNestUid = nest.netId;
        }  
         
        [Server]
        public void UnassignNest()
        {
            if (assignedNestUid == 0)
            {
                Debug.LogError("No nest assigned to remove.");
                return;
            }
            
            if (!NetworkServer.spawned.TryGetValue(assignedNestUid, out var nestObj) || !nestObj.TryGetComponent(out Nest nest))
            {
                Debug.LogError($"Nest with netId {assignedNestUid} not found or invalid.");
                return;
            }
            
            nest.ClearNest();
            assignedNestUid = 0; 
        }
        
    }
}