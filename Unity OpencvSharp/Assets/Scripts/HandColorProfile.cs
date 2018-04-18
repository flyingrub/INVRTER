using OpenCvSharp;
using System.Collections;
using System;
using UnityEngine;


public class HandColorProfile : MonoBehaviour {
	ArrayList regions;
	bool hasColorProfile;
	int width = -1, height = -1;
	Mat mask;

	public HandColorProfile() {
		hasColorProfile = false;
		regions = new ArrayList();
	}

	public void setHasColor(bool has) {
		hasColorProfile = has;
	}

	public bool getHasColor() {
		return hasColorProfile;
	}

	void setRegions() {
		regions.Add(new RegionOfInterest(new Point(width/3,height/6), 10));
		regions.Add(new RegionOfInterest(new Point(width/4,height/2), 10));
		regions.Add(new RegionOfInterest(new Point(width/3,height/1.5), 10));
		regions.Add(new RegionOfInterest(new Point(width/2,height/2), 10));
		regions.Add(new RegionOfInterest(new Point(width/2.5,height/2.5), 10));
		regions.Add(new RegionOfInterest(new Point(width/2,height/1.5), 10));
		regions.Add(new RegionOfInterest(new Point(width/2.5,height/1.8), 10));
	}

	public void drawRegionMarker(Mat image) {
		if (hasColorProfile) {
			return;
		}
		if (height == -1) {
			width = image.Cols;
			height = image.Rows;
			setRegions();
		}

		image.PutText("Press space when your hand is ready",
						new Point(20,10), OpenCv.FONT, 0.5, Scalar.Blue);
		foreach (RegionOfInterest r in regions) {
			r.draw(image, Scalar.Blue);
		}
	}

	Scalar skinColorLower(Scalar color) {
		double hue = Math.Max(color.Val0 - 15, 0);
		double light = Math.Max(color.Val1 - 35, 0);
		double sat = Math.Max(color.Val2 - 80, 0);
		return new Scalar(hue, light, sat);
	}

	Scalar shinColorUpper(Scalar color) {
		double hue = Math.Min(color.Val0 + 15, 255);
		double light = Math.Min(color.Val1 + 35, 255);
		double sat = Math.Min(color.Val2 + 80, 255);
		return new Scalar(hue, light, sat);
	}

	public Mat getMask(Mat image) {
		image = image.MedianBlur(3);
		image = image.CvtColor(ColorConversionCodes.BGR2HLS);
		Mat res = new Mat(image.Size(), MatType.CV_8U);
		foreach (RegionOfInterest r in regions) {
			Scalar lowerColor = skinColorLower(r.getColor());
			Scalar upperColor = shinColorUpper(r.getColor());
			Mat rangeMask = image.InRange(lowerColor, upperColor);
			res += rangeMask;
		}

		res.Erode(new Mat());
		res = res.MedianBlur(7);
		return res;
	}

}