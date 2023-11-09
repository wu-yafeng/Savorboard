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

    public enum ErrorCodes
    {
        Player_AttemptForceSignIn = 100_401_01,
        Backpack_Size_Is_Insufficient = 100_400_001,
        BackPack_Item_Count_Is_Insufficient = 100_400_002,
    }
}
