using Kinect = Windows.Kinect;
using UnityEngine;
using System.Collections.Generic;

public class BodyTracker3D : IBodyTracker {

    private class KinectBody3D : IKinectBody {
//TODO: should handle main body position (spine base, height "-Kinect.z"), and set other position relative to it
// BUT: many computations, potentially useless...
//        public Vector3 position;
        public Dictionary<Kinect.JointType, Vector3> joints;

        public KinectBody3D() {
//            position = Vector3.zero;
            joints = new Dictionary<Kinect.JointType, Vector3>();
        }

        public void Add(Kinect.Joint joint) {
            joints.Add(joint.JointType, Vector3.zero);
        }

        public void Update(Kinect.Joint joint) {
            joints[joint.JointType] = new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
        }
    }


    protected override IKinectBody InstantiateKinectBody() {
        return new KinectBody3D();
    }


//TODO: would be better more generic...
    public Vector3 GetPosition(ulong bodyId, Kinect.JointType jointType) {
        if (bodies.TryGetValue(bodyId, out IKinectBody kinectBody)) {
            KinectBody3D kinectBody3D = (KinectBody3D)kinectBody;
            if (kinectBody3D.joints.TryGetValue(jointType, out Vector3 position)) {
                return position;
            }
        }
        return Vector3.zero; //?
    }

    public Dictionary<Kinect.JointType, Vector3> GetJoints(ulong bodyId) {
        if (bodies.TryGetValue(bodyId, out IKinectBody kinectBody)){
            KinectBody3D kinectBody3D = (KinectBody3D)kinectBody;
            return kinectBody3D.joints;
        }
        return null;
    }

}
