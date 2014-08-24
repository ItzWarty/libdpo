namespace Dargon.PortableObjects
{
   public interface IPortableObject
   {
      void Serialize(IPofWriter writer);
      void Deserialize(IPofReader reader);
   }
}
