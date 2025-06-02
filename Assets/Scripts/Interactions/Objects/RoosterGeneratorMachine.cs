using System;
using Creatures.Roosters;
using Interactions.Base;
using Managers;
using UnityEngine;

namespace Interactions.Objects
{
    public class RoosterGeneratorMachine : InteractableBase
    {
        [Header("Settings")]
        [SerializeField] private Transform spawnPoint;

        [Header("Which creature to spawn")]
        [SerializeField] private CreatureType fixedType; 

        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor); 
            GameManager.Instance.RoosterSpawnerManager.RequestSpawnRandomAt(spawnPoint, fixedType);
        }
    }
}