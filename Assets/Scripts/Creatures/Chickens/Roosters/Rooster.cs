using System.Linq;
using System.Reflection;
using Creatures.Chickens.Base;
using Creatures.Genes.Base;

namespace Creatures.Chickens.Roosters
{
    public class Rooster : Chicken
    {
        private static readonly FieldInfo[] GeneFields = typeof(Gene)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        public Rooster() : base() { }

        public Rooster(ChickenEntity entity) : base(entity)
        {
            Gender = ChickenGender.Male;
        }

        public override Chicken Clone()
        {
            return new Rooster
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
            foreach (var field in GeneFields)
                field.SetValue(copy, field.GetValue(original));
            return copy;
        }
    }
}