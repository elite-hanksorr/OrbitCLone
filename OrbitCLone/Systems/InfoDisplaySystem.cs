using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCLone.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class InfoDisplaySystem : ComponentSystem
    {
        public SpriteBatch sb;
        public SpriteFont f;

        public override void OnUpdate(GameTime gt)
        {
            int high_score = 0;

            // Find the high score among players.
            Entities.ForEach((ref Score s, ref PlayerTag t) =>
            {
                if (s.Value > high_score)
                    high_score = s.Value;
            });

            // Draw the score to the screen.
            var score_string = "Score: " + high_score.ToString();
            Vector2 textMiddlePoint = f.MeasureString(score_string) / 2;
            Vector2 textPos = new Vector2(GameConfig.SCREEN_WIDTH / 2, 50);
            sb.DrawString(f, score_string, textPos, Color.White, 0, textMiddlePoint, 1.5f, SpriteEffects.None, 0.5f);
        }
    }
}
