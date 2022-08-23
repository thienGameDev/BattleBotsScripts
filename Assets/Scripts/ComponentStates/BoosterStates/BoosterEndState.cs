using UnityEngine;

namespace ComponentStates.BoosterStates {
    public class BoosterEndState : BoosterBaseState
    {
        public override void EnterState(BoosterStateManager booster){
            Object.Destroy(booster.gameObject);
        }
        public override void UpdateState(BoosterStateManager booster){

        }
    }
}
