﻿using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class HeadTracking : MonoBehaviour {

    const int COLOR_WIDTH = KinectInputManager.COLOR_WIDTH;
    const int COLOR_HEIGHT = KinectInputManager.COLOR_HEIGHT;

    public int roiWidthFactor = 2; // twice the head width
    public int roiHeightFactor = 2; // twice the head height
//TODO: change names!?
    public Vector3Int HSVGreenMin = new Vector3Int(90, 127, 127);
    public Vector3Int HSVGreenMax = new Vector3Int(95, 255, 255);
    public Vector3Int HSVPinkMin = new Vector3Int(165, 127, 127);
    public Vector3Int HSVPinkMax = new Vector3Int(175, 255, 255);
    public Vector3Int HSVOrangeMin = new Vector3Int(7, 127, 127);
    public Vector3Int HSVOrangeMax = new Vector3Int(20, 255, 255);

    public KinectInputManager kinectInputManager;
    public BodyTracker2D bodyTracker;
    public Image image;

    Texture2D texture;
    byte[] hsvValues;


    void Awake() {
        texture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//        texture = new Texture2D(KinectInputManager.DEPTH_WIDTH, KinectInputManager.DEPTH_HEIGHT, TextureFormat.Alpha8, false);

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
    }

    void Update() {

//TODO: do it once have found the head (only here for display purpose)
        var rawImage = kinectInputManager.GetColorBuffer();

        ulong id = bodyTracker.GetTrackedId(1);
        if (id != 0) {

            Vector2 position = bodyTracker.GetPosition(id, JointType.Head);
            if (position != Vector2.zero) {
                if ((position.x < COLOR_WIDTH) && (position.y < COLOR_HEIGHT)) {
//TODO: determine using the depth value OR body index data (dependent on distance!)
int roiWidth = 200 * roiWidthFactor;
int roiHeight = 200 * roiHeightFactor;

//TODO: use "min" & "max" methods directly
                    int x = (int)position.x - roiWidth / 2;
                    int y = (int)position.y - roiHeight / 2;
                    int w = roiWidth;
                    int h = roiHeight;

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if ((x + w) >= COLOR_WIDTH) w = COLOR_WIDTH - 1 - x;
                    if ((y + h) >= COLOR_HEIGHT) h = COLOR_HEIGHT - 1 - y;

                    RectInt roi = new RectInt(x, y, w, h);

//                    var rawImage = kinectInputManager.GetColorBuffer();

////////
//TODO: TO REMOVE!
//=> Here only for testing purpose
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

                    // continue process
                    //...
                }
            }
        }

//TODO: don't need that (only here for display purpose)
        texture.LoadRawTextureData(rawImage);
        texture.Apply();
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

}
