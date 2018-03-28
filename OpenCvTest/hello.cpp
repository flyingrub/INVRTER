#include <opencv2/opencv.hpp>
#include <iostream>

using namespace cv;
using namespace std;


void equalize(Mat* frame) {
    //Convert the frame from BGR to YCrCb color space
    cvtColor(*frame, *frame, COLOR_BGR2YCrCb);

    //Split the image into 3 channels; Y, Cr and Cb channels respectively and store it in a std::vector
    vector<Mat> vec_channels;
    split(*frame, vec_channels);

    //Equalize the histogram of the Y channel 
    equalizeHist(vec_channels[0], vec_channels[0]);

    //Merge 3 channels in the std::vector to form the color image in YCrCB color space. 
    merge(vec_channels, *frame);

    //Convert the histogram equalized image from YCrCb to BGR color space again
    cvtColor(*frame, *frame, COLOR_YCrCb2BGR);
}

int main(int argc, char** argv)
{
    string window_name = "My Camera Feed";
    //Open the default video camera
    VideoCapture cap(0);
    // Read from image
    // Mat frame = imread("test.jpg");

    // if not success, exit program
    if (cap.isOpened() == false)
    {
        cout << "Cannot open the video camera" << endl;
        cin.get(); //wait for any key press
        return -1;
    }

    double dWidth = cap.get(CAP_PROP_FRAME_WIDTH); //get the width of frames of the video
    double dHeight = cap.get(CAP_PROP_FRAME_HEIGHT); //get the height of frames of the video

    cout << "Resolution of the video : " << dWidth << " x " << dHeight << endl;

    namedWindow(window_name); //create a window called "My Camera Feed"

    int brightnessValue = 50;
    createTrackbar("Brightness", window_name, &brightnessValue, 100);
    int contrastValue = 50;
    createTrackbar("Contrast", window_name, &contrastValue, 100);


    while (true) {
        Mat frame;
        bool bSuccess = cap.read(frame); // read a new frame from vide

        int iBrightness  = brightnessValue - 50;
        double dContrast = contrastValue / 50.0;
        //Breaking the while loop if the frames cannot be captured
        if (bSuccess == false)
        {
            cout << "Video camera is disconnected" << endl;
            cin.get(); //Wait for any key press
            break;
        }

        frame.convertTo(frame, -1, dContrast, iBrightness); // increase the brightness by 50
        //cvtColor(frame, frame, COLOR_BGR2GRAY); // greyScale
        //equalize(&frame);
        Mat dst = Mat::zeros(frame.size(), frame.type());
        medianBlur(frame, dst, 5);
        imshow(window_name, dst);

        //wait for for 10 ms until any key is pressed.
        //If the 'Esc' key is pressed, break the while loop.
        //If any key is not pressed withing 10 ms, continue the loop
        if (waitKey(10) == 27)
        {
            cout << "Esc key is pressed by user. Stoppig the video" << endl;
            break;
        }
    }

    return 0;
}