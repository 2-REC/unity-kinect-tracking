using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class HeadTracking : MonoBehaviour {

    const int COLOR_WIDTH = KinectInputManager.COLOR_WIDTH;
    const int COLOR_HEIGHT = KinectInputManager.COLOR_HEIGHT;
    const int DEPTH_WIDTH = KinectInputManager.DEPTH_WIDTH;
    const int DEPTH_HEIGHT = KinectInputManager.DEPTH_HEIGHT;

    public KinectInputManager kinectInputManager;
    public BodyTracker2D bodyTracker;
    public BodyTracker3D bodyTracker3D;
    public Image image;

    public int headWidth = 20;
    public int roiWidthFactor = 2; // twice the head width
    public int roiHeightFactor = 2; // twice the head height
//TODO: change names!?
    public Vector3Int HSVGreenMin = new Vector3Int(90, 127, 127);
    public Vector3Int HSVGreenMax = new Vector3Int(95, 255, 255);
    public Vector3Int HSVPinkMin = new Vector3Int(165, 127, 127);
    public Vector3Int HSVPinkMax = new Vector3Int(175, 255, 255);
    public Vector3Int HSVOrangeMin = new Vector3Int(7, 127, 127);
    public Vector3Int HSVOrangeMax = new Vector3Int(20, 255, 255);
    public bool filterBodyData = true;

    Texture2D texture;
    byte[] hsvValues;
    int roiSize;


    void Awake() {
        texture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//        texture = new Texture2D(DEPTH_WIDTH, DEPTH_HEIGHT, TextureFormat.Alpha8, false);

        hsvValues = new byte[18];
/*
        hsvValues[0] = (byte)HSVGreenMin.x;
        hsvValues[1] = (byte)HSVGreenMin.y;
        hsvValues[2] = (byte)HSVGreenMin.z;
        hsvValues[3] = (byte)HSVGreenMax.x;
        hsvValues[4] = (byte)HSVGreenMax.y;
        hsvValues[5] = (byte)HSVGreenMax.z;

        hsvValues[6] = (byte)HSVPinkMin.x;
        hsvValues[7] = (byte)HSVPinkMin.y;
        hsvValues[8] = (byte)HSVPinkMin.z;
        hsvValues[9] = (byte)HSVPinkMax.x;
        hsvValues[10] = (byte)HSVPinkMax.y;
        hsvValues[11] = (byte)HSVPinkMax.z;

        hsvValues[12] = (byte)HSVOrangeMin.x;
        hsvValues[13] = (byte)HSVOrangeMin.y;
        hsvValues[14] = (byte)HSVOrangeMin.z;
        hsvValues[15] = (byte)HSVOrangeMax.x;
        hsvValues[16] = (byte)HSVOrangeMax.y;
        hsvValues[17] = (byte)HSVOrangeMax.z;
*/
    }

    void Start() {
        image.material.mainTexture = texture;

        roiSize = (headWidth * COLOR_WIDTH) / (2 * 100);
    }

    void Update() {

//TODO: do it once have found the head (only here for display purpose)
        var rawImage = kinectInputManager.GetColorBuffer();

        ulong id = bodyTracker.GetTrackedId(1);
        if (id != 0) {
            Vector2 position = bodyTracker.GetPosition(id, JointType.Head);
            if (position != Vector2.zero) {
                if ((position.x < COLOR_WIDTH) && (position.y < COLOR_HEIGHT)) {

//TODO: should check if valid bodyTracker3D as well
                    Vector3 pos = bodyTracker3D.GetPosition(id, JointType.Head);

                    // roiSize = (headWidth(m) * res) / (dist(m) * 2)
                    int roiWidth = (int)(roiSize / -pos.z);
                    int roiHeight = roiWidth; // square area

                    roiWidth *= roiWidthFactor;
                    roiHeight *= roiHeightFactor;

//TODO: use "min" & "max" methods directly
                    int roiX = (int)position.x - roiWidth / 2;
                    int roiY = (int)position.y - roiHeight / 2;
                    int roiW = roiWidth;
                    int roiH = roiHeight;

                    if (roiX < 0) roiX = 0;
                    if (roiY < 0) roiY = 0;
                    if ((roiX + roiW) >= COLOR_WIDTH) roiW = COLOR_WIDTH - 1 - roiX;
                    if ((roiY + roiH) >= COLOR_HEIGHT) roiH = COLOR_HEIGHT - 1 - roiY;

                    RectInt roi = new RectInt(roiX, roiY, roiW, roiH);

//                    var rawImage = kinectInputManager.GetColorBuffer();

                    if (filterBodyData) {
                        FilterBodyData(rawImage, roiX, roiY, roiX + roiW, roiY + roiH);
                    }

////////
//TODO: TO REMOVE!
//=> Here only for testing purpose (realtime value changes)
hsvValues[0] = (byte)HSVGreenMin.x;
hsvValues[1] = (byte)HSVGreenMin.y;
hsvValues[2] = (byte)HSVGreenMin.z;
hsvValues[3] = (byte)HSVGreenMax.x;
hsvValues[4] = (byte)HSVGreenMax.y;
hsvValues[5] = (byte)HSVGreenMax.z;

hsvValues[6] = (byte)HSVPinkMin.x;
hsvValues[7] = (byte)HSVPinkMin.y;
hsvValues[8] = (byte)HSVPinkMin.z;
hsvValues[9] = (byte)HSVPinkMax.x;
hsvValues[10] = (byte)HSVPinkMax.y;
hsvValues[11] = (byte)HSVPinkMax.z;

hsvValues[12] = (byte)HSVOrangeMin.x;
hsvValues[13] = (byte)HSVOrangeMin.y;
hsvValues[14] = (byte)HSVOrangeMin.z;
hsvValues[15] = (byte)HSVOrangeMax.x;
hsvValues[16] = (byte)HSVOrangeMax.y;
hsvValues[17] = (byte)HSVOrangeMax.z;
////////

                    FindBlobs(ref rawImage, COLOR_WIDTH, COLOR_HEIGHT, roi, true, 3, hsvValues);
//ApplyMask(ref rawImage, COLOR_WIDTH, COLOR_HEIGHT, roi, bodyIndexImage, DEPTH_WIDTH, DEPTH_HEIGHT);

                    // continue process
                    //...
                }
            }
        }

//TODO: don't need that (only here for display purpose)
        texture.LoadRawTextureData(rawImage);
        texture.Apply();
    }

    void FilterBodyData(byte[] rawImage, int startX, int startY, int endX, int endY) {
        var bodyIndexImage = kinectInputManager.GetBodyIndexBuffer();
        var bodyIndexCoordinates = kinectInputManager.GetDepthCoordinates();

        for (int colorY = startY; colorY < endY; ++colorY) {
            for (int colorX = startX; colorX < endX; ++colorX) {
                int colorIndex = (colorY * COLOR_WIDTH) + colorX;

                float colorMappedToDepthX = bodyIndexCoordinates[colorIndex].X;
                float colorMappedToDepthY = bodyIndexCoordinates[colorIndex].Y;

                if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                        !float.IsNegativeInfinity(colorMappedToDepthY)) {
                    int depthX = (int)(colorMappedToDepthX + 0.5f);
                    int depthY = (int)(colorMappedToDepthY + 0.5f);

                    if ((depthX >= 0) && (depthX < DEPTH_WIDTH) && (depthY >= 0) && (depthY < DEPTH_HEIGHT)) {
                        int depthIndex = (depthY * DEPTH_WIDTH) + depthX;
                        if (bodyIndexImage[depthIndex] != 0xff) {
                            continue;
                        }
                    }
                }

                int index = colorIndex * 4;
                rawImage[index] = 0;
                rawImage[index + 1] = 0;
                rawImage[index + 2] = 0;
                rawImage[index + 3] = 0;
            }
        }
    }


/*
    [DllImport("UnityOpenCV")]
    static extern void ProcessImage(ref byte[] raw, int width, int height);

    [DllImport("UnityOpenCV")]
    static extern void ProcessImageRegion(ref byte[] raw, int width, int height, RectInt roi);

    [DllImport("UnityOpenCV")]
    static extern void DetectColourInROI(ref byte[] raw, int width, int height, RectInt region, int hue1, int sat1, int val1, int hue2, int sat2, int val2);
*/

    [DllImport("UnityOpenCV")]
    static extern bool FindBlobs(ref byte[] raw, int width, int height, RectInt region, bool modifyImage, int numberColours, byte[] hsvValues);

    [DllImport("UnityOpenCV")]
    static extern void ApplyMask(ref byte[] raw, int width, int height, RectInt region, byte[] mask, int maskWidth, int maskHeight);

}
