using OpenCvSharp;
using System.Collections;
using System;

public class HandFeatureExtraction {

	Point[] contour;
	Point[] hull;
	Mat mask;

	public Mat getMask() {
		return mask;
	}

	public HandFeatureExtraction() {

	}

	public void extract(Mat image, HandColorProfile handColor) {
		if (!handColor.getHasColor()) {
			return;
		}
		mask = handColor.getMask(image);
		contour = getContours(mask);
		hull = getConvexHull(contour);
	}

	public Point[] getContours(Mat image) {
		HierarchyIndex[] hierarchyIndexes;
		Point[][] contours;
		Cv2.FindContours(
			image,
			out contours,
			out hierarchyIndexes,
			RetrievalModes.External,
			ContourApproximationModes.ApproxNone);

		if (contours.Length == 0) {
			return null;
		}

		// Find biggest contours
		var contourIndex = 0;
		var previousArea = 0;
		var biggestContourRect = Cv2.BoundingRect(contours[0]);
		var biggestContourIndex = 0;
		while ((contourIndex >= 0)) {
			var contour = contours[contourIndex];

			var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
			var boundingRectArea = boundingRect.Width * boundingRect.Height;
			if (boundingRectArea > previousArea)
			{
				biggestContourRect = boundingRect;
				previousArea = boundingRectArea;
				biggestContourIndex = contourIndex;
			}

			contourIndex = hierarchyIndexes[contourIndex].Next;
		}
		return contours[biggestContourIndex];
	}

	public Mat drawContour(Mat image, Point[] contour, Scalar color) {
		Point[][] contours = new Point[1][];
		contours[0] = contour;
		Cv2.DrawContours(image, contours, 0,
			color: color,
			thickness: 4,
			lineType: LineTypes.Link8,
			maxLevel: int.MaxValue);
		return image;
	}

	public Mat getFeature(Mat image) {
		image = drawContour(image, contour, Scalar.Blue);
		image = drawContour(image, hull, Scalar.Red);
		return image;
	}

	public Point[] getConvexHull(Point[] contour) {
		return Cv2.ConvexHull(contour, false);
	}

	public float getHandArea() {
		if (contour == null) {
			return 0;
		}
		var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
		return boundingRect.Width * boundingRect.Height;
	}
}