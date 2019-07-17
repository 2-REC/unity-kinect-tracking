#ifndef UNITY_OPENCV_H
#define UNITY_OPENCV_H

typedef unsigned char byte;
#include "opencv2/opencv.hpp"

#ifdef __cplusplus
extern "C" {
#endif

	/**
	 * Process on ROI in image
	 * Basic example: Colour detection (hardcoded colour)
	 * => Modifies the original image
	 */
	__declspec(dllexport) void ProcessImageRegion(byte **ppRaw, int width, int height, cv::Rect roi);


	/**
	 * Detects several colours in ROI
	 * => Can specify if modifies the original image
	 */
	__declspec(dllexport) bool DetectColoursInROI(byte **ppRaw, int width, int height, cv::Rect region, bool modifyImage, int numberColours, cv::Scalar *pMinHSV, cv::Scalar *pMaxHSV, cv::Point3f **ppBlobs);

#ifdef __cplusplus
}
#endif

#endif // UNITY_OPENCV_H
