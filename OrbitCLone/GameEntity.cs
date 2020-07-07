using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitCLone
{
    class GameEntity
    {
        public Texture2D sprite { get; set; }
        public Vector2 position { get; set; }
        public BoundingSphere boundingSphere { get; set; }

        public virtual void Update(GameTime gt) { }

        public virtual void Draw(SpriteBatch sb, GameTime gt)
        {
            sb.Draw(sprite, position, null, Color.White, 0f, new Vector2(sprite.Width / 2, sprite.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        }
    }
}
