using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class OpenCv : MonoBehaviour
{
	public static readonly HersheyFonts FONT = HersheyFonts.HersheyDuplex;
	public static readonly int WAIT_TIME_MS = 10;
	public static readonly int SPACE_KEY = 32;
	public static readonly int P_KEY = 112;


	UnityEngine.XR.WSA.WebCam.VideoCapture capture;
	HandColorProfile handColor;
	Window windowCap = new Window("capture");
	Window windowRange = new Window("range");
	int count = 0;

	void Start() {
		handColor = new HandColorProfile();
		capture = new UnityEngine.XR.WSA.WebCam.VideoCapture(0);
		if (capture.IsOpened() == false) {
			print("Cannot open video camera");
		} else {
			print("Camera started");
		}
		StartCoroutine("cv");
	}

	IEnumerator cv() {
		Mat image = new Mat();
		while (true)
		{
			capture.Read(image);
			if (image.Empty())
				break;

			checkKeyPress();
			Mat flip = image.Flip(FlipMode.Y);
			showCamera(flip);
			yield return null;
		}
		yield return null;
	}

	void checkKeyPress() {
		int pressedKey = Window.WaitKey(WAIT_TIME_MS);
		if (pressedKey > 0) print("pressed: " + pressedKey);
		if (pressedKey == SPACE_KEY) {
			count =0;
			handColor.setHasColor(true);
		} else if (pressedKey == P_KEY) {
			Window.DestroyAllWindows();
			handColor.setHasColor(false);
		}
	}

	void showCamera(Mat image) {
		count++;
		if (!handColor.getHasColor()) {
			handColor.drawRegionMarker(image);
		} else {
			showMask(image);
		}
		image.PutText("count:" + count,
						new Point(50,10), OpenCv.FONT, 0.5, Scalar.Blue);

		windowCap.ShowImage(image);
	}

	void showMask(Mat image) {
		Mat mask = handColor.getMask(image);
		windowRange.ShowImage(mask);
	}

	void OnApplicationQuit() {
		Window.DestroyAllWindows();
	}

}