namespace Tiled2Unity
{
	public interface ITmxVisitor
	{
		void VisitMap(TmxMap map);

		void VisitGroupLayer(TmxGroupLayer groupLayer);

		void VisitTileLayer(TmxLayer tileLayer);

		void VisitObjectLayer(TmxObjectGroup groupLayer);

		void VisitObject(TmxObject obj);
	}
}
