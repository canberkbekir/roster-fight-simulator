using Creatures.Chickens.Base;

namespace Creatures.Chickens.Roosters
{
    public class Rooster : Chicken
    {  
        public Rooster() : base()
        { 
        }

        public Rooster(ChickenEntity entity) : base(entity)
        {
            Gender = ChickenGender.Male;
        }
        
    }  
}