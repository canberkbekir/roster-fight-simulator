using InventorySystem.Base;
using UnityEngine;

namespace Managers
{
    public class ContainerManager : MonoBehaviour
    {
      [SerializeField] private ItemDataContainer itemDataContainer;
      
        public ItemDataContainer ItemDataContainer => itemDataContainer;
    }
}
