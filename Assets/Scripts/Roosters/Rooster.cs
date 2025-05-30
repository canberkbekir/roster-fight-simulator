using System;
using Genes.Base;
using Roosters.Components;
using Sirenix.OdinInspector;

namespace Roosters
{
    public class Rooster
    {
        public readonly string Name;
        public int Strength, Agility, Endurance, Intelligence, Health;
        public Gene[] Genes;
        [Required]
        public RoosterGender Gender;

        public Rooster()
        { 
        }
        public Rooster(RoosterEntity e)
        {
            Name         = e.RoosterName;
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

    public enum RoosterGender
    {
        Male,
        Female
    }
}