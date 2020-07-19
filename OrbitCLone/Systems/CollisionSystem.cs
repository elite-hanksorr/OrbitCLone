using ECS;
using Microsoft.Xna.Framework;
using OrbitCLone.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitCLone.Systems
{
    class CollisionSystem : ComponentSystem
    {
        public override void OnUpdate(GameTime gt)
        {
            // Update everyone's collider.
            Entities.ForEach((ref CircleCollider c, ref Position p) =>
            {
                c.Collider = new BoundingSphere(new Vector3(p.x, p.y, 0), c.Radius);
            });

            // Test players for intersection with enemies.
            Entities.ForEach((ref CircleCollider c, ref PlayerTag pt, ref Entity e) =>
            {
                var player_collider = c;
                var player_entitiy = e;
                Entities.ForEach((ref CircleCollider enemy_collider, ref EnemyTag et) =>
                {
                    if(player_collider.Collider.Intersects(enemy_collider.Collider))
                    {
                        entityManager.RequestAction(entityManager.DeleteEntity, player_entitiy);
                    }
                });
            });

            // Test players for intersection with black hole.
            Entities.ForEach((ref CircleCollider c, ref PlayerTag pt, ref Entity e) =>
            {
                var player_collider = c;
                var player_entity = e;
                Entities.ForEach((ref CircleCollider bh_collider, ref BlackHoleTag bt) =>
                {
                    if (player_collider.Collider.Intersects(bh_collider.Collider))
                    {
                        entityManager.RequestAction(entityManager.DeleteEntity, player_entity);
                    }
                });
            });

            // Test enemies for containment by black hole.
            Entities.ForEach((ref CircleCollider c, ref EnemyTag et, ref Entity e) =>
            {
                var enemy_collider = c;
                var enemy_entity = e;
                Entities.ForEach((ref CircleCollider bh_collider, ref BlackHoleTag bt) =>
                {
                    if (bh_collider.Collider.Contains(enemy_collider.Collider) == ContainmentType.Contains)
                    {
                        entityManager.RequestAction(entityManager.DeleteEntity, enemy_entity);
                    }
                });
            });
        }
    }
}
