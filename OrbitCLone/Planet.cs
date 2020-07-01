using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitCLone
{
    class PlanetData
    {
        public float Speed { get; set; }
        public Texture2D Texture { get; set; }
        public int Size { get; set; }
        public int Id { get; set; }

        public PlanetData(float speed, int size, int id)
        {
            Speed = speed;
            Size = size;
            Id = id;
        }
    }

    class Planet : GameEntity
    {
        public float Radius { get; set; }
        public double Angle { get; set; }
        public float Speed { get; set; }
        public int Size { get; set; }

        public Planet()
        {
            centerOfAttraction = new Vector2(1920 / 2, 1080 / 2);
        }

        public void Initialize(PlanetData p)
        {
            Speed = p.Speed;
            sprite = p.Texture;
            Size = p.Size;
        }

        protected Vector2 centerOfAttraction;
    }
}
