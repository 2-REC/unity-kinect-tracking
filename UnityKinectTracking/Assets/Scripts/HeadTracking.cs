using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class HeadTracking : MonoBehaviour {

    const int COLOR_WIDTH = KinectInputManager.COLOR_WIDTH;
    const int COLOR_HEIGHT = KinectInputManager.COLOR_HEIGHT;

    public int roiWidthFactor = 2; // twice the head width
    public int roiHeightFactor = 2; // twice the head height

    public KinectInputManager kinectInputManager;
    public BodyTracker2D bodyTracker;
    public Image image;

    Texture2D texture;


    void Start() {
        texture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//        texture = new Texture2D(KinectInputManager.DEPTH_WIDTH, KinectInputManager.DEPTH_HEIGHT, TextureFormat.Alpha8, false);
        image.material.mainTexture = texture;

    }

    void Update() {

//TODO: do it once have found the head (only here for display purpose)
        var rawImage = kinectInputManager.GetColorBuffer();

        ulong id = bodyTracker.GetTrackedId(1);
        if (id != 0) {

            RectInt roi;
            Vector2 position = bodyTracker.GetPosition(id, JointType.Head);
            if (position != Vector2.zero) {
                if ((position.x < COLOR_WIDTH) && (position.y < COLOR_HEIGHT)) {
//TODO: determine using the depth value OR body index data (dependent on distance!)
int roiWidth = 200 * (roiWidthFactor / 2);
int roiHeight = 200 * (roiHeightFactor / 2);

//TODO: use "min" & "max" methods directly
                    int x = (int)position.x - roiWidth / 2;
                    int y = (int)position.y - roiHeight / 2;
                    int w = roiWidth;
                    int h = roiHeight;

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if ((x + w) >= COLOR_WIDTH) w = COLOR_WIDTH - 1 - x;
                    if ((y + h) >= COLOR_HEIGHT) h = COLOR_HEIGHT - 1 - y;

                    roi = new RectInt(x, y, w, h);

//                    var rawImage = kinectInputManager.GetColorBuffer();
//...

//TODO: check in library if roi is in image!!!!
                    ProcessImageRegion(ref rawImage, COLOR_WIDTH, COLOR_HEIGHT, roi);
                }
            }
        }

//TODO: probably don't need that (only here for display purpose)
        texture.LoadRawTextureData(rawImage);
        texture.Apply();
    }


//    [DllImport("UnityOpenCV")]
//    static extern void ProcessImage(ref byte[] raw, int width, int height);

    [DllImport("UnityOpenCV")]
    static extern void ProcessImageRegion(ref byte[] raw, int width, int height, RectInt roi);

}
