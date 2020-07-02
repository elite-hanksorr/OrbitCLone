using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitCLone
{
    class EnemyPlanet : Planet
    {
        public EnemyPlanet(PlanetData p)
        {
            Initialize(p);
            PlanetId = p.Id;
            Radius = 1000.0f;
            Angle = rng.NextDouble() * Math.PI;
            position = new Vector2(centerOfAttraction.X + (float)Math.Cos(Angle) * Radius, centerOfAttraction.Y + (float)Math.Sin(Angle) * Radius);
            direction = centerOfAttraction - position;
        }

        public override void Update(GameTime gt)
        {
            position += direction * Speed * (float)gt.ElapsedGameTime.TotalSeconds;
            boundingSphere = new BoundingSphere(new Vector3(position, 0), Size);
            Radius = Vector2.Distance(position, centerOfAttraction);

            base.Update(gt);
        }

        private Vector2 direction;
        public int PlanetId;
        private static Random rng = new Random();
    }
}
