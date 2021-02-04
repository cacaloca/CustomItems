using System.Collections.Generic;
using System.Linq;
using CustomItems.Items;
using CustomItems.API;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace CustomItems
{
    public class EventHandlers
    {
        private readonly Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        //This is to prevent making more new item managers when they aren't needed, that could get messy.
        private bool first = true;

        public void OnWaitingForPlayers()
        {
            if (first)
            {
                new Shotgun(ItemType.GunMP7, plugin.Config.ItemConfigs.ShotgunCfg.SpreadCount * 2, 1).RegisterCustomItem();
                
                new GrenadeLauncher(ItemType.GunLogicer,1,  2).RegisterCustomItem();
                
                new SniperRifle(ItemType.GunE11SR, 1, 3).RegisterCustomItem();
                
                new Scp127(ItemType.GunCOM15, 12, 4).RegisterCustomItem();
                
                new ImplosionGrenade(ItemType.GrenadeFrag, 5).RegisterCustomItem();
                
                new EmpGrenade(ItemType.GrenadeFlash, 6).RegisterCustomItem();
                
                new LethalInjection(ItemType.Adrenaline, 7).RegisterCustomItem();
                
                new MediGun(ItemType.GunProject90, 30, 8).RegisterCustomItem();

                new TranqGun(ItemType.GunUSP, 3, 9).RegisterCustomItem();
                
                plugin.Config.ParseSubclassList();
                
                first = false;
            }
        }

        public void OnReloadingConfigs()
        {
            plugin.Config.ParseSubclassList();
            plugin.Config.LoadConfigs();
        }

        public void OnRoundStart()
        {
            foreach (CustomItem item in plugin.ItemManagers)
            {
                if (item.SpawnLocations != null)
                {
                    foreach (KeyValuePair<Vector3, float> spawn in item.SpawnLocations)
                    {
                        Log.Debug($"Attempting to spawn {item.ItemName} at {spawn.Key}", plugin.Config.Debug);
                        if (plugin.Rng.Next(100) <= spawn.Value)
                        {
                            item.SpawnItem(spawn.Key + Vector3.up * 1.5f);
                            Log.Debug($"Spawned {item.ItemName} at {spawn.Key}", plugin.Config.Debug);
                        }
                    }
                }
            }
        }
    }
}