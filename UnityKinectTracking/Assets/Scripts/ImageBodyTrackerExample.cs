using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class ImageBodyTrackerExample : MonoBehaviour {

    const int COLOR_WIDTH = KinectInputManager.COLOR_WIDTH;
    const int COLOR_HEIGHT = KinectInputManager.COLOR_HEIGHT;


    public KinectInputManager kinectInputManager;
    public BodyTracker2D bodyTracker;
    public Image image;
    public bool trackFullBody = true;

    Texture2D texture;


    void Start() {
        texture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//        texture = new Texture2D(KinectInputManager.DEPTH_WIDTH, KinectInputManager.DEPTH_HEIGHT, TextureFormat.Alpha8, false);
        image.material.mainTexture = texture;

    }

    void Update() {

        List<RectInt> rois = new List<RectInt>();

        ulong id = bodyTracker.GetTrackedId(1);
        if (id != 0) {

            Dictionary<JointType, Vector2> joints;
            if (!trackFullBody) {
//TODO: add possibility in Editor to specify joints
                List<JointType> jointTypes = new List<JointType> {
                    JointType.Head,
                    JointType.HandRight,
                    JointType.HandLeft
                };

                joints = new Dictionary<JointType, Vector2>();
                foreach (JointType jointType in jointTypes) {
                    joints.Add(jointType, bodyTracker.GetPosition(id, jointType));
                }
            }
            else {
                joints = bodyTracker.GetJoints(id);
            }

            foreach (KeyValuePair<JointType, Vector2> joint in joints) {
                Vector2 point = joint.Value;

                if (point != Vector2.zero) {
                    if ((point.x >= COLOR_WIDTH) || (point.y >= COLOR_HEIGHT))
                        continue;

                    int x = (int)point.x - 100;
                    int y = (int)point.y - 100;
                    int w = 200;
                    int h = 200;

                    if (joint.Key == JointType.Head) {
                        x = (int)point.x - 200;
                        y = (int)point.y - 200;
                        w = 400;
                        h = 400;
                    }

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if ((x + w) >= COLOR_WIDTH) w = COLOR_WIDTH - 1 - x;
                    if ((y + h) >= COLOR_HEIGHT) h = COLOR_HEIGHT - 1 - y;
                    rois.Add(new RectInt(x, y, w, h));
                }
            }

        }


        var rawImage = kinectInputManager.GetColorBuffer();
        foreach (RectInt roi in rois) {
//TODO: check in library if roi is in image!!!!
            ProcessImageRegion(ref rawImage, COLOR_WIDTH, COLOR_HEIGHT, roi);
        }
//        var rawImage = kinectInputManager.GetBodyIndexBuffer();

        texture.LoadRawTextureData(rawImage);
        texture.Apply();
    }


//    [DllImport("UnityOpenCV")]
//    static extern void ProcessImage(ref byte[] raw, int width, int height);

    [DllImport("UnityOpenCV")]
    static extern void ProcessImageRegion(ref byte[] raw, int width, int height, RectInt roi);

}
