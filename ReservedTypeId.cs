using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.PortableObjects
{
   public enum ReservedTypeId : int
   {
      TYPE_S8 = -1,
      TYPE_U8 = -2,
      TYPE_S16 = -3,
      TYPE_U16 = -4,
      TYPE_S32 = -5,
      TYPE_U32 = -6,
      TYPE_S64 = -7,
      TYPE_U64 = -8,
      TYPE_FLOAT = -9,
      TYPE_DOUBLE = -10,
      TYPE_CHAR = -11,
      TYPE_STRING = -12,
      TYPE_BOOL = -13,
      TYPE_GUID = -14,
      TYPE_NULL = -15,
      TYPE_OBJECT = -16,
      TYPE_ENUMERABLE = -17,
      TYPE_DATETIME = -18,
      TYPE_BYTES = -19,

      /// <summary>
      /// Portable arrays are used to transit array data.
      /// </summary>
      TYPE_PORTABLE_ARRAY = -20,
      TYPE_PORTABLE_MAP = -21,

      /// <summary>
      /// Array type is used to properly serialize array generic type parameters.
      /// e.g. Dictionary of key Int32 and Value Float[] has pof type descriptor
      /// PortableDictionary { Int32, Array of Float }.
      /// 
      /// Regardless, actual array instances are transited over as Portable Arrays. 
      /// </summary>
      TYPE_ARRAY = -22,
      TYPE_TIMESPAN = -23
   }
}
