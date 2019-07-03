#include "unity_opencv.h"

#include "opencv2/opencv.hpp"

#ifdef __cplusplus
extern "C" {
#endif

void ProcessImage(byte **raw, int width, int height) {
    using namespace cv;
    using namespace std;

	Mat image(height, width, CV_8UC4, *raw);

    // Process frame here
	// => Simple example: Edge detection
	Mat edges;
	Canny(image, edges, 50, 200);
	cvtColor(edges, edges, COLOR_GRAY2RGBA);
	multiply(image, edges, image);
}

void ProcessImageRegion(byte **raw, int width, int height, cv::Rect region) {
    using namespace cv;
    using namespace std;

	Mat image(height, width, CV_8UC4, *raw);

	// Process frame here
	// => Colour detection in ROI
	//cout << "ROI: " << region.x << ", " << region.y << ", " << region.width << ", " << region.height << endl;
	Mat imageROI = image(region);

	Mat imageHSV;
	Mat threshold;

	// hardcoded "light blue" colour
	Scalar col1(106, 60, 90);
	Scalar col2(124, 255, 255);

	cvtColor(imageROI, imageHSV, COLOR_BGR2HSV);
	inRange(imageHSV, col1, col2, threshold);
	cvtColor(threshold, imageROI, COLOR_GRAY2RGBA);
	imageROI.copyTo(image(region));
}

//TODO: "GetColourBlobs(List<Colors>...)"
/*
TODO!
=> From:
https://pysource.com/2019/02/15/detecting-colors-hsv-color-space-opencv-with-python/

previous based on:
https://stackoverflow.com/questions/19189482/color-detection-in-opencv
look at:
- info about colours in opencv (& HSV)
	http://www.shervinemami.info/colorConversion.html
- pick HSV value from pixel on screen:
    https://pastebin.com/EHz2a0YP

# Red color
	low_red = np.array([161, 155, 84])
	high_red = np.array([179, 255, 255])
	red_mask = cv2.inRange(hsv_frame, low_red, high_red)
	red = cv2.bitwise_and(frame, frame, mask=red_mask)

	# Blue color
	low_blue = np.array([94, 80, 2])
	high_blue = np.array([126, 255, 255])
	blue_mask = cv2.inRange(hsv_frame, low_blue, high_blue)
	blue = cv2.bitwise_and(frame, frame, mask=blue_mask)

	# Green color
	low_green = np.array([25, 52, 72])
	high_green = np.array([102, 255, 255])
	green_mask = cv2.inRange(hsv_frame, low_green, high_green)
	green = cv2.bitwise_and(frame, frame, mask=green_mask)

	# Every color except white
	low = np.array([0, 42, 0])
	high = np.array([179, 255, 255])
	mask = cv2.inRange(hsv_frame, low, high)
	result = cv2.bitwise_and(frame, frame, mask=mask)
*/
/*
void ProcessImageRegion(byte **raw, int width, int height, cv::Rect region) {
    using namespace cv;
    using namespace std;

	Mat image(height, width, CV_8UC4, *raw);

	//cout << "ROI: " << region.x << ", " << region.y << ", " << region.width << ", " << region.height << endl;
	Mat imageROI = image(region);

	Mat imageHSV;
	Mat threshold;

	cvtColor(imageROI, imageHSV, COLOR_BGR2HSV);
	inRange(imageHSV, Scalar(106, 60, 90), Scalar(124, 255, 255), threshold);

	cvtColor(threshold, imageROI, COLOR_GRAY2RGBA);

//TODO: do something else with found data...
	imageROI.copyTo(image(region));
}
*/

#ifdef __cplusplus
}
#endif
