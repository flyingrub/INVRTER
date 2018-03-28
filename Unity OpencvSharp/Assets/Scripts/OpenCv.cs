using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class OpenCv : MonoBehaviour
{
	VideoCapture capture;

	void Start() {
		capture = new VideoCapture(0);
		if (capture.IsOpened() == false) {
			print("Cannot open video camera");
		} else {
			print("Camera started");
		}
		StartCoroutine("cv");
	}

	IEnumerator cv() {
		using (Window windowCap = new Window("capture"))
		using (Window windowRange = new Window("range"))
		using (Mat image = new Mat()) // Frame image buffer
		{
			// When the movie playback reaches end, Mat.data becomes NULL.
			while (true)
			{
				capture.Read(image); // same as cvQueryFrame
				if (image.Empty())
					break;

				windowCap.ShowImage(image);
				windowRange.ShowImage(handMask(image));
				yield return null;
			}
		}
		yield return null;
	}

    Scalar shinColorUpper(int hue) {
        return new Scalar(hue, (int) 0.8 * 255, (int) 0.6*255);
    }

    Scalar skinColorLower(int hue) {
        return new Scalar(hue, (int) 0.1 * 255, (int) 0.05 * 255);
    }

    Mat handMask(Mat image) {
        Mat imgHLS = image.CvtColor(ColorConversionCodes.BGR2HLS);
        Mat rangeMask = imgHLS.InRange(skinColorLower(0), shinColorUpper(15));

        Mat blurred = rangeMask.Blur(new Size(10,10));
        Mat thresholded = blurred.Threshold(200, 255, ThresholdTypes.Binary);
		return rangeMask;
    }


}