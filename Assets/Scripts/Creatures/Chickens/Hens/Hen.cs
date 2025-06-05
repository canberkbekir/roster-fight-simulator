using Creatures.Chickens.Base;

namespace Creatures.Chickens.Hens
{
    public class Hen : Chicken
    {  
        public Hen() : base()
        { 
        }

        public Hen(ChickenEntity entity) : base(entity)
        {
            Gender = ChickenGender.Female;
        }
        
    }  
}