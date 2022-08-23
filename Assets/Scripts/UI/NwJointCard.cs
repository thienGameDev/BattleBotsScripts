using RobotComponentLib.ChassisLib;
using UnityEngine;

namespace UI {
    public class NwJointCard : MonoBehaviour {
        public NwCard OccupiedCard { get; private set; }
        
        // private uint _occupiedCardNetId;
        public NwChassis.ChassisJoint ChassisJoint { get; set; }
        public Vector3 Position => transform.position;
        public void SetupJointCard(NwChassis.ChassisJoint joint, NwChassis chassis) {
            ChassisJoint = joint;
            string jointType = joint.type;
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                if (child.name != jointType) continue;
                child.gameObject.SetActive(true);
                gameObject.name = jointType; // To match with Card Don't change!
            }
        }
        
        public void SetOccupiedCard(NwCard card) {
            OccupiedCard = card;
        }
    }
}