using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class HeadTracking : MonoBehaviour {

    [StructLayout(LayoutKind.Sequential, Size = 4), Serializable]
    public struct Scalar {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public double[] val;

/*
        public Scalar(double v1, double v2, double v3, double v4 = 0) {
            val = new double[4];
            val[0] = v1;
            val[1] = v2;
            val[2] = v3;
            val[3] = v4;
        }
*/
        public Scalar(Vector3Int v3i) {
            val = new double[4];
            val[0] = v3i.x;
            val[1] = v3i.y;
            val[2] = v3i.z;
            val[3] = 0;
        }
    }


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
    int roiSize;
    Scalar[] minHSV;
    Scalar[] maxHSV;


    void Awake() {
        texture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//        texture = new Texture2D(DEPTH_WIDTH, DEPTH_HEIGHT, TextureFormat.Alpha8, false);

        minHSV = new Scalar[] {
            new Scalar(HSVGreenMin),
            new Scalar(HSVPinkMin),
            new Scalar(HSVOrangeMin)
        };

        maxHSV = new Scalar[] {
            new Scalar(HSVGreenMax),
            new Scalar(HSVPinkMax),
            new Scalar(HSVOrangeMax)
        };
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
//=> Or have a combined 2D+3D tracker...
                    Vector3 pos = bodyTracker3D.GetPosition(id, JointType.Head);

                    RectInt roi = GetROI(position.x, position.y, -pos.z);

//                    var rawImage = kinectInputManager.GetColorBuffer();

                    if (filterBodyData) {
                        FilterBodyData(rawImage, roi);
                    }

//////// RUNTIME_COLOURS - BEGIN
//TODO: To remove, only for test purpose (runtime colours setting in Editor)
minHSV = new Scalar[] {
    new Scalar(HSVGreenMin),
    new Scalar(HSVPinkMin),
    new Scalar(HSVOrangeMin)
};

maxHSV = new Scalar[] {
    new Scalar(HSVGreenMax),
    new Scalar(HSVPinkMax),
    new Scalar(HSVOrangeMax)
};
//////// RUNTIME_COLOURS - END


                    DetectColoursInROI(ref rawImage, COLOR_WIDTH, COLOR_HEIGHT, roi, true, 3, minHSV, maxHSV);

                    // continue process
                    //...

                }
            }
        }

//TODO: don't need that (only here for display purpose)
        texture.LoadRawTextureData(rawImage);
        texture.Apply();
    }


    RectInt GetROI(float x, float y, float cameraDistance) {
        // roiSize = (headWidth(m) * res) / (dist(m) * 2)
        int roiWidth = (int)(roiSize / cameraDistance);
        int roiHeight = roiWidth; // square area

        roiWidth *= roiWidthFactor;
        roiHeight *= roiHeightFactor;

        int roiX = (int)x - roiWidth / 2;
        int roiY = (int)y - roiHeight / 2;
        int roiW = roiWidth;
        int roiH = roiHeight;

        roiX = Math.Max(0, roiX);
        roiY = Math.Max(0, roiY);
        roiW = Math.Min(roiW, COLOR_WIDTH - 1 - roiX);
        roiH = Math.Min(roiH, COLOR_HEIGHT - 1 - roiY);

        return new RectInt(roiX, roiY, roiW, roiH);
    }

    void FilterBodyData(byte[] rawImage, RectInt roi) {
        var bodyIndexImage = kinectInputManager.GetBodyIndexBuffer();
        var bodyIndexCoordinates = kinectInputManager.GetDepthCoordinates();

        int startX = roi.x;
        int startY = roi.y;
        int endX = roi.x + roi.width;
        int endY = roi.y + roi.height;

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
    static extern void ApplyMask(ref byte[] raw, int width, int height, RectInt region, byte[] mask, int maskWidth, int maskHeight);
*/

    [DllImport("UnityOpenCV", CallingConvention = CallingConvention.Cdecl)]
    static extern bool DetectColoursInROI(ref byte[] ppRaw, int width, int height, RectInt region, bool modifyImage, int numberColours, Scalar[] pMinHSV, Scalar[] pMaxHSV);

}
