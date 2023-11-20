using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos
{
    public class BizException : Exception
    {
        public BizException(ErrorCodes errorCode, params KeyValuePair<string, object>[] data)
        {
            ErrorCode = errorCode;
            Message = errorCode.ToString();
            ExtData = data.ToDictionary(x => x.Key, x => x.Value);
        }

        public BizException(ErrorCodes errorCode, string message, params KeyValuePair<string, object>[] data)
        {
            ErrorCode = errorCode;
            Message = message;
            ExtData = data.ToDictionary(x => x.Key, x => x.Value);
        }

        public ErrorCodes ErrorCode { get; }

        public override string Message { get; }

        public IDictionary<string, object> ExtData { get; }
    }



    /// <summary>
    /// error codes
    /// aaa_bbb_ccc
    /// <para>
    /// for aaa -> module_instance
    /// </para>
    /// <para>
    /// for bbb -> http code
    /// </para>
    /// for ccc -> error index
    /// </summary>
    public enum ErrorCodes
    {
        RuntimeException = 010_500_01,
        MetaDataItemNotFound = 010_404_01,
        Player_AttemptForceSignIn = 100_401_01,
        Backpack_Size_Is_Insufficient = 100_400_001,

        // backpack module errors
        Backpack_LvUpExpInsufficient = 110_400_001,
        BackPack_ItemCountOut = 110_400_02,
    }
}
