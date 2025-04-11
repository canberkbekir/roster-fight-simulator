using Mirror;

namespace Roosters
{
    public class RoosterGenome : NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity owner;

        public void Init(RoosterEntity entity)
        {
            owner = entity;
        }

     
    }

}