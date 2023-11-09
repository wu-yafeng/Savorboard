using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Observers
{
    public interface IChat
    {
        Task OnShowChatMsgAsync(string name, string message);
    }
}
