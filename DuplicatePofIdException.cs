using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;

namespace Dargon.PortableObjects {
   public class DuplicatePofIdException : Exception {
      public DuplicatePofIdException(int pofId, Type existing, Type conflict)
         : base(GetMessage(pofId, existing, conflict)){
      }

      private static string GetMessage(int pofId, Type existing, Type conflict) {
         return "Cannot reserve pof id {0} to {1} as it is already assigned to {2}.".F(
            pofId,
            conflict.FullName,
            existing.FullName
         );
      }
   }
}
