#ifndef FUNCTIONS_H
#define FUNCTIONS_H

#include "opencv2/opencv.hpp"

std::vector<cv::Mat> ExtractBlobs(cv::Mat imageHSV, int numberColours, cv::Scalar *pMinHSV, cv::Scalar *pMaxHSV);

#endif // FUNCTIONS_H
