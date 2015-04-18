using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

namespace Assets.Scripts.Common.Utility
{
    public class ParticleUtility
    {
        public static void FixParticlesWorldPositionBug(GameObject obj)
        {
            ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
                var ps = systems[i];
                ps.enableEmission = true;
                float duration = ps.duration;
                if (ps.startDelay <= 0.01f)   //fix unity bug by start Delay
                {
                    ps.startDelay = 0.035f;
                }
            }
        }

        public static void StopAllParticle(GameObject obj)
        {
            ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
                var ps = systems[i];
                ps.enableEmission = false;
                ps.Stop();
                ps.Clear();
            }
            ParticleEmitter[] emitters = obj.GetComponentsInChildren<ParticleEmitter>();
            for (int i = 0; i < emitters.Length; i++)
            {
                var pE = emitters[i];
                pE.enabled = false;
                pE.ClearParticles();

            }
        }

        public static void StartAllParticle(GameObject obj)
        {
            ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
                var ps = systems[i];
                ps.enableEmission = true;
                float duration = ps.duration;
                if (ps.startDelay <= 0.01f)   //fix unity bug by start Delay
                {
                    ps.startDelay = 0.035f;
                }
                ps.Play();
            }

            ParticleEmitter[] emitters = obj.GetComponentsInChildren<ParticleEmitter>();
            for (int i = 0; i < emitters.Length; i++)
            {
                var pe = emitters[i];
                pe.enabled = true;

            }
        }

    }
}
