using AI.Roosters;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Debugs
{ 
    public class DebugAIState : MonoBehaviour
    {
        [SerializeField]
        private RoosterAI   roosterAI;
        [SerializeField]
        private ChickenAI   chickenAI;
        [SerializeField]
        private ChickAI chickAI;
        [SerializeField] 
        private TextMeshProUGUI stateText;

        void Start()
        { 
            roosterAI  = GetComponent<RoosterAI>();
            chickenAI  = GetComponent<ChickenAI>();
            chickAI = GetComponent<ChickAI>();
        }


        private void Update()
        {
            if (roosterAI)
            {
                stateText.text = $"{roosterAI.CurrentState}";
            }
            else if (chickenAI)
            {
                stateText.text = $"{chickenAI.CurrentState}";
            }
            else if (chickAI)
            {
                stateText.text = $"{chickAI.CurrentState}"; 
            }
            else
            {
                stateText.text = "No AI component found!";
            }
            
        }
    }
}