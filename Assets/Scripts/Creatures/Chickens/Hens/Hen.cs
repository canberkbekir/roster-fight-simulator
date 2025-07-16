using System.Linq;
using System.Reflection;
using Creatures.Chickens.Base;
using Creatures.Genes.Base;
using UnityEngine;

namespace Creatures.Chickens.Hens
{
    public class Hen : Chicken
    {
        private static readonly FieldInfo[] GeneFields = typeof(Gene)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        public Hen() : base() { }

        public Hen(ChickenEntity entity) : base(entity)
        {
            Gender = ChickenGender.Female;
        }

        public override Chicken Clone()
        {
            return new Hen
            {
                Name         = Name,
                Gender       = Gender,
                Strength     = Strength,
                Agility      = Agility,
                Endurance    = Endurance,
                Intelligence = Intelligence,
                Health       = Health,
                Genes        = Genes?
                                   .Select(CloneGene)
                                   .Where(g => g != null)
                                   .ToArray()
                               ?? new Gene[0]
            };
        }

        private Gene CloneGene(Gene original)
        {
            if (original == null) return null;
            var copy = new Gene();
            foreach (var f in GeneFields)
                f.SetValue(copy, f.GetValue(original));
            return copy;
        }
    }
}