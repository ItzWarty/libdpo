using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;

namespace Dargon.PortableObjects {
   internal class SpecialTypes {
      public interface Base {
         object Unwrap();
      }

      public class PortableArray<TElement> : Base, IPortableObject {
         private bool isPolymorphic;
         private TElement[] elements;

         public PortableArray() { }

         private PortableArray(bool isPolymorphic, TElement[] elements) {
            this.isPolymorphic = isPolymorphic;
            this.elements = elements;
         }

         public bool IsPolymorphic { get { return isPolymorphic; } }
         public TElement[] Elements { get { return elements; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteBoolean(0, isPolymorphic);
            writer.WriteS32(1, elements.Length);

            var context = writer.Context;
            var serializer = new PofSerializer(context);
            using (var elementStream = new MemoryStream()) {
               using (var elementStreamWriter = new BinaryWriter(elementStream, Encoding.UTF8, true)) {
                  var serializationFlags = isPolymorphic ? SerializationFlags.None : SerializationFlags.Typeless;
                  foreach (var element in elements) {
                     serializer.Serialize(elementStreamWriter, element, serializationFlags);
                  }
               }
               writer.WriteBytes(2, elementStream.ToArray());
            }
         }

         public void Deserialize(IPofReader reader) {
            isPolymorphic = reader.ReadBoolean(0);
            var length = reader.ReadS32(1);
            var data = reader.ReadBytes(2);

            var context = reader.Context;
            var serializer = new PofSerializer(context);
            using (var elementStream = new MemoryStream(data))
            using (var elementStreamReader = new BinaryReader(elementStream)) {
               Func<TElement> readElement = isPolymorphic ? new Func<TElement>(() => (TElement)serializer.Deserialize(elementStreamReader))
                                                          : new Func<TElement>(() => (TElement)serializer.Deserialize(elementStreamReader, SerializationFlags.Typeless, typeof(TElement)));
               elements = Util.Generate(length, i => readElement());
            }
         }

         public static PortableArray<TElement> Create(TElement[] elements) {
            var isPolymorphic = !typeof(TElement).IsValueType && elements.Any(x => x == null || x.GetType() != typeof(TElement));
            return Create(elements, isPolymorphic);
         }

         public static PortableArray<TElement> Create(TElement[] elements, bool isPolymorphic) {
            if (isPolymorphic) {
               return CreatePolymorphic(elements);
            } else {
               return CreateNonPolymorphic(elements);
            }
         }

         public static PortableArray<TElement> CreatePolymorphic(TElement[] elements) {
            return new PortableArray<TElement>(true, elements);
         }

         public static PortableArray<TElement> CreateNonPolymorphic(TElement[] elements) {
#if DEBUG
            Trace.Assert(typeof(TElement).IsValueType || elements.All(x => x != null && x.GetType() == typeof(TElement)));
#endif
            return new PortableArray<TElement>(false, elements);
         }

         public object Unwrap() {
            return this.elements;
         }
      }

      public class PortableMap<TKey, TValue> : Base, IPortableObject {
         private bool keysPolymorphic;
         private bool valuesPolymorphic;
         private IReadOnlyDictionary<TKey, TValue> items;

         public PortableMap() { }

         private PortableMap(bool keysPolymorphic, bool valuesPolymorphic, IReadOnlyDictionary<TKey, TValue> elements) {
            this.keysPolymorphic = keysPolymorphic;
            this.valuesPolymorphic = valuesPolymorphic;
            items = elements;
         }

         public bool KeysPolymorphic { get { return keysPolymorphic; } }
         public bool ValuesPolymorphic { get { return valuesPolymorphic; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteBoolean(0, keysPolymorphic);
            writer.WriteBoolean(1, valuesPolymorphic);
            writer.WriteS32(2, items.Count);

            var context = writer.Context;
            var serializer = new PofSerializer(context);
            using (var elementStream = new MemoryStream()) {
               using (var elementStreamWriter = new BinaryWriter(elementStream, Encoding.UTF8, true)) {
                  var keySerializationFlags = keysPolymorphic ? SerializationFlags.None : SerializationFlags.Typeless;
                  var valueSerializationFlags = valuesPolymorphic ? SerializationFlags.None : SerializationFlags.Typeless;
                  foreach (var kvp in items) {
                     serializer.Serialize(elementStreamWriter, kvp.Key, keySerializationFlags);
                     serializer.Serialize(elementStreamWriter, kvp.Value, valueSerializationFlags);
                  }
               }
               writer.WriteBytes(3, elementStream.ToArray());
            }
         }

         public void Deserialize(IPofReader reader) {
            keysPolymorphic = reader.ReadBoolean(0);
            valuesPolymorphic = reader.ReadBoolean(1);
            var count = reader.ReadS32(2);
            var data = reader.ReadBytes(3);

            var context = reader.Context;
            var serializer = new PofSerializer(context);
            using (var elementStream = new MemoryStream(data))
            using (var elementStreamReader = new BinaryReader(elementStream)) {
               Func<TKey> readKey = keysPolymorphic ? new Func<TKey>(() => (TKey)serializer.Deserialize(elementStreamReader))
                                                    : new Func<TKey>(() => (TKey)serializer.Deserialize(elementStreamReader, SerializationFlags.Typeless, typeof(TKey)));
               Func<TValue> readValue = keysPolymorphic ? new Func<TValue>(() => (TValue)serializer.Deserialize(elementStreamReader))
                                                        : new Func<TValue>(() => (TValue)serializer.Deserialize(elementStreamReader, SerializationFlags.Typeless, typeof(TValue)));
               var items = new Dictionary<TKey, TValue>(count);
               for (var i = 0; i < count; i++) {
                  var key = readKey();
                  var value = readValue();
                  items.Add(key, value);
               }
               this.items = items;
            }
         }

         public static PortableMap<TKey, TValue> Create(KeyValuePair<TKey, TValue>[] dictionary) {
            var keys = dictionary.Select(x => x.Key);
            var values = dictionary.Select(x => x.Value);
            var keysPolymorphic = !typeof(TKey).IsValueType && keys.Any(x => x == null || x.GetType() != typeof(TKey));
            var valuesPolymorphic = !typeof(TValue).IsValueType && values.Any(x => x == null || x.GetType() != typeof(TValue));
            return Create(dictionary, keysPolymorphic, valuesPolymorphic);
         }

         public static PortableMap<TKey, TValue> Create(KeyValuePair<TKey, TValue>[] dictionary, bool keysPolymorphic, bool valuesPolymorphic) {
            return new PortableMap<TKey, TValue>(keysPolymorphic, valuesPolymorphic, dictionary.ToDictionary(x => x.Key, x=> x.Value));  
         }

         public object Unwrap() {
            return this.items;
         }
      }
   }
}
