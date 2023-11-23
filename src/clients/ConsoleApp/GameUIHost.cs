using ConsoleApp.Protos;
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
    /// <summary>
    /// Game UI Main Thread. Render and handling inputs.
    /// </summary>
    public class GameUIHost : IHostedService
    {
        private readonly GameWorld _world;
        private readonly IAsyncNetwork _networkMgr;

        public GameUIHost(GameWorld world, IAsyncNetwork networkMgr)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _networkMgr = networkMgr;
        }

        private ConsoleKeyInfo? PeekMessage()
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey();
            }

            return null;
        }

        private async Task OpenBackpack()
        {
            var viewmodel = await _networkMgr.GetBackpackDataViewModelAsync(new GetBackpackViewModelPack());

            _world.BackpackView.AppendLine($"Your backpack size is -> {viewmodel.MaxSize}");
        }

        private void InputMgrHandler()
        {
            var message = PeekMessage();

            // handle input
            if (message == null)
            {
                return;
            }

            if (message.Value.Key == ConsoleKey.F2)
            {
                if (_world.BackpackView.Length == 0)
                    _ = OpenBackpack();
                else
                    _world.BackpackView.Clear();
            }

            _world.KeyCharTop = string.Format("{0} + {1} + {2}", message.Value.Modifiers, message.Value.Key, message.Value.KeyChar);
        }

        private async Task SyncServerState()
        {
            for (var i = 0; i < 10; i++)
            {
                var events = await _networkMgr.PeekAsync();

                if (events == null)
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
                await BeginScene(frame_num);
                frame_num++;

                // state sync.
                await SyncServerState();
                InputMgrHandler();

                // render
                RenderInputMgr();
                RenderBackpackView();
                RenderChatMsg();

                await EndScene();

                await Present();
                // 60fps
                await Task.Delay(TimeSpan.FromMilliseconds(1000 / 60), cancellationToken);
            }
        }

        private async Task BeginScene(int frameId)
        {
            _world.Surface.Clear();

            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
        }

        private async Task EndScene()
        {
            // nothing todo.
        }

        private async Task Present()
        {
            Console.Write(_world.Surface.ToString());
        }

        private void RenderBackpackView()
        {
            _world.Surface.Append(_world.BackpackView);
        }
        private void RenderInputMgr()
        {
            _world.Surface.AppendFormat("InputMgr State: {0}\n", _world.KeyCharTop);
        }
        private void RenderChatMsg()
        {
            while (_world.ChatMsgs.Count > 8)
            {
                _world.ChatMsgs.RemoveFirst();
            }

            foreach (var item in _world.ChatMsgs)
            {
                _world.Surface.AppendLine(item);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
