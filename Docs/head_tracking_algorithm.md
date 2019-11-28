# KINECT HEAD TRACKING

## INTRODUCTION

Tracking of colour balls attached to a headset, to determine the orientation of the head.<br>
Kinect colour, depth and body tracking is combined to determine the positions of the detected colour balls, and from them extract the head orientation.


## ALGORITHM

### 1. Get Head ROI
=> Unity

Inputs:
- HEAD joint in Body frames

Process:
- Map Body data to RGB colour space
- Determine ROI (rectangle) around Head's position (from inputs and distance to camera)

Outputs:
- ROI (rectangle)


### 2. Filter Body Data
=> Unity (OPTIONAL)

Inputs:
- Body Index frame
- ROI

Process:
- Map Depth data (& thus Body Index data) to colour space
- Apply Body Index mask to ROI

Outputs:
- Filtered Body Index data ROI


### 3. Extract Colours
=> OpenCV

Inputs:
- BGRA Image
    <br>=> With or without filtered Body Index data
- ROI
- Colours to extract (NbColors)

Process:
- Extract ROI from image
- Convert ROI from BGR to HSV
- For each input colour:
    - Extract colour

Outputs:
- Extracted colour masks (NbColors)


### 4. Get Blobs
=> OpenCV: For each "Extracted Colour Mask"
    <br>(Not sure about the process, but provides the position & size of the biggest blob)

Inputs:
- Extracted colour mask

Process:
- "Clean" data
    <br>=> Dilate? Erode? Else?
- Determine biggest blob position & size
    <br>=> Or should determine closest point from camera instead of centre?
    <br>! - BUT: Requires Depth data as input!

Outputs:
- Blob position (in ROI) and size (in pixels)


### 5. Blobs Position
=> Unity

Inputs:
- Blobs positions (in ROI) and sizes (in pixels) in RGB space

Process:
- For each blob:
    - Determine blob position in full RGB image
    - Map position and size to Depth space
    - Determine (x, y, z) position in world from Depth data

Outputs:
- (x, y, z) blobs positions in world space


### 6. Determine Orientation
=> OpenCV? Unity?

Inputs:
- (x, y, z) blobs positions in world space

Process:
- Determine angles
    - YAW, PITCH & ROLL: From a 3D model of the trackers
      <br>=> Pose estimation.
<br>OR (simpler, need to check if results are satisfactory)
    - YAW: From delta-Z between the "side" blobs
        <br>! - Need to consider X offset from centre of Kinect - (though it is probably already managed/corrected by the SDK?)
    - PITCH & ROLL: From Android's gyroscope

Outputs:
- Angles


## COLOURS

### GREEN
=> Position: Left

- General HSV:
```
H: 180-190
S: 50-100
V: 80-100
```
- In OpenCV:
```
H: 90-95 (100?)
S: 127-255
V: 127-255
```


### PINK
=> Position: Right

- General HSV:
```
H: 320-330 (340?)
    (or 330-350)
S: 50-100
V: 50-100
```
- In OpenCV:
```
H: 160-165
    (or 165-175)
S: 127-255
V: 127-255
```

### ORANGE
=> Position: Top (currently ignored)
    <br>! - Colour too "common" (too much similarities in surroundings)
    <br>=> Should try a different colour

- General HSV:
```
H: 10-30
    (or 15-30)
S: 50-100
V: 50-100
```
- In OpenCV:
```
H: 5-15
    (or 7-15)
S: 127-255
V: 127-255
```
