﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using ChartAndGraph;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using Fika.Core.Coop.GameMode;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib;
using RealismMod;
using RealismModSync.Audio.Components;
using RealismModSync.Audio.Packets;
using RealismModSync.HazardZones.Packets;
using RealismModSync.StanceReplication.Components;
using RealismModSync.StanceReplication.Packets;
using UnityEngine;

namespace RealismModSync.Audio;

public static class Fika
{

    public static void Register()
    {
        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(NetworkManagerCreated);
        FikaEventDispatcher.SubscribeEvent<FikaGameCreatedEvent>(GameWorldStarted);
    }

    private static void NetworkManagerCreated(FikaNetworkManagerCreatedEvent @event)
    {
        switch (@event.Manager)
        {
            case FikaServer server:
                server.RegisterPacket<RealismAudioPacket, NetPeer>(HandleRealismAudioPacketServer);
                break;
            case FikaClient client:
                client.RegisterPacket<RealismAudioPacket>(HandleRealismAudioPacket);
                break;
        }
        
    }

    private static void HandleRealismAudioPacketServer(RealismAudioPacket packet, NetPeer peer)
    {
        HandleRealismAudioPacket(packet);
        Plugin.REAL_Logger.LogInfo($"Rebroadcasting audio packet: {packet.Clip} from {packet.NetID} for device {packet.DeviceType}");
        Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered, peer);
    }

    private static void HandleRealismAudioPacket(RealismAudioPacket packet)
    {
        Core.ObservedComponents.TryGetValue(packet.NetID, out var observedComponent);
        if (observedComponent == null)
        {
            Plugin.REAL_Logger.LogInfo($"Could not find observed audio component for {packet.NetID}");
            return;
        }
        
        Plugin.REAL_Logger.LogInfo($"Processing audio packet: {packet.Clip} from {packet.NetID} for device {packet.DeviceType}. Source Volume: {packet.Volume}.");
        
        switch (packet.DeviceType)
        {
            case RealismDeviceType.Geiger:
                observedComponent.PlayGeigerClips(packet.Clip, packet.Volume);
                break;
            case RealismDeviceType.GasAnalyzer:
                observedComponent.PlayGasAnalyserClips(packet.Clip, packet.Volume);
                break;
            default:
                Plugin.REAL_Logger.LogInfo($"Unknown device type {packet.DeviceType}");
                break;
        }
    }


    private static void GameWorldStarted(FikaGameCreatedEvent @event)
    {
        Plugin.REAL_Logger.LogInfo("Clearing audio components");
        if (Core.ObservedComponents != null)
        {
            Core.ObservedComponents.Clear();
        }
        else
        {
            Core.ObservedComponents = new ConcurrentDictionary<int, RSAObservedComponent>();
        }
        
        /* Not needed at this point
        // loop through the audio clips for Gas And Geiger and name them
        foreach(var (key, audioClip) in RealismMod.Plugin.DeviceAudioClips)
        {
            // set the name of the clip to the key
            var original = audioClip.name;
            RealismMod.Plugin.DeviceAudioClips[key].name = key;
            Plugin.REAL_Logger.LogInfo($"Set {key} audio clip name for device from {original}");
        } */
    }
}