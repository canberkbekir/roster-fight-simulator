using Mirror;

namespace Roosters
{
    public class RoosterEquipment: NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity owner;

        public void Init(RoosterEntity entity)
        {
            owner = entity;
        }

     
    }
}