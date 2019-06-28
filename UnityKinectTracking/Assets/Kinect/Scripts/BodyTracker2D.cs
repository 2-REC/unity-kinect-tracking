using Kinect = Windows.Kinect;
using UnityEngine;
using System.Collections.Generic;

public class BodyTracker2D : IBodyTracker {

    private class KinectBody2D : IKinectBody {
//TODO: should handle main body position (spine base), and set other position relative to it
// BUT: many computations, potentially useless...
//        public Vector2 position;
        public Dictionary<Kinect.JointType, Vector2> joints;
        private KinectInputManager kinectInputManager;

        public KinectBody2D(KinectInputManager kinectInputManager) {
//            position = Vector2.zero;
            joints = new Dictionary<Kinect.JointType, Vector2>();

            this.kinectInputManager = kinectInputManager;
        }

        public void Add(Kinect.Joint joint) {
            joints.Add(joint.JointType, Vector2.zero);
        }

        public void Update(Kinect.Joint joint) {
            joints[joint.JointType] = kinectInputManager.GetImagePoint(joint.Position);
        }
    }


    protected override IKinectBody InstantiateKinectBody() {
        return new KinectBody2D(kinectInputManager);
    }


//TODO: would be better more generic...
    public Vector2 GetPosition(ulong bodyId, Kinect.JointType jointType) {
        if (bodies.TryGetValue(bodyId, out IKinectBody kinectBody)) {
            KinectBody2D kinectBody2D = (KinectBody2D)kinectBody;
            if (kinectBody2D.joints.TryGetValue(jointType, out Vector2 position)) {
                return position;
            }
        }
        return Vector2.zero; //?
    }

    public Dictionary<Kinect.JointType, Vector2> GetJoints(ulong bodyId) {
        if (bodies.TryGetValue(bodyId, out IKinectBody kinectBody)){
            KinectBody2D kinectBody2D = (KinectBody2D)kinectBody;
            return kinectBody2D.joints;
        }
        return null;
    }

}
