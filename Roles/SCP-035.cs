﻿using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.Patches;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp173;
using Exiled.Events.EventArgs.Scp3114;
using InventorySystem;
using MEC;
using PlayerRoles;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using TestPlugin;
using TestPlugin.ASMATIX_API;
using UnityEngine;
using UnityEngine.Assertions.Must;
using VoiceChat;

public class SCP035 : CustomRole {
    public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
    public override uint Id { get; set; } = 1;
    public override float SpawnChance { get; set; } = 0;
    public override int MaxHealth { get; set; } = 500;
    public override string Name { get; set; } = "Маска";
    public override string Description { get; set; } =
        "SCP-035";
    public override string CustomInfo { get; set; } = "SCP-035";
    public override List<string> Inventory { get; set; } = new List<string>() {
        $"{ItemType.Medkit}", $"{ItemType.Coin}"
    };
    public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties()
    {
        Limit = 1,
        RoleSpawnPoints = new List<RoleSpawnPoint> {
            new RoleSpawnPoint() {
                Role = RoleTypeId.Scientist,
                Chance = 0,
            }
        }
    };
    System.Random random = new System.Random();
    protected override void SubscribeEvents() {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Player.Spawned += OnSpawn;
        Exiled.Events.Handlers.Player.Died += OnDie;
        Exiled.Events.Handlers.Player.DroppingItem += Dr;
        Exiled.Events.Handlers.Player.FlippingCoin += Att;
        base.SubscribeEvents();
    }
    protected override void UnsubscribeEvents() {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Player.Spawned -= OnSpawn;
        Exiled.Events.Handlers.Player.Died -= OnDie;
        Exiled.Events.Handlers.Player.DroppingItem -= Dr;
        Exiled.Events.Handlers.Player.FlippingCoin -= Att;
        base.UnsubscribeEvents();
    }
    void Att(FlippingCoinEventArgs ev) { 
        if (Check(ev.Player)) { 
            foreach (Player player in Player.List) { 
                if (Vector3.Distance(ev.Player.Position, player.Position) <= 4 && ev.Player.NetId != player.NetId) {
                    foreach (Door door in Door.List) { 
                        if (Vector3.Distance(player.Position, door.Position) <= 6) {
                            if (door.KeycardPermissions != KeycardPermissions.None) {
                                if (player.Items.Any(item => item is Keycard keycard && keycard.Permissions > door.KeycardPermissions)) {
                                    door.IsOpen = true;
                                    Timing.RunCoroutine(Ef(2, player));
                                }
                            } else {
                                door.IsOpen = true;
                            }
                        }
                    }
                }
            }
        }
    }
    void Dr(DroppingItemEventArgs ev) { 
        if (Check(ev.Player) && ev.Item.Type == ItemType.Coin) {
            ev.IsAllowed = false;
        }
    }
    private void OnRoundStarted() {
        if (Exiled.API.Features.Player.List.Count() >= 8) {
            if (random.Next(0, 10) < 50) {
                CustomRole.Get((uint)1).AddRole(Exiled.API.Features.Player.List.Where(x => x.Role.Type == RoleTypeId.FacilityGuard)?.ToList().RandomItem());
            }
        }
    }
    void OnSpawn(SpawnedEventArgs ev) {
        if (Check(ev.Player)) {
            ev.Player.IsGodModeEnabled = false;
            ev.Player.MaxHealth = 500;
            Global.coroutine = Timing.RunCoroutine(API.Damage(ev.Player, 1, 5));
        }
    }
    void OnDie(DiedEventArgs ev) { 
        if (Check(ev.Player)) {
            Timing.KillCoroutines(Global.coroutine);
        }
    }
    private IEnumerator<float> Ef(int s, Player pl)
    {
        pl.EnableEffect(EffectType.Flashed);
        yield return Timing.WaitForSeconds(s);
        pl.DisableEffect(EffectType.Flashed);
    }
}
