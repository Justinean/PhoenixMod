﻿using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using PhoenixWright.Modules.Survivors;

namespace PhoenixWright.SkillStates
{
    public class PressTurnabout : BaseSkillState
    {
        public static float damageCoefficient = 8f;
        public static float procCoefficient = 2f;
        public static float duration = 1f;
        public Vector3 rayPosition;


        private bool hasFired;
        private float stopwatch;
        private Animator animator;
        private int random;

        protected string hitboxName = "press";
        protected BlastAttack blastAttack;
        protected float attackStartTime = 0.01f * duration;
        protected float attackEndTime = 1f *duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = base.GetAimRay();
            this.hasFired = false;

            base.StartAimMode(duration, true);
            base.PlayAnimation("FullBody, Override", "Point", "ShootGun.playbackRate", (PressTurnabout.duration  / PressTurnabout.duration));

            rayPosition = aimRay.origin + 20 * aimRay.direction;

            random = UnityEngine.Random.Range(0, 2);

            if(random == 0)
            {
                EffectManager.SpawnEffect(Modules.Assets.pressobjectEffect, new EffectData
                {
                    origin = rayPosition,
                    scale = 1f,
                    rotation = Quaternion.LookRotation(aimRay.direction)

                }, true);
                if (Modules.Config.loweredVolume.Value)
                {
                    Util.PlaySound("ObjectionQuiet", base.gameObject);
                }
                else Util.PlaySound("Objection", base.gameObject);
            }
            else
            {
                EffectManager.SpawnEffect(Modules.Assets.presstTakeEffect, new EffectData
                {
                    origin = rayPosition,
                    scale = 1f,
                    rotation = Quaternion.LookRotation(aimRay.direction)

                }, true);
                if (Modules.Config.loweredVolume.Value)
                {
                    Util.PlaySound("TakeThatQuiet", base.gameObject);
                }
                else Util.PlaySound("TakeThat", base.gameObject);
            }

            blastAttack = new BlastAttack();
            blastAttack.radius = 10f;
            blastAttack.procCoefficient = procCoefficient;
            blastAttack.position = rayPosition;
            blastAttack.attacker = base.gameObject;
            blastAttack.crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
            blastAttack.baseDamage = this.damageStat * damageCoefficient;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
            blastAttack.damageType = DamageType.Stun1s | DamageType.Freeze2s | DamageType.BypassArmor;
            blastAttack.attackerFiltering = AttackerFiltering.Default;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Ray aimRay = base.GetAimRay();
            rayPosition = aimRay.origin + 20 * aimRay.direction;

            stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch >= attackStartTime && this.stopwatch <= attackEndTime )
            {
                FireAttack();
            }


            if (base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterMotor.disableAirControlUntilCollision = false;
        }

        private void FireAttack()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                blastAttack.Fire();
            }
        }


    }
}