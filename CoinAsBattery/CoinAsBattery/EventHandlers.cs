using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI;
using PluginAPI.Core.Attributes;
using PluginAPI.Core;
using PluginAPI.Enums;
using Footprinting;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;

namespace CoinAsBattery
{
    public class EventHandlers
    {
        [PluginEvent(ServerEventType.PlayerSearchedPickup)]
        public bool OnPickedItem(Player player, ItemPickupBase pickup)
        {
            if (pickup.Info.ItemId == ItemType.Coin)
            {
                if (player.Items.Any(i => i.ItemTypeId == ItemType.Radio) && config.RadioBatteryCapacity > 0)
                {
                    RadioItem radio = (RadioItem)player.Items.First(i => i.ItemTypeId == ItemType.Radio);
                    if (radio.IsUsable || radio.BatteryPercent == 0)
                    {
                        radio.BatteryPercent += config.RadioBatteryCapacity;
                        player.ReferenceHub.inventory.ServerRemoveItem(pickup.Info.Serial, pickup);
                        Log.Debug($"Player {player.Nickname} recharged Radio to {radio.BatteryPercent}.", config.Debug, "CoinAsBattery");
                        return false;
                    }
                }
                
                if (player.Items.Any(i => i.ItemTypeId == ItemType.MicroHID) && config.MicroBatteryCapacity > 0)
                {
                    MicroHIDItem micro = (MicroHIDItem)player.Items.First(i => i.ItemTypeId == ItemType.MicroHID);
                    if (micro.RemainingEnergy < 1)
                    {
                        micro.RemainingEnergy += config.MicroBatteryCapacity;
                        player.ReferenceHub.inventory.ServerRemoveItem(pickup.Info.Serial, pickup);
                        Log.Debug($"Player {player.Nickname} recharged MicroHID to {micro.RemainingEnergy}.", config.Debug, "CoinAsBattery");
                        return false;
                    }
                    if (config.ShouldExplode.ToLower() == "pick")
                    {
                        player.ReferenceHub.inventory.ServerRemoveItem(micro.ItemSerial, micro.PickupDropModel);
                        player.ReferenceHub.inventory.ServerRemoveItem(pickup.Info.Serial, pickup);
                        ExplosionGrenade.Explode(new Footprint(player.ReferenceHub), player.Position, MicroHIDGrenade());
                        Log.Debug($"MicroHID exploded, because player {player.Nickname} overcharged it.", config.Debug, "CoinAsBattery");
                        return false;
                    }
                    if (config.ShouldExplode.ToLower() == "fire")
                    {
                        pickup.DestroySelf();
                        if (!microBombs.ContainsKey(micro.ItemSerial))
                        {
                            microBombs.Add(micro.ItemSerial, micro);
                        }
                        if (!_micro.IsRunning)
                        {
                            _micro = Timing.RunCoroutine(CheckMicro());
                        }
                        return false;
                    }
                }
            }

            if (config.ShouldExplode == "fire" && pickup is MicroHIDPickup && microBombs.ContainsKey(pickup.Info.Serial))
            {
                Timing.CallDelayed(0.1f, delegate ()
                {
                    MicroHIDItem micro = (MicroHIDItem)player.Items.First(i => i.ItemSerial == pickup.Info.Serial);
                    microBombs[pickup.Info.Serial] = micro;
                    Log.Debug("Added picked up MicroHID to microBombs list.", config.Debug, "CoinAsBattery");
                    _micro = Timing.RunCoroutine(CheckMicro());
                });
            }
            return true;
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void OnWaitingForPlayers()
        {
            microBombs.Clear();
        }

        internal static IEnumerator<float> CheckMicro()
        {
            Log.Debug("Run coroutine CheckMicro.", config.Debug, "CoinAsBattery");
            while (Round.IsRoundStarted)
            {
                yield return Timing.WaitForOneFrame;
                if (microBombs.Values.All(v => v == null))
                {
                    Log.Debug("Killed coroutine CheckMicro.", config.Debug, "CoinAsBattery");
                    yield break;
                }
                foreach (var micro in microBombs.ToList())
                {
                    if (micro.Value != null && micro.Value.Owner == null)
                    {
                        microBombs[micro.Key] = null;
                        Log.Debug("Removed dropped MicroHID from microBombs list.", config.Debug, "CoinAsBattery");
                    }
                    if (micro.Value.State == HidState.Firing)
                    {
                        yield return Timing.WaitForSeconds(1.5f);
                        micro.Value.Owner.inventory.ServerRemoveItem(micro.Value.ItemSerial, micro.Value.PickupDropModel);
                        ExplosionGrenade.Explode(new Footprint(micro.Value.Owner), micro.Value.Owner.transform.position, MicroHIDGrenade());
                        Log.Debug($"MicroHID exploded, because player {micro.Value.Owner.nicknameSync.MyNick} fired it, while being overcharged.", config.Debug, "CoinAsBattery");
                        microBombs.Remove(micro.Key);
                        Log.Debug("Removed exploded MicroHID from microBombs list.", config.Debug, "CoinAsBattery");
                    }
                }
            }
            Log.Debug("Killed coroutine CheckMicro.", config.Debug, "CoinAsBattery");
            yield break;
        }

        public static ExplosionGrenade MicroHIDGrenade()
        {
            InventoryItemLoader.AvailableItems.TryGetValue(ItemType.GrenadeHE, out ItemBase itemBase);
            ThrowableItem throwableItem = (ThrowableItem)itemBase;
            return (ExplosionGrenade)throwableItem.Projectile;
        }

        private static Config config = Plugin.Singleton.PluginConfig;

        public static Dictionary<ushort, MicroHIDItem> microBombs = new Dictionary<ushort, MicroHIDItem>();

        public static CoroutineHandle _micro;
    }
}
