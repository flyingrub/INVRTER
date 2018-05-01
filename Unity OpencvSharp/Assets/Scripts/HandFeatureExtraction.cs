using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

public class HandFeatureExtraction {

	Point[] contour;
	IndexedPoint[] hull;
	Point[] defects;
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
		defects = getDefects();
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

	public int[] getHullIndices() {
		return hull.Select((iPoint) => iPoint.indice).ToArray();
	}

	public Point[] getHullPoints() {
		return hull.Select((iPoint) => iPoint.p).ToArray();
	}

	public Mat getFeature(Mat image) {
		image = drawContour(image, contour, Scalar.Blue);
		image = drawPoints(image, getHullPoints());
		image = drawContour(image, getHullPoints(), Scalar.Red);
		return image;
	}

	public Mat drawPoints(Mat image, Point[] points) {
		foreach (Point p in points) {
			image.Circle(p, 10, Scalar.Red);
		}
		return image;
	}

	public IndexedPoint[] getConvexHull(Point[] contour) {
		var hullIndices = Cv2.ConvexHullIndices(contour, false);
		return removeTooClose(hullIndices).ToArray();
	}

	public List<IndexedPoint> removeTooClose(int[] hullIndices) {
		List<IndexedPoint> res = new List<IndexedPoint>();
		var indexedPoints = hullIndices.Select((indice) => new IndexedPoint{ indice = indice, p = contour[indice] });
		var clusters = indexedPoints.GroupBy(i => i.p, new ClosenessComparer(40));
		foreach (var group in clusters)
		{
			var list = group.ToList();
			int median = list.Count()/2;
			res.Add(list[median]);
		}
		return res;
	}

	public Point[] getDefects() {
		int[] hullIndices = getHullIndices();
		var defects = Cv2.ConvexityDefects(contour, hullIndices);
		return null;
	}

	public float getHandArea() {
		if (contour == null) {
			return 0;
		}
		var boundingRect = Cv2.BoundingRect(contour);
		return boundingRect.Width * boundingRect.Height;
	}
}

public class IndexedPoint {
	public Point p;
	public int indice;
}

public class ClosenessComparer : IEqualityComparer<Point>
{
	private readonly int delta;

	public ClosenessComparer(int delta)
	{
		this.delta = delta;
	}

	public bool Equals(Point a, Point b)
	{
		return Point.Distance(a, b) < delta;
	}

	public int GetHashCode(Point point)
	{
		return 0;
	}
}