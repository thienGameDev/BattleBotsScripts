using Mirror;

namespace Utilities {
    public class StaticNetworkInstance<T> : NetworkBehaviour where T : NetworkBehaviour 
    {
        public static T Instance { get; private set; }
        protected virtual void Awake() => Instance = this as T;

        protected virtual void OnApplicationQuit() {
            Instance = null;
            Destroy(gameObject);
        }
    }
    public abstract class NetWorkSingleton<T> : StaticNetworkInstance<T> where T : NetworkBehaviour {
        protected override void Awake() {
            if (Instance != null) Destroy(gameObject);
            base.Awake();
        }
    }

    public abstract class PersistentNetworkSingleton<T> : NetWorkSingleton<T> where T : NetworkBehaviour {
        protected void Start() {
            DontDestroyOnLoad(gameObject);
        }
    }
}