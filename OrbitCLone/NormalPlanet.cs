using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitCLone
{
    class NormalPlanet : GameEntity
    {
        public NormalPlanet(Vector2 c, Random r, SpriteBatch s, Texture2D t)
        {
            centerOfAttraction = c;
            spriteBatch = s;
            sprite = t;
            speed = 0.15f;

            angle = r.Next(0, 364);
            radius = 1000;
            position = new Vector2(centerOfAttraction.X + (float)Math.Cos(angle) * radius, centerOfAttraction.Y + (float)Math.Sin(angle) * radius);
            direction = centerOfAttraction - position;
            active = true;
        }

        public void Update(GameTime gt)
        {
            position += direction * speed * (float)gt.ElapsedGameTime.TotalSeconds;
            boundingSphere = new BoundingSphere(new Vector3(position, 0), 20);
        }

        public bool active { get; set; }
        private float speed;
        private int angle;
        private float radius;
        private Vector2 centerOfAttraction;
        private Vector2 direction;
    }
}
