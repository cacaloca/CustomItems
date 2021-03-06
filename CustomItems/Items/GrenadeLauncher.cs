// <copyright file="GrenadeLauncher.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CustomItems.Items
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomItems.API;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Grenades;
    using MEC;
    using Mirror;
    using UnityEngine;

    /// <inheritdoc />
    public class GrenadeLauncher : CustomWeapon
    {
        /// <inheritdoc />
        public GrenadeLauncher(ItemType type, int clipSize, int itemId)
            : base(type, clipSize, itemId)
        {
        }

        /// <inheritdoc/>
        public override string Name { get; set; } = Plugin.Singleton.Config.ItemConfigs.GlCfg.Name;

        /// <inheritdoc/>
        public override Dictionary<SpawnLocation, float> SpawnLocations { get; set; } = Plugin.Singleton.Config.ItemConfigs.GlCfg.SpawnLocations;

        /// <inheritdoc/>
        public override string Description { get; set; } = Plugin.Singleton.Config.ItemConfigs.GlCfg.Description;

        /// <inheritdoc/>
        public override int SpawnLimit { get; set; } = Plugin.Singleton.Config.ItemConfigs.GlCfg.SpawnLimit;

        /// <inheritdoc/>
        protected override void LoadEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            base.LoadEvents();
        }

        /// <inheritdoc/>
        protected override void UnloadEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            base.UnloadEvents();
        }

        /// <inheritdoc/>
        protected override void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (!CheckItem(ev.Player.CurrentItem))
                return;

            if (Plugin.Singleton.Config.ItemConfigs.GlCfg.UseGrenades)
            {
                ev.IsAllowed = false;
                Log.Debug($"{ev.Player.Nickname} is reloading a {Name}!", Plugin.Singleton.Config.Debug);
                foreach (Inventory.SyncItemInfo item in ev.Player.Inventory.items.ToList())
                {
                    if (item.id != ItemType.GrenadeFrag)
                        continue;

                    ev.Player.ReferenceHub.weaponManager.scp268.ServerDisable();
                    Reload(ev.Player);

                    ev.Player.Inventory.items.ModifyDuration(ev.Player.Inventory.GetItemIndex(), ClipSize);
                    Log.Debug($"{ev.Player.Nickname} successfully reloaded a {Name}.", Plugin.Singleton.Config.Debug);
                    Timing.CallDelayed(4.5f, () => { Reload(ev.Player); });
                    ev.Player.RemoveItem(item);

                    break;
                }

                Log.Debug($"{ev.Player.Nickname} was unable to reload their {Name} - No grenades in inventory.", Plugin.Singleton.Config.Debug);
            }
            else
            {
                base.OnReloadingWeapon(ev);
            }
        }

        /// <summary>
        /// Spawns a live grenade object on the map.
        /// </summary>
        /// <param name="position">The <see cref="Vector3"/> to spawn the grenade at.</param>
        /// <param name="velocity">The <see cref="Vector3"/> directional velocity the grenade should move at.</param>
        /// <param name="fuseTime">The <see cref="float"/> fuse time of the grenade.</param>
        /// <param name="grenadeType">The <see cref="GrenadeType"/> of the grenade to spawn.</param>
        /// <param name="player">The <see cref="Player"/> to count as the thrower of the grenade.</param>
        /// <returns>The <see cref="Grenade"/> being spawned.</returns>
        ///
        /// I stole this from Synapse.Api.Map.SpawnGrenade -- Thanks Dimenzio, I was dreading having to find my super old version and adapting it to the new game version.
        private static Grenades.Grenade SpawnGrenade(Vector3 position, Vector3 velocity, float fuseTime = 3f, GrenadeType grenadeType = GrenadeType.FragGrenade, Player player = null)
        {
            if (player == null)
                player = Server.Host;

            GrenadeManager component = player.GrenadeManager;
            Grenade component2 = GameObject.Instantiate(component.availableGrenades[(int)grenadeType].grenadeInstance).GetComponent<Grenades.Grenade>();
            component2.FullInitData(component, position, Quaternion.Euler(component2.throwStartAngle), velocity, component2.throwAngularVelocity, player == Server.Host ? Team.RIP : player.Team);
            component2.NetworkfuseTime = NetworkTime.time + fuseTime;
            NetworkServer.Spawn(component2.gameObject);

            return component2;
        }

        private void OnShooting(ShootingEventArgs ev)
        {
            if (!CheckItem(ev.Shooter.CurrentItem))
                return;

            ev.IsAllowed = false;
            ev.Shooter.SetWeaponAmmo(ev.Shooter.CurrentItem, (int)ev.Shooter.CurrentItem.durability - 1);

            Vector3 velocity = (ev.Position - ev.Shooter.Position) * Plugin.Singleton.Config.ItemConfigs.GlCfg.GrenadeSpeed;
            Grenade grenadeComponent = ev.Shooter.GrenadeManager.availableGrenades[0].grenadeInstance.GetComponent<Grenade>();
            Vector3 pos = ev.Shooter.CameraTransform.TransformPoint(grenadeComponent.throwStartPositionOffset);
            Grenade grenade = SpawnGrenade(pos, velocity, Plugin.Singleton.Config.ItemConfigs.GlCfg.FuseTime, GrenadeType.FragGrenade, ev.Shooter);
            CollisionHandler collisionHandler = grenade.gameObject.AddComponent<CollisionHandler>();
            collisionHandler.Owner = ev.Shooter.GameObject;
            collisionHandler.Grenade = grenadeComponent;
        }
    }
}