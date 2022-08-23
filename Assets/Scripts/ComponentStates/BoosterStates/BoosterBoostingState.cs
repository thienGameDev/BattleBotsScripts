using UnityEngine;

namespace ComponentStates.BoosterStates {
    public class BoosterBoostingState : BoosterBaseState
    {
        private float _loadingBoosterTime;
        private bool _boosted;
        public override void EnterState(BoosterStateManager booster){
            //booster.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            _loadingBoosterTime = booster.loadingBoosterTime;
        }  

        public override void UpdateState(BoosterStateManager booster){
            if (_loadingBoosterTime > 0) _loadingBoosterTime -= Time.deltaTime;
            if (_boosted || !(_loadingBoosterTime <= 0)) return;
            Vector2 boostDirection = (booster.Head.position - booster.Tail.position).normalized;
            booster.GetComponent<RelativeJoint2D>().connectedBody.AddForce(boostDirection * booster.boostingForce, ForceMode2D.Impulse);
            _boosted = true;
            Debug.Log("Booster is boosted.");
            booster.SwitchState(booster.endState);
        }
    }
}
