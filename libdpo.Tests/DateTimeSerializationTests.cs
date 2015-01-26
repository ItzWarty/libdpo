using System;
using System.IO;
using System.Linq;
using System.Text;
using ItzWarty;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class DateTimeSerializationTests : NMockitoInstance {
      [Fact]
      public void Run() {
         var nowUtc = DateTime.UtcNow;
         var nowLocal = DateTime.Now;
         Console.WriteLine(nowUtc);
         Console.WriteLine(nowLocal);

         var context = new PofContext().With(x => x.RegisterPortableObjectType(0, typeof(DummyType)));
         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, new DummyType(nowUtc, new []{ nowUtc, nowUtc, nowUtc }));
               serializer.Serialize(writer, new DummyType(nowLocal, new[] { nowUtc, nowUtc, nowUtc }));
            }
            ms.Position = 0;
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               var nowUtcWrapper = serializer.Deserialize<DummyType>(reader);
               var nowLocalWrapper = serializer.Deserialize<DummyType>(reader);
               Console.WriteLine(nowUtcWrapper.Time);
               Console.WriteLine(nowLocalWrapper.Time);

               AssertEquals(nowUtc.ToUniversalTime(), nowUtcWrapper.Time.ToUniversalTime());
               AssertEquals(nowUtc.ToUniversalTime(), nowLocalWrapper.Time.ToUniversalTime());

               AssertEquals(nowUtc.ToUniversalTime(), nowUtcWrapper.AlsoTime.ToUniversalTime());
               AssertEquals(nowUtc.ToUniversalTime(), nowLocalWrapper.AlsoTime.ToUniversalTime());

               AssertTrue(nowUtcWrapper.Times.All(time => time.ToUniversalTime().Equals(nowUtc.ToUniversalTime())));
               AssertTrue(nowLocalWrapper.Times.All(time => time.ToUniversalTime().Equals(nowUtc.ToUniversalTime())));
            }
         }
      }

      private class DummyType : IPortableObject {
         private object alsoTime;
         private DateTime time;
         private DateTime[] times;

         public DummyType() { }

         public DummyType(DateTime time, DateTime[] times) {
            this.alsoTime = time;
            this.time = time;
            this.times = times;
         }

         public DateTime AlsoTime { get { return (DateTime)alsoTime; } }
         public DateTime Time { get { return time; } }
         public DateTime[] Times { get { return times; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteObject(0, alsoTime);
            writer.WriteDateTime(1, time);
            writer.WriteCollection(2, times);
         }

         public void Deserialize(IPofReader reader) {
            alsoTime = reader.ReadObject(0);
            time = reader.ReadDateTime(1);
            times = reader.ReadArray<DateTime>(2);
         }
      }
   }
}
