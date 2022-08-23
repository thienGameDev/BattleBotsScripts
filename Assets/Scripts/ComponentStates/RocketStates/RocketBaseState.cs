using UnityEngine;

namespace ComponentStates.RocketStates {
  public abstract class RocketBaseState
  {
    public abstract void EnterState(RocketStateManager rocket);
    public abstract void UpdateState(RocketStateManager rocket);
    public abstract void OnServerTriggerEnter2D(RocketStateManager rocket, Collider2D other);
    public abstract void OnClientTriggerEnter2D(RocketStateManager rocket, Collider2D other);
  }
}
