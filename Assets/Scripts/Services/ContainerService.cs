using Creatures.Genes.Base.ScriptableObjects;
using InventorySystem.Base;
using Mirror;
using UnityEngine;

namespace Services
{
    public class ContainerService : NetworkBehaviour
    {
        [SerializeField] private ItemDataContainer itemDataContainer;
        [SerializeField] private GeneDataContainer geneDataContainer;
      
        public ItemDataContainer ItemDataContainer => itemDataContainer;
        public GeneDataContainer GeneDataContainer => geneDataContainer;
    }
}
