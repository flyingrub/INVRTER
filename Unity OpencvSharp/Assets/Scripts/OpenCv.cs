using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class OpenCv : MonoBehaviour
{
	public static readonly HersheyFonts FONT = HersheyFonts.HersheyDuplex;

	VideoCapture capture;
	Window windowCap;

	HandColorProfile handColor;
	HandFeatureExtraction handGesture;

	private Rigidbody ball;

	void Start() {
		ball = GetComponent<Rigidbody>();
		handColor = new HandColorProfile();
		handGesture = new HandFeatureExtraction();
		capture = new VideoCapture(0);
		windowCap = new Window("capture");
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

		float m = handGesture.getHandArea() / 220000;
		Vector3 movementHand = new Vector3 (0, m, 0);
		transform.position += movementHand;

		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		ball.AddForce(movement * 1);
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
		windowCap.ThrowIfDisposed();
		windowCap.ShowImage(image);
	}

	void OnApplicationQuit() {
		windowCap.Dispose();
		Window.DestroyAllWindows();
	}

}