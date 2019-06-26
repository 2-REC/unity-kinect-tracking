using UnityEngine;
using Windows.Kinect;
using System.Runtime.InteropServices;

public class KinectInputManager : MonoBehaviour {

    public const int DEPTH_WIDTH = 512;
    public const int DEPTH_HEIGHT = 424;
    public const int COLOR_WIDTH = 1920;
    public const int COLOR_HEIGHT = 1080;


    public bool useColorInput = false;
    public bool useDepthInput = false;
    public bool useBodyInput = false;
    public bool useBodyIndexInput = false;


    private KinectSensor kinectSensor;
    private CoordinateMapper coordinateMapper;
    private MultiSourceFrameReader multiSourceFrameReader;
    private DepthSpacePoint[] pDepthCoordinates;

    private byte[] pColorBuffer;
    private ushort[] pDepthBuffer;
    private byte[] pBodyIndexBuffer;
    private Body[] pBodyData;

//////// FPS - BEGIN
//TODO: remove or make optional
    private long frameCount = 0;
    private double elapsedCounter = 0.0;
    private double fps = 0.0;
//////// FPS - END

//////// TEXTURE - BEGIN
//    private Texture2D colorTexture;
//////// TEXTURE - END

//////// INIT_CHECK - BEGIN
//TODO: if have process in "Update"
//    private bool init;
//////// INIT_CHECK - END


    void Awake() {
//////// INIT_CHECK - BEGIN
//        init = false;
//////// INIT_CHECK - END

        pColorBuffer = new byte[COLOR_WIDTH * COLOR_HEIGHT * 4];
        pDepthBuffer = new ushort[DEPTH_WIDTH * DEPTH_HEIGHT];
        pBodyIndexBuffer = new byte[DEPTH_WIDTH * DEPTH_HEIGHT];
        pBodyData = null;

//////// TEXTURE - BEGIN
//        colorTexture = new Texture2D(COLOR_WIDTH, COLOR_HEIGHT, TextureFormat.BGRA32, false);
//////// TEXTURE - END

        pDepthCoordinates = new DepthSpacePoint[COLOR_WIDTH * COLOR_HEIGHT];

        InitializeDefaultSensor();
    }

    void Update() {
//////// FPS - BEGIN
        elapsedCounter +=Time.deltaTime;
        if (elapsedCounter > 1.0) {
            fps = frameCount / elapsedCounter;
            frameCount = 0;
            elapsedCounter = 0.0;
        }
//////// FPS - END

/*
//////// INIT_CHECK - BEGIN
        if (!init) {
            return;
        }
//////// INIT_CHECK - END

//TODO: nothing else to do in "Update"?
//...
*/
    }

    void OnApplicationQuit() {
        pColorBuffer = null;
        pDepthBuffer = null;
        pBodyIndexBuffer = null;
        pBodyData = null;

        if (pDepthCoordinates != null) {
            pDepthCoordinates = null;
        }

        if (multiSourceFrameReader != null) {
            multiSourceFrameReader.Dispose();
            multiSourceFrameReader = null;
        }

        if (kinectSensor != null) {
            kinectSensor.Close();
            kinectSensor = null;
        }
    }


//////// FPS - BEGIN
    Rect fpsRect = new Rect(10, 10, 200, 30);
    void OnGUI () {
        GUI.Box (fpsRect, "FPS: " + fps.ToString("0.00"));
    }
//////// FPS - END


    private void InitializeDefaultSensor() {
        kinectSensor = KinectSensor.GetDefault();
//TODO: Find solution!
        // "GetDefault" returns something even if no Kinect is connected
        if (kinectSensor == null) {
            Debug.LogError("ERROR: No Kinect found!");
            return;
        }

        coordinateMapper = kinectSensor.CoordinateMapper;

        kinectSensor.Open();
        if (!kinectSensor.IsOpen) {
            Debug.LogError("ERROR: Can't open Kinect!");
            return;
        }

        FrameSourceTypes frameSourceTypes = GetSourceTypes();
        if (frameSourceTypes == FrameSourceTypes.None) {
            Debug.LogError("ERROR: No source selected!");
            return;
        }

        multiSourceFrameReader = kinectSensor.OpenMultiSourceFrameReader(frameSourceTypes);
        multiSourceFrameReader.MultiSourceFrameArrived += MultiFrameArrived;

//////// INIT_CHECK - BEGIN
//            init = true;
//////// INIT_CHECK - END
    }

    private FrameSourceTypes GetSourceTypes() {
        FrameSourceTypes frameSourceTypes = FrameSourceTypes.None;
        if (useColorInput) {
            Debug.Log("Using Color source");
            frameSourceTypes |= FrameSourceTypes.Color;
        }
        if (useDepthInput) {
            Debug.Log("Using Depth source");
            frameSourceTypes |= FrameSourceTypes.Depth;
        }
        if (useBodyInput) {
            Debug.Log("Using Body source");
            frameSourceTypes |= FrameSourceTypes.Body;
        }
        if (useBodyIndexInput) {
            Debug.Log("Using BodyIndex source");
            frameSourceTypes |= FrameSourceTypes.BodyIndex;
        }

        return frameSourceTypes;
    }

    private void ProcessFrame() {
        var pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
        var pDepthCoordinatesData = GCHandle.Alloc(pDepthCoordinates, GCHandleType.Pinned);

        coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort),
                pDepthCoordinatesData.AddrOfPinnedObject(), (uint)pDepthCoordinates.Length);

        pDepthCoordinatesData.Free();
        pDepthData.Free();

//////// TEXTURE - BEGIN
/*
        colorTexture.LoadRawTextureData(pColorBuffer);
        colorTexture.Apply();
*/
//////// TEXTURE - END
    }

    private void MultiFrameArrived(object sender, MultiSourceFrameArrivedEventArgs args) {
        bool dataReceived = false;
        MultiSourceFrame multiSourceFrame = args.FrameReference.AcquireFrame();

        using (DepthFrame depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame()) {
            if (depthFrame != null) {
                var pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
                depthFrame.CopyFrameDataToIntPtr(pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort));
                pDepthData.Free();
                dataReceived = true;
            }
        }

        using (ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame()) {
            if (colorFrame != null) {
                var pColorData = GCHandle.Alloc(pColorBuffer, GCHandleType.Pinned);
                colorFrame.CopyConvertedFrameDataToIntPtr(pColorData.AddrOfPinnedObject(), (uint)pColorBuffer.Length, ColorImageFormat.Bgra);
                pColorData.Free();
                dataReceived = true;
            }
        }

        using (BodyIndexFrame bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame()) {
            if (bodyIndexFrame != null) {
                var pBodyIndexData = GCHandle.Alloc(pBodyIndexBuffer, GCHandleType.Pinned);
                bodyIndexFrame.CopyFrameDataToIntPtr(pBodyIndexData.AddrOfPinnedObject(), (uint)pBodyIndexBuffer.Length);
                pBodyIndexData.Free();
                dataReceived = true;
            }
        }

        using (BodyFrame bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame()) {
            if (bodyFrame != null) {
                if (pBodyData == null) {
                    pBodyData = new Body[bodyFrame.BodyCount];
                }
                bodyFrame.GetAndRefreshBodyData(pBodyData);
                dataReceived = true;
            }
        }

        if (dataReceived) {
//////// FPS - BEGIN
            frameCount++;
//////// FPS - END
            ProcessFrame();
        }
    }


    public byte[] GetColorBuffer() {
        return pColorBuffer;
    }
//////// TEXTURE - BEGIN
/*
    public Texture2D GetColorTexture() {
        return colorTexture;
    }
*/
//////// TEXTURE - END

    public ushort[] GetDepthBuffer() {
        return pDepthBuffer;
    }

    public DepthSpacePoint[] GetDepthCoordinates() {
        return pDepthCoordinates;
    }

    public byte[] GetBodyIndexBuffer() {
        return pBodyIndexBuffer;
    }

    public Body[] GetBodyData() {
        return pBodyData;
    }

}
