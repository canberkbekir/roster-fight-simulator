using AI.Chickens;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Debugs
{ 
    public class DebugAIState : MonoBehaviour
    {
        [SerializeField]
        private RoosterAI   roosterAI;
        [FormerlySerializedAs("chickenAI")] [SerializeField]
        private HenAI   henAI;
        [SerializeField]
        private ChickAI chickAI;
        [SerializeField] 
        private TextMeshProUGUI stateText;

        void Start()
        { 
            roosterAI  = GetComponent<RoosterAI>();
            henAI  = GetComponent<HenAI>();
            chickAI = GetComponent<ChickAI>();
        }


        private void Update()
        {
            if (roosterAI)
            {
                stateText.text = $"{roosterAI.CurrentState}";
            }
            else if (henAI)
            {
                stateText.text = $"{henAI.CurrentState}";
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