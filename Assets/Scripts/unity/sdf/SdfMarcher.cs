using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace snorri
{    
    public struct SdfSurfaceData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 diffuse;
        public float radius;
        public int shapeType;
        public int blend;
        public float blendStrength;

        public static int GetSize()
        {
            return sizeof(float) * 14 + sizeof(int) * 4;
        }
    }
    public static class VectorExtensions
    {
        public static Vector3 ToVector3(this Vector4 vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }
    }
    public class SdfMarcher : Ticker
    {
        SdfModule module;
        Material material;

        bool isBlend;
        bool isOutline;

        Bag<SdfSurface> surfaces;
        List<ComputeBuffer> buffers;

        protected override void Setup()
        {
            base.Setup();

            isBlend = Vars.Get<bool>("is_blend", false);
            isOutline = Vars.Get<bool>("is_outline", false);
        }
        protected override void Launch()
        {
            base.Launch();

            module = Node.GetActor<SdfModule>();
            material = module.Material;
            surfaces = Node.GetBagOChildActors<SdfSurface>();

            LOG.Console($"sdf marcher found {surfaces.Length} surfaces");

            //CleanupBuffers();

            //SetShapesRenderData();
            //UpdateShader();
        }

        public override void Tick()
        {
            base.Tick();

            if (surfaces == null)
            {
                surfaces = Node.GetBagOChildActors<SdfSurface>();

                return;
            }
                
            if (surfaces.Length == 0)   
            {
                surfaces = Node.GetBagOChildActors<SdfSurface>();

                LOG.Console($"sdf marcher found {surfaces.Length} surfaces");
                return;
            }
        
            CleanupBuffers();

            SetShapesRenderData();
            
            UpdateShader();
        }

        protected override void WhenDestroy()
        {
            base.WhenDestroy();

            if (buffers == null)
                return;
                
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }

        private void CleanupBuffers()
        {
            if (buffers == null)
                buffers = new List<ComputeBuffer>();

            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }

            buffers.Clear();
        }

        private void SetShapesRenderData()
        {
            //var surfaces = new List<SdfSurface>(this.surfaces.All()).OrderBy(x => x.BlendMode).ToArray();
            surfaces = Node.GetBagOChildActors<SdfSurface>();

            var shapeData = new SdfSurfaceData[surfaces.Length];

            for (int i = 0; i < surfaces.Length; i++)
            {
                var surface = surfaces[i];
                if (surface == null)
                    continue;

                if (surface.GrowthFactor < 0.1)
                    continue;

                shapeData[i] = new SdfSurfaceData
                {
                    position = surface.Position,
                    scale = surface.Scale,
                    rotation = surface.Rotation,
                    shapeType = (int)surface.ShapeType,
                    radius = surface.Radius,
                    blend = (int)surface.BlendMode,
                    diffuse = (Vector4)surface.Diffuse,
                    blendStrength = surface.BlendStrength,
                };
            }

            UpdateComputeBuffer("shapes", shapeData);

            material.SetInt("numShapes", shapeData.Length);
        }

        private void UpdateComputeBuffer<T>(string name, T[] data) where T : struct
        {
            if (data.Length == 0)
                return;
                
            ComputeBuffer buffer = new ComputeBuffer(data.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            buffer.SetData(data);
            material.SetBuffer(name, buffer);
            buffers.Add(buffer);
        }

        private void UpdateShader()
        {
            material.SetFloat("_Outline", isOutline ? 1f : 0f);
            material.SetFloat("_Blend", isBlend ? 1f : 0f);
        }
    }
}   