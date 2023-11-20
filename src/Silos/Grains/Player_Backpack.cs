using GameSdk.Base;
using GameSdk.Parameters;
using GameSdk.ViewModels;
using Silos.PersistenStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    // user backpack module.
    public partial class Player
    {
        private async Task<ErrorCodes?> BackpackLvUpAsync(int level)
        {
            var errorCode = default(ErrorCodes?);

            var targetLvMeta = await _metaMgr.Service.GetRequiredAsync<BackpackData>(level);

            if (_backpack.State.Exp < targetLvMeta.Exp)
            {
                errorCode = ErrorCodes.Backpack_LvUpExpInsufficient;
            }

            if (errorCode == null)
            {
                _backpack.State.Level = level;
                _backpack.State.MaxSize += targetLvMeta.ExtSize;
            }

            return errorCode;
        }

        private void InvokeFeatures(ItemMeta meta, int count)
        {
            switch (meta.Feature)
            {
                case ItemMeta.FeatureType.None:
                    break;
                case ItemMeta.FeatureType.AddBackpackExp:
                    _backpack.State.Exp += meta.FeatureParam1 * count;
                    break;
                default:
                    break;
            }
        }

        private async Task InitBackpack()
        {
            if (!_backpack.RecordExists)
            {
                _backpack.State = UBackpackData.Default();

                await BackpackLvUpAsync(_backpack.State.Level);

                await _backpack.WriteStateAsync();
            }

            foreach (var item in _backpack.State.Items)
            {
                item.SetMeta(await _metaMgr.Service.GetRequiredAsync<ItemMeta>(item.BaseId));
            }
        }

        public async Task UpgradeBackpackAsync(UpgradeBackpackPack messagePack)
        {
            // search items for add exp.
            var targetLv = await _metaMgr.Service.GetRequiredAsync<BackpackData>(messagePack.TargetLevel);

            if (_backpack.State.Exp < targetLv.Exp)
            {
                var itemsPool = _backpack.State.Items
                    .Where(x => x.GetMeta().Feature == ItemMeta.FeatureType.AddBackpackExp && x.GetMeta().FeatureParam1 > 0)
                    .OrderBy(x => x.BaseId)
                    .ToArray();

                foreach (var item in itemsPool)
                {
                    decimal needExp = targetLv.Exp - _backpack.State.Exp;

                    if (needExp <= 0)
                    {
                        break;
                    }

                    var count = Math.Min(item.Count,
                        (int)Math.Ceiling(needExp / item.GetMeta().FeatureParam1));

                    _backpack.State.RemoveItem(item, count);

                    InvokeFeatures(item.GetMeta(), count);
                }
            }

            var errorCode = await BackpackLvUpAsync(targetLv.Id);

            if (errorCode != null)
            {
                throw new BizException(errorCode.Value);
            }

            await _backpack.WriteStateAsync();
            await _observerManager.Notify(x => x.OnShowChatMsgAsync("backpack", $"your backpack has been upgraded! Now, your backpack level is {_backpack.State.Level}"));
        }


        public async Task<UBackpackViewModel> GetBackpackViewDataAsync()
        {
            return _backpack.State.ToViewModel();
        }

        public async Task AddEquipAsync(AddEquipPack message)
        {
            var serverPos = Enumerable.Range(1, _backpack.State.MaxSize)
                .Except(_backpack.State.Equips.Select(eqp => eqp.ServerPos))
                .FirstOrDefault();

            if (_backpack.State.MaxSize >= _backpack.State.GetCurrentSize())
            {
                //await _observerManager.Notify();
                return;
            }

            var equip = new UEquip()
            {
                ServerPos = serverPos,
                Attributes = message.Attributes,
                AllowedWearSlots = message.AllowedWearSlots,
                BaseId = message.BaseId
            };

            _backpack.State.Equips.Add(equip);

            await _backpack.WriteStateAsync();

            var result = equip.ToViewModel();

            await _observerManager.Notify(channel => channel.OnEquipAddedAsync(result));
        }

        private async Task AddItemAsync(ItemMeta meta, int count)
        {
            var item = _backpack.State.Items.FirstOrDefault(x => x.BaseId == meta.Id);

            if (item == null)
            {
                if (_backpack.State.MaxSize <= _backpack.State.GetCurrentSize())
                {
                    await _observerManager.Notify(x => x.OnShowChatMsgAsync("Backpack", "back pack size error."));
                    return;
                }

                var serverPos = Enumerable.Range(1, _backpack.State.MaxSize)
                    .Except(_backpack.State.Items.Select(eqp => eqp.ServerPos))
                    .FirstOrDefault();

                item = new()
                {
                    ServerPos = serverPos,
                    BaseId = meta.Id,
                    Count = default
                };

                item.SetMeta(meta);

                _backpack.State.Items.Add(item);
            }

            item.Count += count;

            await _backpack.WriteStateAsync();

            await _observerManager.Notify(x => x.OnShowChatMsgAsync("Backpack", $"You received a item [{item.BaseId}]x{count}. Now, you have [{item.BaseId}]x{item.Count}"));
        }
    }
}
