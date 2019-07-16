#include "unity_opencv.h"

#include "opencv2/opencv.hpp"
#include "functions.h"

using namespace cv;
using namespace std;


#ifdef __cplusplus
extern "C" {
#endif


void ProcessImage(byte **ppRaw, int width, int height) {

	Mat image(height, width, CV_8UC4, *ppRaw);

	Mat edges;
	Canny(image, edges, 50, 200);
	cvtColor(edges, edges, COLOR_GRAY2RGBA);
	multiply(image, edges, image);
}

void ProcessImageRegion(byte **ppRaw, int width, int height, cv::Rect region) {

	Mat image(height, width, CV_8UC4, *ppRaw);

	//cout << "ROI: " << region.x << ", " << region.y << ", " << region.width << ", " << region.height << endl;
//TODO: Should check that ROI is in image!
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


void ApplyMask(byte **ppRaw, int width, int height, cv::Rect region, byte* pMask, int maskWidth, int maskHeight) {

	Mat image(height, width, CV_8UC4, *ppRaw);

//TODO: Should check that ROI is in image!
	Mat imageROI = image(region);

	Mat imageMask(maskHeight, maskWidth, CV_8U, pMask);
	cvtColor(imageMask, imageMask, COLOR_GRAY2BGRA);

	Size size(width, height);

	Mat masked;
	resize(imageMask, masked, size);

	Mat maskROI = masked(region);
	bitwise_and(imageROI, maskROI, imageROI);
}


//TODO: change name
//TODO: use "const ...& ..."?
bool DetectColoursInROI(byte **ppRaw, int width, int height, Rect region, bool modifyImage, int numberColours, Scalar *pMinHSV, Scalar *pMaxHSV, Point3f **ppBlobs) {

	bool success = false;

	Mat image(height, width, CV_8UC4, *ppRaw);

	// Extract ROI
//TODO: Should check that ROI is in image!
	Mat imageROI = image(region);

	// Extract colour masks
	Mat imageHSV;
	cvtColor(imageROI, imageHSV, COLOR_BGR2HSV);
	vector<Mat> colorMasks = ExtractColorMasks(imageHSV, numberColours, pMinHSV, pMaxHSV);

//TODO: For display/debug purpose, can be removed
	if (modifyImage) {
		for (vector<Mat>::iterator it = colorMasks.begin(); it != colorMasks.end(); ++it) {
			// apply mask to image
			Mat tmp;
			cvtColor(*it, tmp, COLOR_GRAY2RGBA);
			add(image(region), tmp, image(region));
		}
	}


	// Extract blobs
	vector<vector<KeyPoint>> keypointsLists = ExtractBlobs(colorMasks);


	// Get biggest blobs
	vector<KeyPoint> blobs;
	for (vector<vector<KeyPoint>>::iterator keyPointsIterator = keypointsLists.begin(); keyPointsIterator != keypointsLists.end(); ++keyPointsIterator) {
		KeyPoint keyPoint = GetBlob(*keyPointsIterator);
		blobs.push_back(keyPoint);
	}


	// Set output sizes & positions
	for (int i = 0; i < blobs.size(); ++i) {
		KeyPoint keyPoint = blobs[i];
		Point3f pt(keyPoint.pt.x, keyPoint.pt.y, keyPoint.size);
		(*ppBlobs)[i] = pt;
		if (keyPoint.size != 0.0) {
			success = true; // have a key point
		}
	}

	return success;
}


#ifdef __cplusplus
}
#endif
