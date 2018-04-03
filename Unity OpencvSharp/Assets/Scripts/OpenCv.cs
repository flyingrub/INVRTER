using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class OpenCv : MonoBehaviour
{
	public static HersheyFonts font = HersheyFonts.HersheyDuplex;
	public static Scalar blue = new Scalar(255,0,0);
	public static int waitTimeMs = 30;
	public static int spaceKey = 32;


	VideoCapture capture;
	HandColorProfile handColor;

	void Start() {
		handColor = new HandColorProfile();
		capture = new VideoCapture(0);
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
		int pressedKey = Window.WaitKey(waitTimeMs);
		if (pressedKey > 0) print("pressed: " + pressedKey);
		if (pressedKey == spaceKey) {
			handColor.setHasColor();
		};
	}

	void showCamera(Mat image) {
		if (!handColor.getHasColor()) {
			handColor.drawRegionMarker(image);
		} else {
			showMask(image);
		}
		using (Window windowCap = new Window("capture"))
		windowCap.ShowImage(image);
	}

	void showMask(Mat image) {
		Mat mask = handColor.getMask(image);
		using (Window windowRange = new Window("range"))
		windowRange.ShowImage(mask);
	}

	void OnApplicationQuit() {
		Window.DestroyAllWindows();
	}

}