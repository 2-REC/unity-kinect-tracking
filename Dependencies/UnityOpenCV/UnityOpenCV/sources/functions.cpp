#include "functions.h"

using namespace std;
using namespace cv;

SimpleBlobDetector::Params InitDetectorParameters();


vector<Mat> ExtractColorMasks(const Mat& imageHSV, const int& numberColours, Scalar *pMinHSV, Scalar *pMaxHSV) {
	vector<Mat> blobs;
	for (int i = 0; i < numberColours; ++i) {
		Mat threshold;
		inRange(imageHSV, pMinHSV[i], pMaxHSV[i], threshold);
		blobs.push_back(threshold);
	}
	return blobs;
}

//TODO: rename?
vector<vector<KeyPoint>> ExtractBlobs(const vector<Mat>& masks) {

	SimpleBlobDetector::Params detectorParams = InitDetectorParameters();
	Ptr<SimpleBlobDetector> detector = SimpleBlobDetector::create(detectorParams);

	vector<vector<KeyPoint>> masksKeyPoints;
	for (vector<Mat>::const_iterator maskIterator = masks.begin(); maskIterator != masks.end(); ++maskIterator) {
		vector<KeyPoint> keypoints;
		detector->detect(*maskIterator, keypoints);

		masksKeyPoints.push_back(keypoints);
	}

	return masksKeyPoints;
}

//TODO: rename?
KeyPoint GetBlob(const vector<KeyPoint>& keypoints) {

//cout << "points: " << keypoints.size() << endl;
	float sizeMax = 0.0;
	KeyPoint keyPoint;
	if (!keypoints.empty()) {
//TODO: should have an "ideal" size (determined from size of real objects and distance from camera)
//=> or simply discard in Unity if not satisfiable
		// get biggest blob
		for (vector<KeyPoint>::const_iterator pointIterator = keypoints.begin(); pointIterator != keypoints.end(); ++pointIterator) {
			const float blobSize = (*pointIterator).size;
//cout << "octave: " << (*pointIterator).octave << endl;
			if (blobSize > sizeMax) {
				sizeMax = blobSize;
//TODO: need to explicitly make a copy?
				keyPoint = *pointIterator;
			}
		}
	}

	if (sizeMax == 0.0) {
		// no key point
		keyPoint.pt.x = 0.0;
		keyPoint.pt.y = 0.0;
		keyPoint.size = 0.0;
	}

	return keyPoint;
}


SimpleBlobDetector::Params InitDetectorParameters() {
	SimpleBlobDetector::Params detectorParams;

//TODO: not sure about this...
	detectorParams.minThreshold = 128; //OK?
	detectorParams.maxThreshold = 130; //OK?
	detectorParams.thresholdStep = 1; //OK? (any)

	detectorParams.filterByColor = 1;
	detectorParams.blobColor = 255;

//TODO: Area should be dependent on input (distance from camera)
	detectorParams.filterByArea = true;
	detectorParams.minArea = 20;

	detectorParams.filterByCircularity = false;
/*
	detectorParams.filterByCircularity = true;
	detectorParams.minCircularity = 0.1;
*/

	detectorParams.filterByConvexity = false;
/*
	detectorParams.filterByConvexity = true;
	detectorParams.minConvexity = 0.87;
*/

	detectorParams.filterByInertia = false;
/*
	detectorParams.filterByInertia = true;
	detectorParams.minInertiaRatio = 0.01;
*/

	return detectorParams;
}
