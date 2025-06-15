using Creatures.Chickens.Base;
using UnityEngine;

namespace Interactions.Objects.Breeders
{
    public class BreederChickenTracker : MonoBehaviour
    {
        private Breeder _breeder;
        private ChickenEntity _chicken;

        public void Init(Breeder breeder, ChickenEntity chicken)
        {
            _breeder = breeder;
            _chicken = chicken;
        }

        private void OnDestroy()
        {
            if (_breeder && _chicken)
            {
                _breeder.UnregisterChicken(_chicken);
            }
        }
    }
}
