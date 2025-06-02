using Creatures.Genes.Base.ScriptableObjects;
using InventorySystem.Base;
using UnityEngine;

namespace Managers
{
    public class ContainerManager : MonoBehaviour
    {
        [SerializeField] private ItemDataContainer itemDataContainer;
        [SerializeField] private GeneDataContainer geneDataContainer;
      
        public ItemDataContainer ItemDataContainer => itemDataContainer;
        public GeneDataContainer GeneDataContainer => geneDataContainer;
    }
}
