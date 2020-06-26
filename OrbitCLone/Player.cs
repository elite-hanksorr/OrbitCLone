using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Input;

namespace OrbitCLone
{
    class Player : Planet
    {
        public Player()
        {
            Radius = 400.0f;
            Angle = 0.0f;
            Speed = 2;
            Score = 0;
        }

        public override void Update(GameTime gt)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                Radius += 7;

            position = new Vector2(centerOfAttraction.X + (float)Math.Cos(Angle) * Radius, centerOfAttraction.Y + (float)Math.Sin(Angle) * Radius);
            boundingSphere = new BoundingSphere(new Vector3(position, 0), 22);

            double previousAngle = Angle;
            Angle = ((Angle + Speed * (float)gt.ElapsedGameTime.TotalSeconds) % (2 * Math.PI));
            if (Angle < previousAngle)
                Score++;

            if (Radius > 430)
            {
                Radius = 430;
                Speed = 2.0f;
            }
            else if (Radius > 300)
            {
                Speed = 2.0f;
            }
            else if (Radius > 200)
            {
                Speed = 2.5f;
            }
            else if (Radius > 100)
            {
                Speed = 3;
            }

            Radius -= 200 * (float)gt.ElapsedGameTime.TotalSeconds;

            base.Update(gt);
        }

        public int Score { get; set; }
    }
}
