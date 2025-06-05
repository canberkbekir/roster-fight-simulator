using Creatures.Chickens.Base;

namespace Creatures.Chickens.Chicks
{
    public class Chick : Chicken
    {  
        public Chick() : base()
        { 
        }

        public Chick(ChickenEntity entity) : base(entity)
        {
            Gender = ChickenGender.Female;
        }
        
    }  
}