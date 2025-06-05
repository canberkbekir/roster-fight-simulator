using System;
using Creatures.Genes.Base;
using Sirenix.OdinInspector;

namespace Creatures.Chickens.Base
{
    public abstract class Chicken
    {
        public string Name;
        public int Strength, Agility, Endurance, Intelligence, Health;
        public Gene[] Genes;
        [Required]
        public ChickenGender Gender;

        protected Chicken()
        {
            
        }
        protected Chicken(ChickenEntity e)
        {
            Name         = e.ChickenName;
            Strength     = e.Stats.strength;
            Agility      = e.Stats.agility;
            Endurance    = e.Stats.endurance;
            Intelligence = e.Stats.intelligence;
            Health       = e.Stats.health;
            Gender = e.Gender;
            Genes = e.Genome.Genes != null
                ? (Gene[])e.Genome.Genes.Clone()
                : Array.Empty<Gene>();
        } 
    }

    public enum ChickenGender
    {
        Male,
        Female
    }
    
    public enum CreatureType
    {
        Rooster,
        Chicken,
        Chick,
        Egg
    }
}