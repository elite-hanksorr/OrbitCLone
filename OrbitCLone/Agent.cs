using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using OrbitLearner;

namespace OrbitCLone
{
    class Agent : GameEntity
    {
        public Agent(Brain b)
        {
            radius = 400.0f;
            angle = 0.0f;
            speed = 2;
            Score = 0;
            brain = b;
            neareastPlanets = new List<(float, float, int)>(3);
        }

        public override void Update(GameTime gt)
        {

        }

        public void GetNearestPlanets()
        {

        }


        private List<(float, float, int)> neareastPlanets;
        private double angle;
        private float radius;
        private float speed;
        public int Score { get; }
        private Brain brain;
    }
}
