using System;
using System.Collections.Generic;
using CocosSharp;
using CocosDenshion;
using Microsoft.Xna.Framework.Input;

using Amazon.Device.GameController;

namespace SpaceGameCommon
{
    public class GameLayer : CCLayerColor
    {
        const float SHIP_SPEED = 500.0f;
        const float SHOT_RADIUS = 7.5f;
        const float ENEMY_RADIUS = 40.0f;

        const int SHIP_UP_TAG = 1;
        const int SHIP_DOWN_TAG = 2;

        CCDrawNode ship;
        CCNode shots;
        CCNode enemies;
        CCParticleSystemQuad explosion;

        CCCallFuncN removeNodeAction = new CCCallFuncN (node => node.RemoveFromParent ());

        protected override void AddedToScene ()
        {
            base.AddedToScene ();

            Window.GamePadEnabled = true;

            shots = new CCNode ();
            enemies = new CCNode ();

            AddChild (shots);
            AddChild (enemies);

            CCRect bounds = VisibleBoundsWorldspace;

            AddStars (bounds);
                
            AddShip (bounds);

            Schedule (_ => Fire (), 0.1f);

            var touchListener = new CCEventListenerTouchAllAtOnce ();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            AddEventListener (touchListener, this);

            Schedule (_ => AddEnemy (), 0.5f);

            Schedule (_ => CheckEnemyCollisions ());

#if FIRESTICK
            Schedule (_ => HandleRightJoystick ());
#endif

            CCSimpleAudioEngine.SharedEngine.PlayEffect ("sounds/laser", true);

            InitExplosionParticles ();
        }

        public void HandleRightJoystick ()
        {
            GameController gameController = null;

            try {
                gameController = GameController.GetControllerByPlayer (1);


                float deltaX = gameController.GetAxisValue (GameControllerAxis.StickRightX);
                float deltaY = gameController.GetAxisValue (GameControllerAxis.StickRightY);

                MoveShip(deltaX, deltaY);
          
            } catch (GameController.PlayerNumberNotFoundException) {
            }
        }

        void MoveShip(float dx, float dy)
        {
            float speed = 10.0f;

            ship.Position = new CCPoint (ship.Position.X + speed * dx, ship.Position.Y - speed * dy);
        }

        void AddStars (CCRect bounds)
        {
            var stars = new CCParticleSystemQuad ("stars.plist");
            const float xOffset = 400.0f;
            stars.Position = new CCPoint (bounds.Center.X + xOffset, bounds.Center.Y);
            AddChild (stars);
        }

        void AddShip (CCRect bounds)
        {
            ship = new CCDrawNode ();

            CCPoint[] points = {
                new CCPoint (0, 25),
                new CCPoint (50, 0),
                new CCPoint (0, -25)
            };

            ship.DrawPolygon (points, points.Length, CCColor4B.White, 1.0f, CCColor4B.Orange);
            ship.Position = new CCPoint (bounds.MinX + 100.0f, bounds.MidY);
            AddChild (ship);

            var fire = new CCParticleFire (new CCPoint (-10.0f, 0.0f));
            fire.Rotation = -90.0f;
            fire.Scale = 0.5f;
            ship.AddChild (fire);
        }

        void Fire ()
        {
            var shot = new CCDrawNode ();

            shot.DrawCircle (new CCPoint (0, 0), SHOT_RADIUS, CCColor4B.Yellow);
            shots.AddChild (shot); 
            shot.Position = new CCPoint (ship.Position.X + 60.0f, ship.Position.Y - 2.5f);

            var moveShot = new CCMoveTo (1.0f, new CCPoint (VisibleBoundsWorldspace.MaxX, shot.Position.Y));
            shot.RunActions (moveShot, removeNodeAction);
        }

        void OnTouchesBegan (List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0) {
                CCTouch touch = touches [0];
                CCPoint touchLocation = touch.Location;

                MoveShipTo (new CCPoint (ship.Position.X, touchLocation.Y));
            }
        }

        void MoveShipTo (CCPoint location)
        {
            float ds = CCPoint.Distance (ship.Position, location);
            var dt = ds / SHIP_SPEED;

            var moveShip = new CCMoveTo (dt, location);
            var easeShip = new CCEaseSineInOut (moveShip);
            ship.RunAction (easeShip);
        }

        void AddEnemy ()
        {
            var enemy = new CCDrawNode ();
            enemy.DrawCircle (new CCPoint (0, 0), ENEMY_RADIUS, CCColor4B.White);
            enemy.Position = GetRandomPointY (ENEMY_RADIUS);
            enemies.AddChild (enemy);

            var moveEnemy = new CCMoveTo (3.0f, new CCPoint (0, enemy.Position.Y));
            enemy.RunActions (moveEnemy, removeNodeAction);
        }

        CCPoint GetRandomPointY (float r)
        {
            double rnd = CCRandom.NextDouble ();
            double randomY = (rnd > 0.0f) ? rnd * VisibleBoundsWorldspace.Size.Height - r : r;

            return new CCPoint ((VisibleBoundsWorldspace.Size.Width - r), (float)randomY);
        }

        void InitExplosionParticles ()
        {
            explosion = new CCParticleSystemQuad ("implode.plist");
            explosion.AutoRemoveOnFinish = false;
            explosion.StopSystem ();
            explosion.Visible = false;
            AddChild (explosion);
        }

        void CheckEnemyCollisions ()
        {
            if (enemies.ChildrenCount > 0 && shots.ChildrenCount > 0) {
                foreach (var enemy in enemies.Children) {
                    foreach (var shot in shots.Children) {

                        var d = CCPoint.Distance (shot.Position, enemy.Position);
                        if (d <= ENEMY_RADIUS + SHOT_RADIUS) {
                            var pt = enemy.Position;
                            enemy.RemoveFromParent (true);

                            CCSimpleAudioEngine.SharedEngine.PlayEffect ("sounds/explosion");

                            explosion.Visible = true;
                            explosion.Position = pt;
                            explosion.ResetSystem ();
                        }
                    }
                }
            }
        }

    }
}