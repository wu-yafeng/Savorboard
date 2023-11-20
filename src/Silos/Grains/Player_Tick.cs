using GameSdk.Base;
using GameSdk.Models;
using GameSdk.Parameters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    public partial class Player
    {
        public async Task TickAsync(TimeSpan deltaTime)
        {
            var addunits = (DateTimeOffset.Now - _vipData.State.AddExpTime).TotalSeconds;
            if (addunits > 5)
            {
                _vipData.State.CurExp += addunits * 5;
                _vipData.State.AddExpTime = DateTimeOffset.Now;
            }

            if (_vipData.State.CurExp >= _vipData.State.MaxExp)
            {
                _vipData.State.Level++;
                _vipData.State.CurExp -= _vipData.State.MaxExp;
                _vipData.State.MaxExp *= 1.5;

                await _observerManager.Notify(channel => channel.OnShowChatMsgAsync("提示", $"您的会员等级提升了 Lv{_vipData.State.Level}"));

                await AddEquipAsync(new AddEquipPack()
                {
                    AllowedWearSlots = WearSlot.RHand,
                    Attributes = new UAttributeData()
                    {
                        Armor = 10d,
                        Health = 20d,
                        Attack = 15d
                    }
                });

                var itemadd = await _metaMgr.Service.GetRequiredAsync<ItemMeta>(1);

                await AddItemAsync(itemadd, 1);
                if (_vipData.State.AutoExtBackpack)
                {
                    await UpgradeBackpackAsync(new UpgradeBackpackPack() { TargetLevel = _backpack.State.Level + 1 }).ContinueWith(x =>
                    {
                        if (x.Exception != null)
                        {
                            _logger.LogWarning("Auto ext backpack size failed.");
                        }
                    });
                }
            }



            //await _vipData.WriteStateAsync();
        }

    }
}
