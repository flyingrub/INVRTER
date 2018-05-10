using UnityEngine;
using OpenCvSharp;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;



public class HandFeatureExtraction : MonoBehaviour {

	Point[] contour;
	IndexedPoint[] hull;
	IEnumerable<HullDefectVertice> hullDefectVertices;
	Mat mask;
	List<int> fingerNumbers = new List<int>();

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
		hullDefectVertices = filterVertices(getDefects());
	}

	public Mat getFeature(Mat image) {
		image = drawContour(image, contour, Scalar.Blue);
		image = drawPoints(image, getFingerPoints(), Scalar.Blue);
		image = drawPoints(image, getDefectPoints(), Scalar.Red);
		image = drawContour(image, getHullPoints(), Scalar.Red);
		countFinger();
		image.PutText("Finger Number :" + getFingerNumber(),
						new Point(20,20), OpenCv.FONT, 0.5, Scalar.White);
		image.PutText("Hand Direction :" + getDirection(),
						new Point(20,50), OpenCv.FONT, 0.5, Scalar.White);
		return image;
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

	public Mat drawPoints(Mat image, Point[] points, Scalar color) {
		foreach (Point p in points) {
			image.Circle(p, 10, color, thickness: 4);
		}
		return image;
	}

	public int[] getHullIndices() {
		return hull.Select((iPoint) => iPoint.indice).ToArray();
	}

	public Point[] getHullPoints() {
		return hull.Select((iPoint) => iPoint.p).ToArray();
	}

	public Point[] getFingerPoints() {
		return hullDefectVertices.Select((d) => d.pt).ToArray();
	}

	public Point[] getDefectPoints() {
		return hullDefectVertices.Select((d) => d.d1).ToArray();
	}

	public void countFinger() {
		// fingerNumbers.ForEach((v) => { print(v); });
		if (fingerNumbers.Count > 20) {
			fingerNumbers.Remove(19);
		}
		fingerNumbers.Add(getCurrentFingerNumber());
	}

	private int getCurrentFingerNumber() {
		return hullDefectVertices.Count();
	}

	public int getFingerNumber() {
		if (fingerNumbers.Count<15) {
			return 1;
		}
		return fingerNumbers.GroupBy(x => x)
						  .OrderByDescending(x => x.Count())
						  .First().Key;
	}

	public float getHandArea() {
		if (contour == null) {
			return 0;
		}
		var boundingRect = Cv2.BoundingRect(contour);
		return boundingRect.Width * boundingRect.Height;
	}

	public Direction getDirection() {
		var boundingRect = Cv2.BoundingRect(contour);
		if (boundingRect.Height > boundingRect.Width) {
			return Direction.UP;
		} else if (hullDefectVertices.Count() > 3) {
			Point a = hullDefectVertices.ElementAt(3).pt;
			Point b = hullDefectVertices.ElementAt(3).d1;
			if (isLeft(a, b)) {
				return Direction.LEFT;
			} else {
				return Direction.RIGHT;
			}
		} else {
			return Direction.UP;
		}
	}

	public bool isLeft(Point a, Point b) {
		return (a.X * b.Y - a.Y * b.X) < 0;
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

	public IndexedPoint[] getConvexHull(Point[] contour) {
		var hullIndices = Cv2.ConvexHullIndices(contour, false);
		return removeTooClose(hullIndices).ToArray();
	}

	public IEnumerable<IndexedPoint> removeTooClose(int[] hullIndices) {
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

	public IEnumerable<HullDefectVertice> getDefects() {
		int[] hullIndices = getHullIndices();
		var defects = Cv2.ConvexityDefects(contour, hullIndices);
		var hullPointDefectNeighbors = hullIndices.ToDictionary(indices => indices, i => new List<int>());
		foreach(var defect in defects) {
			int startPoint = defect.Item0;
			int endPoint = defect.Item1;
			int defectPoint = defect.Item2;
			hullPointDefectNeighbors[startPoint].Add(defectPoint);
			hullPointDefectNeighbors[endPoint].Add(defectPoint);
		}

		return hullIndices
			.Where((indice) => hullPointDefectNeighbors[indice].Count() > 1)
			.Select((indice) => {
				var defectNeighbor = hullPointDefectNeighbors[indice];
				return new HullDefectVertice {
					pt = contour[indice],
					d1 = contour[defectNeighbor[0]],
					d2 = contour[defectNeighbor[1]]
				};
			}
		);
	}

	public IEnumerable<HullDefectVertice> filterVertices(IEnumerable<HullDefectVertice> hullVertices) {
		return hullVertices.Where((v) => {
			double a = v.d1.DistanceTo(v.d2);
			double b = v.pt.DistanceTo(v.d1);
			double c = v.pt.DistanceTo(v.d2);
			double angle = Math.Acos(((Math.Pow(b, 2) + Math.Pow(c, 2)) - Math.Pow(a, 2)) / (2 * b * c)) * (180 / Math.PI);
			return angle < 60;
		});
	}

}

public enum Direction
{
	LEFT, RIGHT, UP
}

public class HullDefectVertice {
	public Point pt, d1, d2;
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