using UnityEngine;
using UnityEngine.UI;

public class MessageSender : TrackingListener {

    public override void NotifyChange(Vector3 orientation) {
        Debug.Log("TODO: Send orientation " + orientation);
    }

}
