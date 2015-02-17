using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class CustomActivatorTests : NMockitoInstance {
      [Mock] private readonly DummyRemoteService dummyRemoteService = null;

      [Fact]
      public void Run() {
         var context = new PofContext();
         context.MergeContext(new CustomPofContext(1000, dummyRemoteService));

         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, new LogicController(10));
               serializer.Serialize(writer, new LogicController(20));
            }
            ms.Position = 0;
            VerifyNoMoreInteractions();
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               var instance1 = serializer.Deserialize<LogicController>(reader);
               var instance2 = serializer.Deserialize<LogicController>(reader);
               VerifyNoMoreInteractions();
               
               instance1.Execute();
               Verify(dummyRemoteService, Once()).DoSomething(10);
               VerifyNoMoreInteractions();

               instance2.Execute();
               Verify(dummyRemoteService, Once()).DoSomething(20);
               VerifyNoMoreInteractions();
            }
         }
      }

      public class LogicController : IPortableObject {
         private readonly DummyRemoteService dummyRemoteService;
         private int value;

         public LogicController(DummyRemoteService dummyRemoteService) {
            this.dummyRemoteService = dummyRemoteService;
         }

         public LogicController(int value) {
            this.value = value;
         }

         public void Execute() {
            dummyRemoteService.DoSomething(value);
         }

         public void Serialize(IPofWriter writer) {
            writer.WriteS32(0, value);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadS32(0);
         }
      }

      public interface DummyRemoteService {
         void DoSomething(int value);
      }

      public class CustomPofContext : PofContext {
         public CustomPofContext(int basePofId, DummyRemoteService dummyRemoteService) {
            RegisterPortableObjectType(basePofId + 1, () => new LogicController(dummyRemoteService));
         }
      }
   }

}
