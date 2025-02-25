﻿using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.UI;
using Fika.Core.Coop.Players;
using Fika.Core.Networking;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fika.Core.Coop.Utils;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Fika.Core.Coop.Utils;
using RealismModSync.StanceReplication.Packets;

namespace RealismModSync.StanceReplication.Components
{
    public class RSR_Component : MonoBehaviour
    {
        CoopPlayer player;
        FikaServer server;
        FikaClient client;
        bool isServer = false;
 
        private void Start()
        {
            player = GetComponent<CoopPlayer>();
            if(player == null)
            {
                Destroy(this);
                return;
            }

            if (FikaBackendUtils.IsServer)
            {
                server = Singleton<FikaServer>.Instance;
                isServer = true;
            }
            else
            {
                client = Singleton<FikaClient>.Instance;
            }

            player.OnPlayerDead += DeleteThis;
        }

        private void DeleteThis(EFT.Player player, EFT.IPlayer lastAggressor, DamageInfoStruct damageInfo, EBodyPart part)
        {
            player.OnPlayerDead -= DeleteThis;
            Destroy(this);
        }

        private void FixedUpdate()
        {
         /*   if (frameCounter % Plugin.TickRate.Value == 0)
            {
                frameCounter = 0;

         
            }
            else
            {
                frameCounter++;
            }*/

            RealismStanceReplicationPacket packet = new RealismStanceReplicationPacket()
            {
                NetID = player.NetId,
                WeapPosition = player.ProceduralWeaponAnimation.HandsContainer.HandsPosition.Zero,
                Rotation = player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim.transform.rotation,
                IsPatrol = WeaponAnimationSpeedControllerClass.GetBoolPatrol(player.HandsAnimator.Animator),
                SprintAnimationVarient = player.BodyAnimatorCommon.GetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH)
            };

           
            if (isServer)
            {
                server.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.Unreliable);
            }
            else
            {
                client.SendData(ref packet, LiteNetLib.DeliveryMethod.Unreliable);
            }
        }
    }
}
