using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Input;

namespace SteeringBehavior
{
    public class Particle
    {
        private Vector2 position;
        public ref Vector2 Position
        {
            get
            {
                return ref position;
            }
        }
        public Vector2 Target { get; }

        private Vector2 velocity;
        public ref Vector2 Velocity
        {
            get
            {
                return ref velocity;
            }
        }

        private Vector2 acceleration;
        public ref Vector2 Acceleration
        {
            get
            {
                return ref acceleration;
            }
        }

        public float radius = 2f;

        private float maxSpeed = 15f;

        private float maxForce = 1f;
        public Particle(Vector2 position, Vector2 target, Random rand)
        {
            Position = position;
            Target = target;

            Velocity = new Vector2(rand.Next(1, 3) * (rand.Next(0, 2) == 0 ? 1 : -1), rand.Next(-3, 3) * (rand.Next(0, 2) == 0 ? 1 : -1));
            Acceleration = new Vector2(0);
        }

        public void Update()
        {
            Position += Velocity;
            Velocity += Acceleration;

            Acceleration = Vector2.Zero;
        }
        public void Behaviors(Vector2 fleePosition)
        {
            var fleeForce = Flee(fleePosition);
            var arriveForce = Arrive(Target);

            fleeForce *= new Vector2(7);

            if (arriveForce == null && fleeForce == Vector2.Zero)
            {
                Velocity = Vector2.Zero;
            }

            if (arriveForce.HasValue)
            {
                ApplyForce(arriveForce.Value);
            }

            ApplyForce(fleeForce);
        }

        private Vector2 SetMag(Vector2 vectorToScale, float lengthToSet)
        {
            var length = vectorToScale.Length();
            if (length == 0)
            {
                return new Vector2(lengthToSet, lengthToSet);
            }
            var scale = lengthToSet / length;
            return new Vector2(vectorToScale.X * scale, vectorToScale.Y * scale);
        }
        private Vector2 Flee(Vector2 target)
        {
            var desired = target - position;
            if (desired.Length() >= 20)
            {
                return Vector2.Zero;
            }

            desired = SetMag(desired, maxSpeed);
            desired *= new Vector2(-1);

            var steering = desired - velocity;

            if (steering.Length() > maxForce)
            {
                steering = SetMag(steering, maxForce);
            }

            return steering;
        }

        private Vector2? Arrive(Vector2 target)
        {
            var desired = target - position;
            if (desired.LengthSquared() < 1)
            {
                return null;
            }

            float speed = maxSpeed;
            if (desired.Length() < 50)
            {
                speed /= 10;
            }
            desired = SetMag(desired, speed);

            var steering = desired - velocity;

            if (steering.Length() > maxForce)
            {
                steering = SetMag(steering, maxForce);
            }

            return steering;
        }
        private void ApplyForce(Vector2 force)
        {
            Acceleration += force;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawCircle(new CircleF(new Point2(Position.X - radius / 2, Position.Y - radius / 2), radius), 30, Color.White, radius);
        }
    }
}
