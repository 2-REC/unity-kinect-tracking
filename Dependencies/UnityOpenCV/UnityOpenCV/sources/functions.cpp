#include "functions.h"

using namespace std;
using namespace cv;


vector<Mat> ExtractBlobs(Mat imageHSV, int numberColours, Scalar *pMinHSV, Scalar *pMaxHSV) {
	vector<Mat> blobs;
	for (int i = 0; i < numberColours; ++i) {
		Mat threshold;
		inRange(imageHSV, pMinHSV[i], pMaxHSV[i], threshold);
		blobs.push_back(threshold);
	}
	return blobs;
}
