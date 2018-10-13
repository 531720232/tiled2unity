namespace Tiled2Unity
{
	internal class TmxDisplayOrderVisitor : ITmxVisitor
	{
		private int drawOrderIndex;

		private int depthBufferIndex;

		public void VisitMap(TmxMap map)
		{
		}

		public void VisitGroupLayer(TmxGroupLayer groupLayer)
		{
			groupLayer.DrawOrderIndex = drawOrderIndex;
			groupLayer.DepthBufferIndex = depthBufferIndex++;
		}

		public void VisitObject(TmxObject obj)
		{
			if (obj is TmxObjectTile)
			{
				(obj as TmxObjectTile).DrawOrderIndex = drawOrderIndex++;
			}
		}

		public void VisitObjectLayer(TmxObjectGroup objectLayer)
		{
			objectLayer.DrawOrderIndex = drawOrderIndex;
			objectLayer.DepthBufferIndex = ((objectLayer.ParentNode == null) ? depthBufferIndex++ : 0);
		}

		public void VisitTileLayer(TmxLayer tileLayer)
		{
			tileLayer.DrawOrderIndex = drawOrderIndex++;
			tileLayer.DepthBufferIndex = ((tileLayer.ParentNode == null) ? depthBufferIndex++ : 0);
		}
	}
}
