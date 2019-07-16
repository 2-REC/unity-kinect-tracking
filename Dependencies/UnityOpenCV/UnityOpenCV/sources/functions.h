#ifndef FUNCTIONS_H
#define FUNCTIONS_H

#include "opencv2/opencv.hpp"

std::vector<cv::Mat> ExtractColorMasks(const cv::Mat& imageHSV, const int& numberColours, cv::Scalar *pMinHSV, cv::Scalar *pMaxHSV);

std::vector<std::vector<cv::KeyPoint>> ExtractBlobs(const std::vector<cv::Mat>& masks);

cv::KeyPoint GetBlob(const std::vector<cv::KeyPoint>& keypoints);

#endif // FUNCTIONS_H
