using Creatures.Chickens.Base;
using Creatures.Chickens.Roosters;
using Interactions.Objects.Nests;
using Mirror; 

namespace Creatures.Chickens.Hens.Components
{
    public class HenEntity : ChickenEntity
    {    
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
            Chicken.Gender = ChickenGender.Female; 
            
        }
    }
}
