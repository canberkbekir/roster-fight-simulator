using System;
using Genes;
using Genes.Base;
using Roosters.Components;
using UnityEngine;

namespace Roosters.Components
{
    /// <summary>
    /// A serializable snapshot of everything that makes one RoosterEntity unique.
    /// </summary>
    [Serializable]
    public struct RoosterState
    {
        public string Name;
        public int Strength, Agility, Endurance, Intelligence, Health;
        public GeneInstance[] GeneInstances;

        public RoosterState(RoosterEntity e)
        {
            Name         = e.roosterName;
            Strength     = e.Stats.strength;
            Agility      = e.Stats.agility;
            Endurance    = e.Stats.endurance;
            Intelligence = e.Stats.intelligence;
            Health       = e.Stats.health;
            GeneInstances = e.Genome.GeneInstances != null
                ? (GeneInstance[])e.Genome.GeneInstances.Clone()
                : Array.Empty<GeneInstance>();
        }
    }
}