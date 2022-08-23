using UnityEngine;

namespace ComponentStates {
    public abstract class ComponentStateManager : MonoBehaviour {
        public Transform Head { get; set; }
        public Transform Tail { get; set; }

    }
}