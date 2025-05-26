using System;
using System.Collections.Generic;
using Interactions.Base;
using Managers;
using Roosters;
using Roosters.Components;
using UI;
using UnityEngine;

namespace Interactions.Objects
{
    public class Breeder : InteractableBase
    {
        [Header("Settings")] [SerializeField] private Transform spawnPoint;
        [SerializeField] private int maxRoosters = 10;

        [Space] [Header("Rooster Detection Box")]
        [SerializeField] private Vector3 centerOffset = Vector3.zero;
        [SerializeField] private Vector3 size = new Vector3(1, 1, 1);
        [SerializeField] private Color gizmoColor = Color.green;
        [SerializeField] private LayerMask detectionMask;


        public List<Rooster> CurrentRoosters { get; private set; }

        public int MaxRoosters => maxRoosters;

        private GameObject[] _roostersGameObjects;
        private RoosterSpawnerManager _roosterSpawnerManager;
        private PlayerUIHandler _playerUIHandler;
        private readonly Collider[] _results = new Collider[256];

        private void Awake()
        {
            _roosterSpawnerManager = GameManager.Instance.RoosterSpawnerManager;
            _playerUIHandler = PlayerUIHandler.Instance;
            if (_roosterSpawnerManager == null)
            {
                Debug.LogError("RoosterSpawnerManager is not initialized.");
            }

            CurrentRoosters = new List<Rooster>();
            _roostersGameObjects = new GameObject[maxRoosters];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor; 
            Gizmos.DrawWireCube(transform.position + centerOffset, size);
        }

        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor);
            CheckForRoosters();
            OpenBreederUI();
        }

        private void CheckForRoosters()
        { 
            var hitCount = Physics.OverlapBoxNonAlloc(
                transform.position + centerOffset,
                size * 0.5f,
                _results,
                Quaternion.identity,
                detectionMask
            );
 
            if (hitCount == 0)
            {
                Debug.Log("No roosters found in the detection area.");
                return;
            }
 
            for (var i = 0; i < hitCount; i++)
            {
                var col = _results[i];
                if (col == null) 
                    continue;  

                var rooster = col.GetComponent<RoosterEntity>().Rooster;
                if (rooster != null)
                {
                    CurrentRoosters.Add(rooster);
                }
                else
                {
                    Debug.LogWarning($"Collider {col.name} does not have a Rooster component.");
                }
            } 
            Array.Clear(_results, 0, _results.Length); // Clear results for next check
            Debug.Log($"Checked for roosters, found {hitCount} in the detection area.");
            
        }

        private void OpenBreederUI()
        {
            if (!_playerUIHandler)
            {
                Debug.LogError("PlayerUIHandler is not initialized.");
                return;
            }

            if (_playerUIHandler.BreederUI == null)
            {
                Debug.LogError("BreederUI is not assigned in PlayerUIHandler.");
                return;
            }

            _playerUIHandler.BreederUI.Show();
            _playerUIHandler.BreederUI.Init(CurrentRoosters.ToArray());
            Debug.Log("Breeder UI opened.");
        }
        
       

    }
}