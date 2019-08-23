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

    public TrackingListener trackingListener;

    public int headWidth = 20;
    public int roiWidthFactor = 2; // twice the head width
    public int roiHeightFactor = 2; // twice the head height
//TODO: change names!?
    public Vector3Int leftHSVMin = new Vector3Int(90, 127, 127);  // greenish
    public Vector3Int leftHSVMax = new Vector3Int(95, 255, 255);
    public Vector3Int rightHSVMin = new Vector3Int(165, 127, 127);  // pinkish
    public Vector3Int rightHSVMax = new Vector3Int(175, 255, 255);
    public Vector3Int topHSVMin = new Vector3Int(7, 127, 127);  // orangish
    public Vector3Int topHSVMax = new Vector3Int(20, 255, 255);
    public bool filterBodyData = true;

    bool foundOrientation;
    Vector3 orientation;

//////// DISPLAY - BEGIN
    Texture2D texture;
    public Image image;
//////// DISPLAY - MID

    int roiSize;
    Scalar[] minHSV;
    Scalar[] maxHSV;


    void Awake() {
//////// DISPLAY - BEGIN
        texture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//        texture = new Texture2D(DEPTH_WIDTH, DEPTH_HEIGHT, TextureFormat.Alpha8, false);
//////// DISPLAY - MID

        minHSV = new Scalar[] {
            new Scalar(leftHSVMin),
            new Scalar(rightHSVMin),
            new Scalar(topHSVMin)
        };

        maxHSV = new Scalar[] {
            new Scalar(leftHSVMax),
            new Scalar(rightHSVMax),
            new Scalar(topHSVMax)
        };
    }

    void Start() {
        orientation = Vector3.zero;
        roiSize = (headWidth * COLOR_WIDTH) / (2 * 100);

//////// DISPLAY - BEGIN
        image.material.mainTexture = texture;
//////// DISPLAY - MID
    }

    void Update() {
        foundOrientation = false;

//////// DISPLAY - BEGIN
        var rawImage = kinectInputManager.GetColorBuffer();
//////// DISPLAY - MID

        ulong id = bodyTracker.GetTrackedId(1);
        if (id != 0) {
            Vector2 position = bodyTracker.GetPosition(id, JointType.Head);
            if (position != Vector2.zero) {
                if ((position.x < COLOR_WIDTH) && (position.y < COLOR_HEIGHT)) {

                    //TODO: should check if valid bodyTracker3D as well
                    //=> Or have a combined 2D+3D tracker
                    Vector3 pos = bodyTracker3D.GetPosition(id, JointType.Head);

                    RectInt roi = GetROI(position.x, position.y, -pos.z);

//////// DISPLAY - MID
//                    var rawImage = kinectInputManager.GetColorBuffer();
//////// DISPLAY - END

                    if (filterBodyData) {
                        FilterBodyData(rawImage, roi);
                    }

//////// RUNTIME_COLOURS - BEGIN
/*
                    // For test purpose (runtime colours setting in Editor)
                    minHSV = new Scalar[] {
                        new Scalar(leftHSVMin),
                        new Scalar(rightHSVMin),
                        new Scalar(topHSVMin)
                    };

                    maxHSV = new Scalar[] {
                        new Scalar(leftHSVMax),
                        new Scalar(rightHSVMax),
                        new Scalar(topHSVMax)
                    };
*/
//////// RUNTIME_COLOURS - MID


                    Vector3[] blobs = new Vector3[3];
                    // Blobs are defined by their position (x,y) and size (z).
                    // If for a colour no blob has been found, its size is 0 (z=0).
                    bool detect = DetectColoursInROI(ref rawImage, COLOR_WIDTH, COLOR_HEIGHT, roi, true, 3, minHSV, maxHSV, ref blobs);
                    if (detect) {

                        bool haveLeft = GetPosition((int)blobs[0].x + roi.x, (int)blobs[0].y + roi.y, blobs[0].z, out Vector3 left);
                        bool haveRight = GetPosition((int)blobs[1].x + roi.x, (int)blobs[1].y + roi.y, blobs[1].z, out Vector3 right);
                        //TODO: Extend process to 3 points
                        //bool haveTop = GetPosition((int)blobs[2].x + roi.x, (int)blobs[2].y + roi.y, blobs[2].z, out Vector3 top);

                        if (haveLeft && haveRight) { // +haveTop
                            //orientation = GetOrientation(left, right, top);
                            orientation = GetOrientation(left, right);
                            foundOrientation = true;
                        }
                    }
                }
            }
        }

        if (foundOrientation) {
            trackingListener.NotifyChange(orientation);
        }

//////// DISPLAY - BEGIN
        texture.LoadRawTextureData(rawImage);
        texture.Apply();
//////// DISPLAY - MID
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

    //TODO: Add more checks
    bool GetPosition(int x, int y, float size, out Vector3 position) {
        position = Vector3.zero;

        //Debug.Log("IN: " + x + ", " + y + ", size: " + size);

        if (size == 0.0) {
            return false;
        }

        //TODO!
        // check position & size of blobs
        //...
        // - size "close" to 1/4 head size (?) (& >0)
        // (size is diameter in pixels)
        // ?- X & Y in ROI
        // - depth "close" to Head's Z
        //Debug.Log("HEAD Z: " + -pos.z);


        position = kinectInputManager.GetWorldPositionFromColor(x, y);
        //Debug.Log("OUT: " + position);

        return true;
    }

    //TODO: Extend process to 3 points
    // => Handle 3rd point & compute other angles
    // BUT: Orange colour not reliable (too close to skin...!?)
    Vector3 GetOrientation(Vector3 left, Vector3 right) {
        Vector3 delta = left - right;
        //Debug.Log("DELTA: " + delta.x + ", " + delta.y + ", " + delta.z);

        float angle = Vector3.SignedAngle(delta, Vector3.forward, Vector3.up) - 90.0f;
        //Debug.Log("YAW: " + angle);

        return new Vector3(.0f, angle, .0f);
    }


    [DllImport("UnityOpenCV", CallingConvention = CallingConvention.Cdecl)]
    static extern bool DetectColoursInROI(ref byte[] ppRaw, int width, int height, RectInt region, bool modifyImage, int numberColours, Scalar[] pMinHSV, Scalar[] pMaxHSV, ref Vector3[] ppBlobs);

}
