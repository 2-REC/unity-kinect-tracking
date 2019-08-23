using UnityEngine;

public abstract class TrackingListener : MonoBehaviour {

    public abstract void NotifyChange(Vector3 orientation);

}
