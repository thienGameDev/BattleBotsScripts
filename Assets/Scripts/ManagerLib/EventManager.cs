using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace ManagerLib {
    public class EventManager : PersistentSingleton<EventManager> {
        private Dictionary<string, UnityEvent<float>> _eventDictionary;
        protected override void Awake() {
            base.Awake();
            Init();
        }

        private void Init() {
            _eventDictionary ??= new Dictionary<string, UnityEvent<float>>();
        }

        public static void StartListening(string eventId, UnityAction<float> listener) {
            // Debug.Log($"Start listening event {eventId}.");
            if (Instance._eventDictionary.TryGetValue(eventId, out UnityEvent<float> thisEvent)) {
                thisEvent.AddListener(listener);
            }
            else {
                thisEvent = new UnityEvent<float>();
                thisEvent.AddListener(listener);
                Instance._eventDictionary.Add(eventId, thisEvent);
            }
        }

        public static void StopListening(string eventId, UnityAction<float> listener) {
            // if (Instance == null) {
            //     return;
            // }
            if (Instance._eventDictionary.TryGetValue(eventId, out UnityEvent<float> thisEvent)) {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(string eventId, float eventParameter) {
            if (Instance._eventDictionary.TryGetValue(eventId, out UnityEvent<float> thisEvent)) {
                Debug.Log($"Event {eventId} is triggered");
                thisEvent.Invoke(eventParameter);
            }
        }
    }
}
