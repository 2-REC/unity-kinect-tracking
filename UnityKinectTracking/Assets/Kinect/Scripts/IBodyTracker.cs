using Kinect = Windows.Kinect;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public abstract class IBodyTracker : MonoBehaviour {

    public KinectInputManager KinectInputManager { get { return kinectInputManager; } private set { kinectInputManager = value; } }
    [SerializeField]
    protected KinectInputManager kinectInputManager;

//TODO: should check value in Editor script
//TODO: should also be as readonly
    public uint nbBodiesTracked = 1;

    protected interface IKinectBody {
        void Add(Kinect.Joint joint);
        void Update(Kinect.Joint joint);
    }


    protected Dictionary<ulong, IKinectBody> bodies;
    List<ulong> trackedIds;

    List<ulong> limitedTrackedIds = new List<ulong>();
    public ReadOnlyCollection<ulong> TrackedIdsList {
        get { return limitedTrackedIds.AsReadOnly(); }
    }


    void Awake() {
        bodies = new Dictionary<ulong, IKinectBody>();
        trackedIds = new List<ulong>();
    }

    void Start() {
        if (nbBodiesTracked == 0) {
            nbBodiesTracked = 1;
        }
        else if (nbBodiesTracked > 6) {
            nbBodiesTracked = 6;
        }
    }

    void Update () {
        Kinect.Body[] data = kinectInputManager.GetBodyData();
        if (data == null) {
            return;
        }

        trackedIds.Clear();
        foreach(var body in data) {
            if (body == null) {
                continue;
            }

            if(body.IsTracked) {
                trackedIds.Add(body.TrackingId);
            }
        }

        // delete untracked bodies
/*
        foreach(ulong trackingId in bodies.Keys) {
            if(!trackedIds.Contains(trackingId)) {
                limitedTrackedIds.Remove(trackingId);
                bodies.Remove(trackingId);
            }
        }
*/
        List<ulong> keys = new List<ulong>(bodies.Keys);
        foreach (ulong trackingId in keys) {
            if(!trackedIds.Contains(trackingId)) {
                limitedTrackedIds.Remove(trackingId);
                bodies.Remove(trackingId);
            }
        }

        foreach(var body in data) {
            if (body == null) {
                continue;
            }

            if(body.IsTracked) {
                if ((limitedTrackedIds.Count < nbBodiesTracked) && !limitedTrackedIds.Contains(body.TrackingId)) {
                    limitedTrackedIds.Add(body.TrackingId);
                    bodies[body.TrackingId] = CreateBodyObject(body);
                }

                if(limitedTrackedIds.Contains(body.TrackingId)) {
                    RefreshBodyObject(body);
                }
            }
        }
    }


    protected IKinectBody CreateBodyObject(Kinect.Body body) {
        IKinectBody kinectBody = InstantiateKinectBody();
        for (Kinect.JointType jointType = Kinect.JointType.SpineBase; jointType <= Kinect.JointType.ThumbRight; ++jointType) {
            if (body.Joints.TryGetValue(jointType, out Kinect.Joint joint)) {
                kinectBody.Add(joint);
            }
        }
        return kinectBody;
    }

    protected void RefreshBodyObject(Kinect.Body body) {
        IKinectBody kinectBody = bodies[body.TrackingId];
        for (Kinect.JointType jointType = Kinect.JointType.SpineBase; jointType <= Kinect.JointType.ThumbRight; ++jointType) {
            if (body.Joints.TryGetValue(jointType, out Kinect.Joint joint)) {
                kinectBody.Update(joint);
            }
        }
    }


    public ulong GetTrackedId(int index) {
        if (index <= limitedTrackedIds.Count) {
            return limitedTrackedIds[index - 1];
        }
        return 0;
    }


    protected abstract IKinectBody InstantiateKinectBody();

}
