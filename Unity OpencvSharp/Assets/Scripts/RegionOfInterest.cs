using OpenCvSharp;
using System.Collections;

public class RegionOfInterest
{
	Rect roi;
	Mat data;
	Scalar average;

	public RegionOfInterest(Point start, int size) {
		this.roi = new Rect(start.X, start.Y, size, size);
		average = new Scalar(-1);
	}

	public void draw(Mat image, Scalar color) {
		data = image.SubMat(roi).CvtColor(ColorConversionCodes.BGR2HLS);
		image.Rectangle(roi, color);
	}

	public Scalar getColor() {
		if (average.Val0 != -1) {
			return average;
		}
		MatOfByte3 mat3 = new MatOfByte3(data);
		var indexer = mat3.GetIndexer();

		ArrayList hue = new ArrayList();
		ArrayList light = new ArrayList();
		ArrayList sat = new ArrayList();
		for (int y = 0; y < data.Height; y++){
			for (int x = 0; x < data.Width; x++){
				Vec3b color = indexer[y, x];
				hue.Add(color.Item0);
				light.Add(color.Item1);
				sat.Add(color.Item2);
			}
		}
		byte medHue = getMedian(hue);
		byte medLight = getMedian(light);
		byte medSat = getMedian(sat);
		average = new Scalar(medHue, medLight, medSat);
		return average;
	}

	byte getMedian(ArrayList array) {
		array.Sort();
		int count = array.Count;
		if (count % 2 == 0){
			byte a = (byte) array[count / 2 - 1];
			byte b = (byte) array[count / 2];
			return (byte) ((a + b) / 2);
		} else {
			return (byte) array[count / 2];
		}
	}

}