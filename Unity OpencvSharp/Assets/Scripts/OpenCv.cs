using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class OpenCv : MonoBehaviour
{
	public static readonly HersheyFonts FONT = HersheyFonts.HersheyDuplex;

	VideoCapture capture;
	HandColorProfile handColor;
	Window windowCap;
	Window windowRange;
	int count = 0;

	void Start() {
		handColor = new HandColorProfile();
		capture = new VideoCapture(0);
		windowCap = new Window("capture");
		windowRange = new Window("range");
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
		if (Input.GetKeyDown(KeyCode.Space)) {
			count = 0;
			handColor.setHasColor(true);
		} else if (Input.GetKeyDown(KeyCode.P)) {
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
		windowCap.Dispose();
		windowRange.Dispose();
		Window.DestroyAllWindows();
	}

}