namespace Detector.Example.Detectors {
  public class Range {
    public Range(double minValue, double maxValue, RangePurposeMethod detectMethod = RangePurposeMethod.Hold) {
      MinValue = minValue;
      MaxValue = maxValue;
      DetectMethod = detectMethod;
    }
    public double MinValue { get; private set; }
    public double MaxValue { get; private set; }
    public RangePurposeMethod DetectMethod { get; private set; }

    public bool IsDetected(double value) {
      switch (DetectMethod) {
        case RangePurposeMethod.Lower:
          return value <= MaxValue;

        case RangePurposeMethod.Heigher:
          return value >= MinValue;

        default:
          return value >= MinValue && value <= MaxValue;
      }
    }
  }
}
