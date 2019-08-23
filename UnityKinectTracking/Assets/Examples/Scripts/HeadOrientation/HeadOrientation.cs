using UnityEngine;
using UnityEngine.UI;

public class HeadOrientation : TrackingListener {

    public Transform head;
    public Text text;


    public override void NotifyChange(Vector3 orientation) {
        head.eulerAngles = orientation;
        text.text = "YAW: " + ((int)(orientation.y * 100)) / 100.0f;
    }

}
