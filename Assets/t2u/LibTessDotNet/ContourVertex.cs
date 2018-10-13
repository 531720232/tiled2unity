namespace LibTessDotNet
{
	public struct ContourVertex
	{
		public Vec3 Position;

		public object Data;

		public override string ToString()
		{
			return $"{Position}, {Data}";
		}
	}
}
