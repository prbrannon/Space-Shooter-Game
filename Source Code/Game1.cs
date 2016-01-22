using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game2D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        int dead_spawn_delay = 121;

        bool end_of_game_is_over_now_and_fiveever_alone = false;

        int total_enemies;
        float wave_timer = 20;

        int number_lives = 3;

        Texture2D projectile_picture;
        Texture2D shrek_picture;
        Texture2D particle_picture;

        SoundEffect music;
        SoundEffectInstance sfx_instance;
        SoundEffect shot;
        SoundEffect player_shoot;
        SoundEffect player_death;

        Random random_generator = new Random();

        int SCREEN_WIDTH = 500;
        int SCREEN_HEIGHT = 500;

        int current_shoop_delay;
        const int invincibilityframes = 120;

        const float PI = (float) Math.PI;
        const float ship_velocity = 0.1f;
        const int SHOOP_DELAY = 10;
        const float projectile_velocity = 35f;
        float shrek_velocity = 1.0f;
        const float particle_velocity = 10.0f;
        const int number_score_digits = 10;

        bool InMenu = true;

        int enemies_killed = 0;
        int score;
        int multiplier = 0;

        SpriteGroup bullettes;
        SpriteGroup bad_dudes;
        SpriteGroup particles;
        SpriteGroup HUD;
        SpriteGroup Main_menu;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteGroup sprites;
        Sprite ship;
        Sprite crosshair;
        Sprite the_grid;
        Sprite player_score;
        Sprite player_lives;
        Sprite x;
        Sprite multi;
        Sprite timer;
        Sprite d_point;

        float rock_mass = 10.0f;  // kilograms
        float gravity_accel = 9.8f; // m/sec^2

        Vector3 rock_velocity = new Vector3(-2, 4, 0);  // meters per second
        float rock_angular_velocity = MathHelper.Pi;   // radians per second

        float ship_speed;
        float max_ship_speed = 2; // m/sec
        float ship_heading;
        float ship_angular_velocity = MathHelper.Pi;   // radians per second

        BoundingBox world_box = new BoundingBox(new Vector3(0, 0, 0), new Vector3(30, 30, 0));

        Camera camera;
        KeyboardState old_keyboard_state;
        MouseState old_mouse_state;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            

            Log.open("collision_log.txt");

        }

        // screen-related init tasks
        public void InitScreen(int W, int H)
        {
            Log.write_line("Entering InitScreen");

            // back buffer
            graphics.PreferredBackBufferHeight = H;
            graphics.PreferredBackBufferWidth = W;
            graphics.PreferMultiSampling = false;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            bullettes = new SpriteGroup();
            bad_dudes = new SpriteGroup();
            particles = new SpriteGroup();
            HUD = new SpriteGroup();
            Main_menu = new SpriteGroup();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new Camera(new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT),
                                world_box.Min, world_box.Max);

            Log.write_line("camera=" + camera);

            InitScreen(SCREEN_WIDTH, SCREEN_HEIGHT);

            Texture2D ship_picture = Content.Load<Texture2D>(@"media\ship");
            Texture2D crosshair_picture = Content.Load<Texture2D>(@"media\crosshair");
            Texture2D the_grid_picture = Content.Load<Texture2D>(@"media\the_grid");
            Texture2D digital_numbers_picture = Content.Load<Texture2D>(@"media\digitalNumbers");
            Texture2D start_button_picture = Content.Load<Texture2D>(@"media\start_button");
            Texture2D quit_button_picture = Content.Load<Texture2D>(@"media\quit_button");
            Texture2D x_picture = Content.Load<Texture2D>(@"media\X");
            Texture2D decimal_picture = Content.Load<Texture2D>(@"media\decimal_point");

            projectile_picture = Content.Load<Texture2D>(@"media\projectile");
            shrek_picture = Content.Load<Texture2D>(@"media\shrek");
            particle_picture = Content.Load<Texture2D>(@"media\particle");

            music = Content.Load<SoundEffect>(@"media\song");
            shot = Content.Load<SoundEffect>(@"media\shooting");
            player_shoot = Content.Load<SoundEffect>(@"media\gun");
            player_death = Content.Load<SoundEffect>(@"media\death");

            sfx_instance = music.CreateInstance();
            sfx_instance.IsLooped = true;
            sfx_instance.Play();

            sprites = new SpriteGroup();
            int group_ID = sprites.Group_ID;
            // Persistant Assets:
            the_grid = new Sprite(the_grid_picture, new Vector2(0, 0), group_ID);
            sprites.add_sprite(the_grid, true);
            the_grid.Origin = new Vector2(0, the_grid_picture.Height);
            the_grid.Scale = 0.6238f / camera.x_scale;

            ship = new Sprite(ship_picture, new Vector2(world_box.Max.X / 2, world_box.Max.Y / 2), group_ID);
            sprites.add_sprite(ship, true);
            ship.Scale = 0.05f;
            ship.Origin = new Vector2(ship_picture.Width / 2, ship_picture.Height / 2);

            crosshair = new Sprite(crosshair_picture, new Vector2(0, 0), group_ID);
            sprites.add_sprite(crosshair, true);
            crosshair.Scale = 1.5f / camera.x_scale;
            crosshair.Origin = new Vector2(crosshair_picture.Width / 2, crosshair_picture.Height / 2);

            // Main Menu:
            Sprite start_button = new Sprite(start_button_picture, new Vector2(0, 0), Main_menu.Group_ID);
            start_button.Scale = 1 / camera.x_scale;
            start_button.Position = new Vector3(0, start_button.Picture.Height * start_button.Scale, 0); // Left side of screen
            Main_menu.add_sprite(start_button, true);

            Sprite quit_button = new Sprite(quit_button_picture, new Vector2(0, 0), Main_menu.Group_ID);
            quit_button.Scale = 1 / camera.x_scale;
            quit_button.Position = new Vector3(world_box.Max.X - quit_button.Picture.Width * quit_button.Scale, quit_button.Picture.Height * quit_button.Scale, 0); // Right side of screen
            Main_menu.add_sprite(quit_button, true);

            // HUD:
            player_score = new Sprite(digital_numbers_picture, new Vector2(0, 0), HUD.Group_ID);
            player_score.Origin = new Vector2(0, digital_numbers_picture.Height);
            player_score.Scale = 1.7f / camera.x_scale;
            player_score.Frame_bounds = new Rectangle(0, 0, digital_numbers_picture.Width / 10, digital_numbers_picture.Height);
            player_score.Frameset_count = 10;
            player_score.Frameset_column_count = 10;
            HUD.add_sprite(player_score, true);

            player_lives = new Sprite(digital_numbers_picture, new Vector2(0, 0), HUD.Group_ID);
            player_lives.Origin = new Vector2(0, digital_numbers_picture.Height);
            player_lives.Scale = 1.7f / camera.x_scale;
            player_lives.Frame_bounds = new Rectangle(0, 0, digital_numbers_picture.Width / 10, digital_numbers_picture.Height);
            player_lives.Frameset_count = 10;
            player_lives.Frameset_column_count = 10;
            HUD.add_sprite(player_lives, true);

            x = new Sprite(x_picture, new Vector2(0, 0), HUD.Group_ID);
            x.Origin = new Vector2(0, x_picture.Height);
            x.Scale = 0.4f / camera.x_scale;
            HUD.add_sprite(x, true);

            multi = new Sprite(digital_numbers_picture, new Vector2(0, 0), HUD.Group_ID);
            multi.Origin = new Vector2(0, digital_numbers_picture.Height);
            multi.Scale = 0.4f / camera.x_scale;
            multi.Frame_bounds = new Rectangle(0, 0, digital_numbers_picture.Width / 10, digital_numbers_picture.Height);
            multi.Frameset_count = 10;
            multi.Frameset_column_count = 10;
            HUD.add_sprite(multi, true);

            timer = new Sprite(digital_numbers_picture, new Vector2(0, 0), HUD.Group_ID);
            timer.Origin = new Vector2(0, digital_numbers_picture.Height);
            timer.Scale = 0.4f / camera.x_scale;
            timer.Frame_bounds = new Rectangle(0, 0, digital_numbers_picture.Width / 10, digital_numbers_picture.Height);
            timer.Frameset_count = 10;
            timer.Frameset_column_count = 10;
            HUD.add_sprite(timer, true);

            d_point = new Sprite(decimal_picture, new Vector2(0, 0), HUD.Group_ID);
            d_point.Origin = new Vector2(0, decimal_picture.Height);
            d_point.Scale = 0.4f / camera.x_scale;
            HUD.add_sprite(d_point, true);

            Log.write_line("sprites=" + sprites);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState new_keyboard_state = Keyboard.GetState();

            MouseState mouse_state = Mouse.GetState();

            // Get mouse position:
            Vector3 mouse_Position = camera.get_world_point(mouse_state.X, mouse_state.Y);

            // Keyboard Updates

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                new_keyboard_state.IsKeyDown(Keys.Escape) ||
                new_keyboard_state.IsKeyDown(Keys.Q)
                )
                this.Exit();

            float dt = gameTime.ElapsedGameTime.Seconds +
                       gameTime.ElapsedGameTime.Milliseconds / 1000f +
                       gameTime.ElapsedGameTime.Minutes * 60f;

            if (new_keyboard_state.IsKeyDown(Keys.Up))
            {
                Vector3 ship_movement = new Vector3(ship.Position.X, ship.Position.Y + ship_velocity, 0);

                ship.Position = ship_movement;
            }
            else if (new_keyboard_state.IsKeyDown(Keys.Down))
            {
                Vector3 ship_movement = new Vector3(ship.Position.X, ship.Position.Y - ship_velocity, 0);

                ship.Position = ship_movement;
            }
            
            if (new_keyboard_state.IsKeyDown(Keys.Left))
            {
                Vector3 ship_movement = new Vector3(ship.Position.X - ship_velocity, ship.Position.Y, 0);

                ship.Position = ship_movement;
            }
            else if (new_keyboard_state.IsKeyDown(Keys.Right))
            {
                Vector3 ship_movement = new Vector3(ship.Position.X + ship_velocity, ship.Position.Y, 0);

                ship.Position = ship_movement;
            }

            ship_heading = (float)(Math.Atan2(ship.Position.Y - mouse_Position.Y, mouse_Position.X - ship.Position.X));
            ship.Angle = ship_heading;

            // Mouse Updates:

            // Update Crosshair Position
            crosshair.Position = mouse_Position;

            // Update Camera Position - Put camera halfway between ship and crosshair
            Vector2 next_camera_position = new Vector2((ship.Position.X + crosshair.Position.X) / 2,
                                                       (ship.Position.Y + crosshair.Position.Y) / 2);
            camera.move_to(next_camera_position);

            // Shoot if the player is holding down left click
            if (mouse_state.LeftButton == ButtonState.Pressed)
            {
                if (current_shoop_delay <= 0)
                {
                    Sprite projectile = new Sprite(projectile_picture, new Vector2(ship.Position.X, ship.Position.Y), bullettes.Group_ID);
                    projectile.Health = 1;
                    projectile.Damage = 1;
                    projectile.Angle = ship_heading;
                    projectile.Scale = 0.022f;
                    bullettes.add_sprite(projectile, true);
                    current_shoop_delay = SHOOP_DELAY;
                    player_shoot.Play();
                }
            }

            if (current_shoop_delay > 0)
                current_shoop_delay--;

            old_mouse_state = mouse_state;

            old_keyboard_state = new_keyboard_state;

            // Game Updates: A.I., Projectiles, Wave Spawning

            Vector3 ship_go = ship_speed * new Vector3((float)Math.Cos(-ship_angular_velocity), (float)Math.Sin(-ship_angular_velocity), 0);
            if (out_of_bounds_left(ship, ship_go, world_box))
            {
                float x = ship.Picture.Width * ship.Scale * 0.5f;
                ship.Position = new Vector3(x, ship.Position.Y, 0);
            }
            else if (out_of_bounds_right(ship, ship_go, world_box))
            {
                float x = world_box.Max.X - ship.Picture.Width * ship.Scale * 0.5f;
                ship.Position = new Vector3(x, ship.Position.Y, 0);
            }


            if (out_of_bounds_up(ship, ship_go, world_box))
            {
                float y = world_box.Max.Y - (ship.Picture.Height * ship.Scale * 0.5f);
                ship.Position = new Vector3(ship.Position.X, y, 0);
            }
            else if (out_of_bounds_down(ship, ship_go, world_box))
            {
                float y = ship.Picture.Height * ship.Scale * 0.5f;
                ship.Position = new Vector3(ship.Position.X, y, 0);
            }

            // Update Projectile Position

            for (int i = 0; i < bullettes.Count; i++)
            {
                Vector3 bullet_velocity = projectile_velocity * new Vector3((float)Math.Cos(-bullettes.get_sprite(i).Angle), (float)Math.Sin(-bullettes.get_sprite(i).Angle), 0);
                bullettes.get_sprite(i).Position += bullet_velocity * dt;
                if (out_of_bounds(bullettes.get_sprite(i), bullet_velocity, world_box))
                {
                    damage_bullette(i, 9001); // <---------OVER 9000!!!!!!!!!111@@#1337
                }
            }

            // Update Enemy Position:

            if (bad_dudes.Count != 0)
            {
                for (int i = 0; i < bad_dudes.Count; i++)
                {
                    if (bad_dudes.get_sprite(i).Invincibility > 0)
                    {
                        bad_dudes.get_sprite(i).Invincibility -= 1;
                        bad_dudes.get_sprite(i).Tint = Color.ForestGreen;
                        bad_dudes.get_sprite(i).Angle = (float)(Math.Atan2(bad_dudes.get_sprite(i).Position.Y - ship.Position.Y, ship.Position.X - bad_dudes.get_sprite(i).Position.X));
                    }
                    else
                    {
                        bad_dudes.get_sprite(i).Tint = Color.White;

                        Vector3 shrek_go;

                        bad_dudes.get_sprite(i).Angle = (float)(Math.Atan2(bad_dudes.get_sprite(i).Position.Y - ship.Position.Y, ship.Position.X - bad_dudes.get_sprite(i).Position.X));

                        shrek_go = shrek_velocity * new Vector3((float)Math.Cos(-bad_dudes.get_sprite(i).Angle), (float)Math.Sin(-bad_dudes.get_sprite(i).Angle), 0);

                        bad_dudes.get_sprite(i).Position += shrek_go * dt;
                    }
                }
            }

            // Update Particles:

            for (int i = 0; i < particles.Count; i++)
            {
                Vector3 dust_velocity = particle_velocity * new Vector3((float)Math.Cos(-particles.get_sprite(i).Angle), (float)Math.Sin(-particles.get_sprite(i).Angle), 0);
                particles.get_sprite(i).Health -= 1;

                if (out_of_bounds_left(particles.get_sprite(i), dust_velocity, world_box))
                {
                    dust_velocity.X *= -1;
                    particles.get_sprite(i).Angle += PI;
                }
                else if (out_of_bounds_right(particles.get_sprite(i), dust_velocity, world_box))
                {
                    dust_velocity.X *= -1;
                    particles.get_sprite(i).Angle += PI;
                }


                if (out_of_bounds_up(particles.get_sprite(i), dust_velocity, world_box))
                {
                    dust_velocity.Y *= -1;
                    particles.get_sprite(i).Angle += PI;
                }
                else if (out_of_bounds_down(particles.get_sprite(i), dust_velocity, world_box))
                {
                    dust_velocity.Y *= -1;
                    particles.get_sprite(i).Angle += PI;
                }

                particles.get_sprite(i).Position += dust_velocity * dt;
            }

            // Check Collisions:

            // Menu boxes being shot
            if (InMenu)
            {
                for (int i = 0; i < bullettes.Count; i++)
                {
                    if (bullettes.get_sprite(i).pixel_collision(Main_menu.get_sprite(0))) // Start
                    {
                        game_is_start();
                    }

                    else if (bullettes.get_sprite(i).pixel_collision(Main_menu.get_sprite(1))) // Quit
                    {
                        Exit(); // Close program
                    }
                }
            }

            else
            {
                if (dead_spawn_delay != 0)
                {
                    dead_spawn_delay--;
                }
                else
                {
                    ship.Tint = Color.White;

                    // Spawn Standard
                    if (bad_dudes.Count <= 0 || wave_timer < 0)
                    {
                        if (bad_dudes.Count <= 0)
                        {
                            if (multiplier < 9)
                                multiplier++;
                        }
                        spawn_wave(5);
                        enemies_killed = 0;
                        wave_timer = 20;
                    }
                }

                // Projectiles -> Enemies
                for (int i = 0; i < bullettes.Count; i++)
                {
                    for (int j = 0; j < bad_dudes.Count; j++)
                    {
                        if (bad_dudes.get_sprite(j).Invincibility <= 0 && bullettes.get_sprite(i).pixel_collision(bad_dudes.get_sprite(j)))
                        {
                            CreateExplosion(bad_dudes.get_sprite(j).Position, 10);
                            damage_bullette(i, bad_dudes.get_sprite(j).Damage);
                            damage_bad_dude(j, bullettes.get_sprite(i).Damage);
                        }
                    }
                }

                // Player -> Enemies

                for (int j = 0; j < bad_dudes.Count; j++)
                {
                    if (bad_dudes.get_sprite(j).Invincibility <= 0 && ship.pixel_collision(bad_dudes.get_sprite(j)))
                    {
                        player_deed();
                        break;
                    }
                }



                // Enemies -> Enemies
                for (int i = 0; i < bad_dudes.Count; i++)
                {
                    for (int j = i + 1; j < bad_dudes.Count; j++)
                    {
                        if (bad_dudes.get_sprite(i).spheres_meet(bad_dudes.get_sprite(j)))
                        {
                            // Fix collision:
                            Vector3 center_distance = new Vector3(bad_dudes.get_sprite(i).Position.X - bad_dudes.get_sprite(j).Position.X,
                                                                  bad_dudes.get_sprite(i).Position.Y - bad_dudes.get_sprite(j).Position.Y,
                                                                  0);

                            float magnitude = (bad_dudes.get_sprite(i).Radius + bad_dudes.get_sprite(j).Radius) - center_distance.Length();
                            center_distance.Normalize();

                            float combined_mass = bad_dudes.get_sprite(i).Radius + bad_dudes.get_sprite(j).Radius;
                            bad_dudes.get_sprite(i).Position += center_distance * ((bad_dudes.get_sprite(i).Radius / combined_mass) * magnitude);
                            bad_dudes.get_sprite(j).Position -= center_distance * ((bad_dudes.get_sprite(j).Radius / combined_mass) * magnitude);
                        }
                    }
                }

                wave_timer -= 0.016f;

                // Cleanup DAMAGED DUDES
                kill_dead_things();
            }
                base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            sprites.Draw(spriteBatch, camera, Matrix.Identity, 0);

            if (!InMenu)
            {
                DrawHUD();
            }
            else
            {
                DrawMenu();
                if (end_of_game_is_over_now_and_fiveever_alone)
                {
                    Draw_Final_Score();
                }
            }

            bullettes.Draw(spriteBatch, camera, Matrix.Identity, 0);
            bad_dudes.Draw(spriteBatch, camera, Matrix.Identity, 0);
            particles.Draw(spriteBatch, camera, Matrix.Identity, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            // Draw score in the bottom left of the screen:
            player_score.Position = new Vector3(world_box.Min.X + player_score.Picture.Width * player_score.Scale, world_box.Min.Y - player_score.Picture.Height * player_score.Scale, 0);

            for (int i = 1; i <= number_score_digits; i++)
            {
                int digit = score % (int) Math.Pow(10, i) / (int) Math.Pow(10, i - 1);

                player_score.Animation_index = digit;

                player_score.Position -= new Vector3(player_score.Picture.Width / 10 * player_score.Scale, 0, 0);

                HUD.Draw(spriteBatch, camera, Matrix.Identity, 0);
            }

            // Timer:
            timer.Position = new Vector3(crosshair.Position.X + 2, crosshair.Position.Y - 1.6f, 0);

            // Draw decimal numbers:
            timer.Animation_index = (int)(wave_timer * 10) % 10;
            HUD.Draw(spriteBatch, camera, Matrix.Identity, 0);

            d_point.Position = new Vector3(timer.Position.X - d_point.Picture.Width * d_point.Scale, timer.Position.Y, 0);

            timer.Position -= new Vector3(d_point.Picture.Width * d_point.Scale, 0, 0);

            // Draw whole numbers:
            for (int i = 1; i <= 2; i++)
            {
                int digit = (int) Math.Floor(wave_timer) % (int)Math.Pow(10, i) / (int)Math.Pow(10, i - 1);

                timer.Animation_index = digit;

                timer.Position -= new Vector3(timer.Picture.Width / 10 * timer.Scale, 0, 0);

                HUD.Draw(spriteBatch, camera, Matrix.Identity, 0);
            }

            // Player Lives
            player_lives.Position = new Vector3(world_box.Max.X - player_lives.Picture.Width / 10 * player_lives.Scale, world_box.Min.Y - player_score.Picture.Height * player_score.Scale, 0);
            player_lives.Animation_index = number_lives % 10;

            // Multiplier
            x.Position = new Vector3(crosshair.Position.X - 3, crosshair.Position.Y - 1.6f, 0);

            multi.Position = new Vector3(x.Position.X + x.Picture.Width * x.Scale, x.Position.Y, 0);
            multi.Animation_index = multiplier % 10;

            HUD.Draw(spriteBatch, camera, Matrix.Identity, 0);
        }

        private void DrawMenu()
        {
            for (int i = 0; i <= Main_menu.Count; i++)
            {
                Main_menu.Draw(spriteBatch, camera, Matrix.Identity, 0);
            }
        }

        private void Draw_Final_Score()
        {
            player_score.Position = new Vector3((world_box.Max.X + player_score.Picture.Width * player_score.Scale) / 2, (world_box.Max.Y + player_score.Picture.Height * player_score.Scale) / 2, 0);
            player_lives.Position = new Vector3(9001, 9001, 9001);
            multi.Position = new Vector3(9001, 9001, 9001);
            x.Position = new Vector3(9001, 9001, 9001);
            timer.Position = new Vector3(9001, 9001, 9001);


            for (int i = 1; i <= number_score_digits; i++)
            {
                int digit = score % (int)Math.Pow(10, i) / (int)Math.Pow(10, i - 1);

                player_score.Animation_index = digit;

                player_score.Position -= new Vector3(player_score.Picture.Width / 10 * player_score.Scale, 0, 0);

                HUD.Draw(spriteBatch, camera, Matrix.Identity, 0);
            }
        }

        private void spawn_shrek(float starting_X, float starting_Y)
        {
            Sprite shrek = new Sprite(shrek_picture, new Vector2(starting_X, starting_Y), bad_dudes.Group_ID);
            shrek.Health = 1;
            shrek.Damage = 1;
            shrek.Invincibility = invincibilityframes;
            shrek.Scale = 0.025f;
            shrek.Origin = new Vector2(shrek.Picture.Width / 2, shrek.Picture.Height / 2);
            bad_dudes.add_sprite(shrek, true);

            return;
        }

        private void damage_bullette(int index, int damage)
        {
            bullettes.get_sprite(index).Health -= damage;
        }

        private void damage_bad_dude(int index, int damage)
        {
            bad_dudes.get_sprite(index).Health -= damage;
        }

        private void kill_dead_things()
        {
            int index;

            index = 0;
            while(index < bullettes.Count)
            {
                if (bullettes.get_sprite(index).Health <= 0)
                {
                    bullettes.remove_sprite(index);
                    index--;
                }
                index++;
            }

            index = bad_dudes.Count - 1;
            while (index >= 0)
            {
                if (bad_dudes.get_sprite(index).Health <= 0)
                {
                    bad_dudes.remove_sprite(index);
                    enemies_killed++;
                    score += 1 * multiplier;
                    shot.Play();
                }
                index--;
            }

            index = 0;
            while (index < particles.Count)
            {
                if (particles.get_sprite(index).Health <= 0)
                {
                    particles.remove_sprite(index);
                    index--;
                }
                index++;
            }
        }

        private void player_deed()
        {
            player_death.Play();
            for (int i = 0; i < bad_dudes.Count; i++)
            {
                CreateExplosion(bad_dudes.get_sprite(i).Position, 10);
            }
            number_lives--;
            if (number_lives < 0)
                game_its_over();
            dead_spawn_delay = 121;
            multiplier = 0;
            wave_timer = 20;
            shrek_velocity += 0.1f;
            bullettes.remove_all();
            bad_dudes.remove_all();
            ship.Tint = Color.Gray;
        }

        private bool out_of_bounds(Sprite sprite, Vector3 velocity, BoundingBox box)
        {
            return out_of_bounds_left(sprite, velocity, box) ||
                   out_of_bounds_right(sprite, velocity, box) ||
                   out_of_bounds_up(sprite, velocity, box) ||
                   out_of_bounds_down(sprite, velocity, box);
        }

        private bool out_of_bounds_left(Sprite sprite, Vector3 velocity, BoundingBox box)
        {
            return sprite.Position.X - sprite.Picture.Width * 0.5f * sprite.Scale < box.Min.X;
        }

        private bool out_of_bounds_right(Sprite sprite, Vector3 velocity, BoundingBox box)
        {
            return sprite.Position.X + (sprite.Picture.Width * 0.5f * sprite.Scale) > box.Max.X;
        }

        private bool out_of_bounds_up(Sprite sprite, Vector3 velocity, BoundingBox box)
        {
            return sprite.Position.Y + (sprite.Picture.Height * 0.5f * sprite.Scale) > box.Max.Y;
        }

        private bool out_of_bounds_down(Sprite sprite, Vector3 velocity, BoundingBox box)
        {
            return sprite.Position.Y - (sprite.Picture.Height * 0.5f * sprite.Scale) < box.Min.Y;
        }

        private void bounce_off_walls(ref Sprite sprite, ref Vector3 velocity, BoundingBox box)
        {
            bool collided = false;
            if (out_of_bounds_left(sprite, velocity, box) ||
                out_of_bounds_right(sprite, velocity, box))
            {
                velocity.X *= -1;
                collided = true;
            }
            else if (out_of_bounds_up(sprite, velocity, box) ||
               out_of_bounds_down(sprite, velocity, box))
            {
                velocity.Y *= -1;
                collided = true;
            }

            if (collided)
            { // advance object in new direction, making sure it clears the walls.
                sprite.Position += velocity;
            }
        }

        // Create random particles branching from a central point:
        private void CreateExplosion(Vector3 explosion_center, int num_particles)
        {
            Color c = new Color(random_generator.Next(255), random_generator.Next(255), random_generator.Next(255), 125);
            for (int i = 0; i < num_particles; i++)
            {
                Sprite particle = new Sprite(particle_picture, new Vector2(explosion_center.X, explosion_center.Y), particles.Group_ID);
                particle.Scale = 0.01f;
                int particle_angle = random_generator.Next(360); // Angle in degrees
                particle.Angle = particle_angle * PI / 180;
                particle.Health = 30 + random_generator.Next(60); // Particle last between 0.5 and 1.5 seconds
                particle.Tint = c;
                particles.add_sprite(particle, true);
            }
        }

        private void spawn_wave(int incriment)
        {
            int randomX;
            int randomY;
            bool colided;
            if (total_enemies <= 25)
            {
                total_enemies += incriment;
                for (int i = 0; i < total_enemies; i++)
                {
                    colided = false;
                    do
                    {
                        randomX = random_generator.Next((int)world_box.Max.X);
                        randomY = random_generator.Next((int)world_box.Max.Y);
                        spawn_shrek(randomX, randomY);
                        if (ship.pixel_collision(bad_dudes.get_sprite(bad_dudes.Count - 1)))
                        {
                            colided = true;
                            bad_dudes.remove_sprite(bad_dudes.Count - 1);
                        }
                        else
                            colided = false;
                    } while (colided);
                }
            }
            else
            {
                shrek_velocity += 0.3f;
                for (int i = 0; i < total_enemies; i++)
                {
                    colided = false;
                    do
                    {
                        randomX = random_generator.Next((int)world_box.Max.X);
                        randomY = random_generator.Next((int)world_box.Max.Y);
                        spawn_shrek(randomX, randomY);
                        if (ship.pixel_collision(bad_dudes.get_sprite(bad_dudes.Count - 1)))
                        {
                            colided = true;
                            bad_dudes.remove_sprite(bad_dudes.Count - 1);
                        }
                        else
                            colided = false;
                    } while (colided);
                }
            }
        }

        private void game_is_start()
        {
            InMenu = false;
            total_enemies = 0;
            number_lives = 3;
            multiplier = 0;
            score = 0;
            shrek_velocity = 1;
            end_of_game_is_over_now_and_fiveever_alone = false;
        }

        private void game_its_over()
        {
            InMenu = true;
            end_of_game_is_over_now_and_fiveever_alone = true;
        }
    }
}
