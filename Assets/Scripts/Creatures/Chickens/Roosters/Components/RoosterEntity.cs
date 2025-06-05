using Creatures.Chickens.Base;  

namespace Creatures.Chickens.Roosters.Components
{
    public class RoosterEntity : ChickenEntity
    { 
        public new Rooster Chicken { get; set; }   
        public override void Init(Chicken chicken)
        {
            base.Init(chicken);
        }
        
    }
}
