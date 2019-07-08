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
//TODO: check that ROI is in image!
	Mat imageROI = image(region);

	Mat imageHSV;
	Mat threshold;

	// hardcoded "pink" colour (?)
	Scalar col1(161, 155, 84);
	Scalar col2(179, 255, 255);

	// detect a colour in ROI
	cvtColor(imageROI, imageHSV, COLOR_BGR2HSV);
	inRange(imageHSV, col1, col2, threshold);
	cvtColor(threshold, imageROI, COLOR_GRAY2RGBA);
	imageROI.copyTo(image(region));
}

void DetectColourInROI(byte **raw, int width, int height, cv::Rect region, int hue1, int sat1, int val1, int hue2, int sat2, int val2) {
	using namespace cv;
	using namespace std;

	Mat image(height, width, CV_8UC4, *raw);

	// Process frame here
	// => Colour detection in ROI
	//cout << "ROI: " << region.x << ", " << region.y << ", " << region.width << ", " << region.height << endl;
	Mat imageROI = image(region);

	Mat imageHSV;
	Mat threshold;

	Scalar col1(hue1, sat1, val1);
	Scalar col2(hue2, sat2, val2);

	cvtColor(imageROI, imageHSV, COLOR_BGR2HSV);
	inRange(imageHSV, col1, col2, threshold);
	cvtColor(threshold, imageROI, COLOR_GRAY2RGBA);
	imageROI.copyTo(image(region));
}

//TODO: use "const ...& ..."?
bool FindBlobs(byte **raw, int width, int height, cv::Rect region, bool modifyImage, int numberColours, byte* hsvValues) {

// "hsvValues" must be of length "numberColours * 3" (H, S, V value for each colour)

	using namespace cv;
	using namespace std;

	bool success = false;


	Mat image(height, width, CV_8UC4, *raw);

	// Extract ROI
	//cout << "ROI: " << region.x << ", " << region.y << ", " << region.width << ", " << region.height << endl;
//TODO: check that ROI is in image!
	Mat imageROI = image(region);

	Mat imageHSV;
	Mat threshold;

//TODO: to remove, test purpose
//addWeighted(image(region), 1.0, imageROI, 0.25, 0.0, image(region));

	for (int i = 0; i < numberColours; ++i) {
//cout << "color " << i << " of " << numberColours << endl;
//cout << "1:" << (int)*(hsvValues + i*6) << ", " << (int)*(hsvValues + i*6 + 1) << ", "<< (int)*(hsvValues + i*6 + 2) << endl;
//cout << "2:" << (int)*(hsvValues + i*6 + 3) << ", " << (int)*(hsvValues + i*6 + 4) << ", " << (int)*(hsvValues + i*6 + 5) << endl;

//TODO: should be set before, and kept instead of recomputed for every frame
//=> Good for testing though
		// Set HSV colour
		byte* offset = (hsvValues + i * 6);
		Scalar col1((int)*offset, (int)*(offset + 1), (int)*(offset + 2));
		Scalar col2((int)*(offset + 3), (int)*(offset + 4), (int)*(offset + 5));

		// Detect HSV colour
		cvtColor(imageROI, imageHSV, COLOR_BGR2HSV);
		inRange(imageHSV, col1, col2, threshold);

//TODO: Do something else with found data
//=> Continue process
Mat tmp;
cvtColor(threshold, tmp, COLOR_GRAY2RGBA);
add(image(region), tmp, image(region));
	}

//TODO: return true; (check found data?)
	return success;
}


#ifdef __cplusplus
}
#endif
