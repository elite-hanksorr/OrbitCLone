using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using ECS;
using Microsoft.Xna.Framework;
using OrbitCLone.Components;

namespace OrbitCLone.Systems
{
    class DrawingSystem : ComponentSystem
    {
        public SpriteBatch sb;

        public override void OnUpdate(GameTime gt)
        {
            sb.Begin();
            Entities.ForEach((ref Position p, ref Sprite s) =>
            {
                sb.Draw(s.Texture, new Vector2(p.x, p.y), null, Color.White, 0f, new Vector2(s.Texture.Width / 2, s.Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            });
            sb.End();
        }
    }
}
