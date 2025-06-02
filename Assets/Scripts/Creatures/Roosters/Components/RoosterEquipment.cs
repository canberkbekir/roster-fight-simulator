using Mirror;

namespace Creatures.Roosters.Components
{
    public class RoosterEquipment: NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity _owner;

        public void Init(RoosterEntity entity)
        {
            _owner = entity;
        }

     
    }
}