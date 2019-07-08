#ifndef UNITY_OPENCV_H
#define UNITY_OPENCV_H

typedef unsigned char byte;
#include "opencv2/opencv.hpp"

#ifdef __cplusplus
extern "C" {
#endif

    __declspec(dllexport) void ProcessImage(byte **raw, int width, int height);
	__declspec(dllexport) void ProcessImageRegion(byte **raw, int width, int height, cv::Rect roi);
	__declspec(dllexport) void DetectColourInROI(byte **raw, int width, int height, cv::Rect region, int hue1, int sat1, int val1, int hue2, int sat2, int val2);
	__declspec(dllexport) bool FindBlobs(byte **raw, int width, int height, cv::Rect region, bool modifyImage, int numberColours, byte* hsvValues);
	__declspec(dllexport) void ApplyMask(byte **raw, int width, int height, cv::Rect region, byte* mask, int maskWidth, int maskHeight);

#ifdef __cplusplus
}
#endif

#endif // UNITY_OPENCV_H
