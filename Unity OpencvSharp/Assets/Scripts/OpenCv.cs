using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class OpenCv : MonoBehaviour
{

	void Start() {
		StartCoroutine("opencv");
	}

	IEnumerator opencv() {
		VideoCapture capture = new VideoCapture(0);
		if (capture.IsOpened() == false)
		{
			print("Cannot open video camera");
		} else {
			print("Camera started");
		}

		using (Window window = new Window("capture"))
		using (Mat image = new Mat()) // Frame image buffer
		{
			// When the movie playback reaches end, Mat.data becomes NULL.
			while (true)
			{
				capture.Read(image); // same as cvQueryFrame
				if (image.Empty())
					break;

				window.ShowImage(image);
				yield return null;
			}
		}
		yield return null;
	}


}