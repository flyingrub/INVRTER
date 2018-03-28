using UnityEngine;
using OpenCvSharp;
using System.Collections;

public class WebcamFeed : MonoBehaviour
{
	WebCamTexture tex;

	void Start() {
 		WebCamDevice[] devices = WebCamTexture.devices;
		for(int i = 0; i < devices.Length; i++)
		{
			print("Webcam available: " + devices[i].name);
		}

		Renderer rend = this.GetComponentInChildren<Renderer>();

		tex = new WebCamTexture(devices[0].name);
		rend.material.mainTexture = tex;
		tex.Play();
	}
}