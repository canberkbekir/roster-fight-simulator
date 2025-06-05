using Creatures.Chickens.Base;  

namespace Creatures.Chickens.Roosters.Components
{
    public class RoosterEntity : ChickenEntity
    {  
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
            Chicken.Gender = ChickenGender.Male;
        }
        
    }
}
