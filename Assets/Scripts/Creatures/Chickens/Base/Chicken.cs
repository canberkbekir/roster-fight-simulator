using System;
using Creatures.Genes.Base; 
using UnityEngine;

namespace Creatures.Chickens.Base
{
     public abstract class Chicken
    {
        [Header("Basic Information")]
        [SerializeField] private string name = "Unnamed Chicken";
        [SerializeField] private ChickenGender gender = ChickenGender.Female;
        
        [Header("Stats")]
        [SerializeField, Range(0, 100)] private int strength = 50;
        [SerializeField, Range(0, 100)] private int agility = 50;
        [SerializeField, Range(0, 100)] private int endurance = 50;
        [SerializeField, Range(0, 100)] private int intelligence = 50;
        [SerializeField, Range(0, 100)] private int health = 100;
        
        [Header("Genetics")]
        [SerializeField] private Gene[] genes = Array.Empty<Gene>();

        public string Name
        {
            get => name;
            protected set => name = value;
        }

        public ChickenGender Gender
        {
            get => gender;
            set => gender = value;
        }

        public int Strength
        {
            get => strength;
            set => strength = value;
        }

        public int Agility
        {
            get => agility;
            set => agility = value;
        }

        public int Endurance
        {
            get => endurance;
            set => endurance = value;
        }

        public int Intelligence
        {
            get => intelligence;
            set => intelligence = value;
        }

        public int Health
        {
            get => health;
            set => health = value;
        }

        public Gene[] Genes
        {
            get => genes;
            set => genes = value ?? Array.Empty<Gene>();
        }

        protected Chicken() { }

        protected Chicken(ChickenEntity e)
        {
            if (!e) throw new ArgumentNullException(nameof(e));

            Name         = e.ChickenName;
            Strength     = e.Stats?.strength     ?? strength;
            Agility      = e.Stats?.agility      ?? agility;
            Endurance    = e.Stats?.endurance    ?? endurance;
            Intelligence = e.Stats?.intelligence ?? intelligence;
            Health       = e.Stats?.health       ?? health;
            Gender       = e.Gender;
            Genes        = e.Genome?.Genes != null
                           ? (Gene[])e.Genome.Genes.Clone()
                           : Array.Empty<Gene>();
        }

        public Gene GetGeneById(int geneId) =>
            Array.Find(Genes, g => g.GeneId == geneId);

        public Gene GetGeneByName(string geneName) =>
            Array.Find(Genes, g => g.GeneName == geneName);

        public bool HasGene(int geneId) =>
            GetGeneById(geneId) != null;

        public bool HasGene(string geneName) =>
            GetGeneByName(geneName) != null;

        public int GeneCount => Genes.Length;

        public abstract Chicken Clone();
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