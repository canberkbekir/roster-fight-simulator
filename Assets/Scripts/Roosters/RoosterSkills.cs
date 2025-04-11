using Mirror;

namespace Roosters
{
    public class RoosterSkills: NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity owner;

        public void Init(RoosterEntity entity)
        {
            owner = entity;
            
        }
    }
}