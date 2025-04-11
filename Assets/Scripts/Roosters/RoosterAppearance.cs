using Mirror;

namespace Roosters
{
    public class RoosterAppearance: NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity owner;

        public void Init(RoosterEntity entity)
        {
            owner = entity;
            
        }
    }
}