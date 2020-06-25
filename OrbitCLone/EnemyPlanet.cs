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
        
        public PlanetData(float speed, int size)
        {
            Speed = speed;
            Size = size;
        }
    }

    class EnemyPlanet : Planet
    {
        public EnemyPlanet(PlanetData p)
        {
            Radius = 1000.0f;
            Angle = new Random().NextDouble() * Math.PI;
            position = new Vector2(centerOfAttraction.X + (float)Math.Cos(Angle) * Radius, centerOfAttraction.Y + (float)Math.Sin(Angle) * Radius);
            direction = centerOfAttraction - position;
            size = p.Size;
            sprite = p.Texture;
            speed = p.Speed;
        }

        public override void Update(GameTime gt)
        {
            position += direction * speed * (float)gt.ElapsedGameTime.TotalSeconds;
            boundingSphere = new BoundingSphere(new Vector3(position, 0), size);
            Radius = Vector2.Distance(position, centerOfAttraction);

            base.Update(gt);
        }

        private int size;
        private float speed;
        private Vector2 direction;
    }
}
