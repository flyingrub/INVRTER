using UnityEngine;
using OpenCvSharp;
using System.Collections;
using System;

public class OpenCv : MonoBehaviour
{
	public static readonly HersheyFonts FONT = HersheyFonts.HersheyDuplex;

	VideoCapture capture;
	Window windowCap;

	HandColorProfile handColor;
	HandFeatureExtraction handGesture;

	private Rigidbody ball;

	void hueRangeChange(int val) {
		HandColorProfile.HUE_RANGE = val;
	}

	void lightRangeChange(int val) {
		HandColorProfile.LIGHT_RANGE = val;
	}

	void satRangeChange(int val) {
		HandColorProfile.SATURATION_RANGE = val;
	}

	void Start() {
		ball = GetComponent<Rigidbody>();
		handColor = new HandColorProfile();
		handGesture = new HandFeatureExtraction();
		capture = new VideoCapture(0);
		windowCap = new Window("capture");
		// windowCap.CreateTrackbar("Hue range", HandColorProfile.HUE_RANGE, 50, this.hueRangeChange);
		// windowCap.CreateTrackbar("Light range", HandColorProfile.LIGHT_RANGE, 60, this.lightRangeChange);
		// windowCap.CreateTrackbar("Saturation range", HandColorProfile.SATURATION_RANGE, 160, this.satRangeChange);
		if (capture.IsOpened() == false) {
			print("Cannot open video camera");
		} else {
			print("Camera started");
		}
		StartCoroutine("cv");
	}

	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		float m = handGesture.getHandArea() / 110000;
		Vector3 movementHand = new Vector3 (10, m, 15);
		transform.position = movementHand;
		gameObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(handGesture.getFingerNumber()  * 0.2f, 1, 1);

		// Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		// ball.AddForce(movement * 4);
	}

	IEnumerator cv() {
		Mat image = new Mat();
		while (true)
		{
			capture.Read(image);
			if (image.Empty())
				break;

			checkKeyPress();
			image = image.Flip(FlipMode.Y);
			handColor.drawRegionMarker(image);
			handGesture.extract(image, handColor);
			showCamera(image);
			yield return null;
		}
		yield return null;
	}

	void checkKeyPress() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			handColor.setHasColor(true);
		} else if (Input.GetKeyDown(KeyCode.P)) {
			handColor.setHasColor(false);
		}
	}

	void showCamera(Mat image) {
		if (handColor.getHasColor()) {
			image = handGesture.getFeature(image);
		}
		if (Input.GetKeyDown(KeyCode.Return)) {
			DateTime date = System.DateTime.Now;
			Cv2.ImWrite(date.Ticks + "", image);
		}
		windowCap.ThrowIfDisposed();
		windowCap.ShowImage(image);
	}

	void OnApplicationQuit() {
		windowCap.Dispose();
		Window.DestroyAllWindows();
	}

}