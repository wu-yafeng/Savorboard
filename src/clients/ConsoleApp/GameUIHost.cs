using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class GameUIHost : IHostedService
    {
        private readonly GameWorld _world;

        public GameUIHost(GameWorld world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        private ConsoleKeyInfo? PeekMessage()
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey();
            }

            return null;
        }

        private void InputMgrHandler()
        {
            var message = PeekMessage();

            // handle input
            if (message == null)
            {
                return;
            }

            _world.KeyCharTop = string.Format("{0} + {1} + {2}", message.Value.Modifiers, message.Value.Key, message.Value.KeyChar);
        }

        private void SyncServerState()
        {
            for (var i = 0; i < 10; i++)
            {
                if (!_world.Messages.TryDequeue(out var events))
                {
                    break;
                }

                if (events.Data.TryUnpack<ChatMsg>(out var chatMsg))
                {
                    _world.ChatMsgs.AddLast($"[{chatMsg.Channel}]:{chatMsg.Content}");
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var frame_num = 0;
            // game ui loop
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Clear();
                Console.WriteLine("rendering frame#{0}...", frame_num);
                frame_num++;

                // state sync.
                SyncServerState();
                InputMgrHandler();

                // render
                RenderInputMgr();
                RenderChatMsg();

                // 60fps
                await Task.Delay(TimeSpan.FromMilliseconds(1000 / 60), cancellationToken);
            }
        }

        private void RenderInputMgr()
        {
            Console.WriteLine("InputMgr State: {0}", _world.KeyCharTop);
        }
        private void RenderChatMsg()
        {
            while (_world.ChatMsgs.Count > 8)
            {
                _world.ChatMsgs.RemoveFirst();
            }

            foreach (var item in _world.ChatMsgs)
            {
                Console.WriteLine(item);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
