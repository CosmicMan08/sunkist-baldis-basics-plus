using MTM101BaldAPI.Components;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SillyBaldiAdditions
{
    public class SunkistDog : NPC
    {
        public AudioManager audMan;
        public SoundObject bark;
        public SoundObject panting;
        public AudioManager pantAudMan;
        public int barkTimer = 60;
        public int pantingTimer = 25;

        public override void Initialize()
        {
            Debug.Log("Sunkist initialized");
            base.Initialize();
            this.behaviorStateMachine.ChangeState(new SunkistDog_DefaultState(this, this));
        }

        protected override void VirtualUpdate()
        {
            base.VirtualUpdate();
        }

        public void GoCrazy()
        {
            this.behaviorStateMachine.ChangeState(new SunkistDog_Satisfied(this, this));
        }
    }
    public enum SunkistDogPrimaryState
    {
        Normal, Crazed
    }


    public class SunkistDog_DefaultState : NpcState
    {
        public SunkistDog_DefaultState(NPC npc, SunkistDog SunkistDog) : base(npc)
        {
            this.SunkistDog = SunkistDog;
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("SunkistDog entering default state.");
            //this.SunkistDog.pantAudMan.Pause(false);
            base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));
            this.npc.looker.Blink();
            this.npc.Navigator.SetSpeed(15f);
            this.npc.Navigator.maxSpeed = 15f;
        }

        public override void Update()
        {
            base.Update();
            if (this.SunkistDog.pantingTimer < 0)
            {
                this.SunkistDog.pantAudMan.PlaySingle(this.SunkistDog.panting);
                this.SunkistDog.pantingTimer = 25;
            }
            this.SunkistDog.pantingTimer--;
        }

        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            //this.SunkistDog.pantAudMan.Pause(true);
            this.npc.behaviorStateMachine.ChangeState(new SunkistDog_PursuePlayer(this.npc, this.SunkistDog, player));
            //this.SunkistDog.audMan.PlaySingle(this.SunkistDog.bark);
        }

        public override void OnStateTriggerEnter(Collider other)
        {
            base.OnStateTriggerEnter(other);
            if (other.CompareTag("Player"))
            {
                //this.SunkistDog.GoCrazy();
                //this.SunkistDog.audMan.PlaySingle(this.SunkistDog.bark);
            }
        }

        public SunkistDog SunkistDog;
    }


    public class SunkistDog_Satisfied : NpcState
    {
        public SunkistDog_Satisfied(NPC npc, SunkistDog SunkistDog) : base(npc)
        {
            this.SunkistDog = SunkistDog;
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));
            Debug.Log("doggo is satisfied :3");
        }

        public override void Update()
        {
            base.Update();
            this.npc.Navigator.SetSpeed(25f);
            this.satisficationTime -= Time.deltaTime;
            if (this.satisficationTime <= 0f)
            {
                this.npc.behaviorStateMachine.ChangeState(new SunkistDog_DefaultState(this.npc, this.SunkistDog));
            }
        }

        public SunkistDog SunkistDog;

        private float satisficationTime = 4f;
    }


    public class SunkistDog_PursuePlayer : NpcState
    {
        public SunkistDog_PursuePlayer(NPC npc, SunkistDog SunkistDog, PlayerManager player) : base(npc)
        {
            this.SunkistDog = SunkistDog;
            this.player = player;
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("SunkistDog entering pursuit state.");
            base.ChangeNavigationState(new NavigationState_TargetPlayer(this.npc, 63, this.player.transform.position));
        }

        public override void PlayerInSight(PlayerManager player)
        {
            if (this.SunkistDog.barkTimer < 0) {
                this.SunkistDog.audMan.pitchModifier = new System.Random().Next(100, 150) / 100f;
                this.SunkistDog.audMan.PlaySingle(this.SunkistDog.bark);
                this.SunkistDog.ec.MakeNoise(this.SunkistDog.transform.position, 62);
                this.SunkistDog.barkTimer = new System.Random().Next(30,70);
            }
            this.SunkistDog.barkTimer--;
            base.PlayerInSight(player);
            this.npc.behaviorStateMachine.CurrentNavigationState.UpdatePosition(player.transform.position);
        }

        public override void DestinationEmpty()
        {
            this.SunkistDog.barkTimer--;
            base.DestinationEmpty();
            if (!this.npc.looker.PlayerInSight())
            {
                this.npc.behaviorStateMachine.ChangeState(new SunkistDog_DefaultState(this.npc, this.SunkistDog));
            }
        }

        public override void PlayerLost(PlayerManager player)
        {
            this.SunkistDog.behaviorStateMachine.ChangeState(new SunkistDog_Satisfied(this.SunkistDog, this.SunkistDog));
        }

        public SunkistDog SunkistDog;

        public PlayerManager player;
    }
}