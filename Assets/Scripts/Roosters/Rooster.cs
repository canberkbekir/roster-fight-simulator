using System;
using Genes.Base;
using Roosters.Components;

namespace Roosters
{
    public class Rooster
    {
        public string Name;
        public int Strength, Agility, Endurance, Intelligence, Health;
        public Gene[] Genes;

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
            Genes = e.Genome.Genes != null
                ? (Gene[])e.Genome.Genes.Clone()
                : Array.Empty<Gene>();
        }
    }
}