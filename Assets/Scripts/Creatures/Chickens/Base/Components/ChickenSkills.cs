using Mirror;

namespace Creatures.Chickens.Base.Components
{
    public class ChickenSkills: NetworkBehaviour, IChickenComponent
    {
        private ChickenEntity _owner;

        public void Init(ChickenEntity entity)
        {
            _owner = entity;
            
        }
    }
}