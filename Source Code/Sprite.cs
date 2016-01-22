using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game2D
{
    public class Sprite
    {
        public enum Inside { Box, Circle, Foreground };

        #region data_members
        bool is_invincible;
        Texture2D picture;    // holds all the pixels
        Vector3 position;     // top-left position within the group
        float scale;        // stretch factor
        float depth;          // for visibility/occlusion
        Color tint;           // overall color
        float angle;          // rotation angle (clockwise)
        float opacity;        // for translucency effects
        Vector2 origin;       // center of rotation
        int health;
        int damage;
        int invincibility;

        // These are needed to handle animated sprite sheets
        // Our sheet will consist of a grid of images (frames) within the sprite
        int animation_index;       // the current frame
        Rectangle frame_bounds;    // the span of each frame
        int frameset_column_count; // frames per row
        int frameset_count;        // total frames in all rows.

        int group_ID;             // what group of sprites we belong to.
        #endregion

        #region constructors
        public Sprite(Texture2D p_picture, Vector2 p_position, int p_group_ID,
                      float p_scale, float p_depth,
                      Color p_tint, float p_angle, float p_opacity,
                      Vector2 p_origin, Rectangle p_frame_bounds,
                      int p_frameset_count)
        {
            Initialize(p_picture, p_position, p_group_ID, p_scale, p_depth, p_tint, p_angle,
                       p_opacity, p_origin, p_frame_bounds, p_frameset_count);
        }

        public Sprite(Texture2D p_picture, Vector2 p_position, int p_group_ID)
        {
            Initialize(p_picture, p_position, p_group_ID, 1, 0, Color.White, 0, 1, Vector2.Zero, p_picture.Bounds, 1);
        }

        public void Initialize(Texture2D p_picture, Vector2 p_position, int p_group_ID,
                               float p_scale, float p_depth,
                               Color p_tint, float p_angle, float p_opacity,
                               Vector2 p_origin, Rectangle p_frame_bounds,
                               int p_frameset_count)
        {
            is_invincible = false;
            picture = p_picture;
            position = new Vector3(p_position.X, p_position.Y, 0);
            group_ID = p_group_ID;
            scale = p_scale;
            depth = p_depth;
            tint = p_tint;
            angle = p_angle;
            opacity = p_opacity;
            origin = p_origin;
            frame_bounds = p_frame_bounds;
            frameset_column_count = p_picture.Width / p_frame_bounds.Width;
            frameset_count = p_frameset_count;

            animation_index = 0;
        }

        #endregion

        #region selection
        public bool contains_world_point(float x, float y,
                                         Matrix group_transformation_inverse,
                                         Inside containment_type)
        {
            Log.write_line("Sprite.contains_world_point: (x y)=(" + x + " " + y + ")");
            Log.write_line("transform=" + group_transformation_inverse + " contain=" + containment_type);

            Vector3 world_point = new Vector3(x, y, 0);                       // the point, in world coords
            Matrix world_to_object = group_transformation_inverse * inverse_placement_transform();
            // convert point to object coords
            Vector3 object_point = Vector3.Transform(world_point, world_to_object);

            switch (containment_type)
            {
                case Inside.Box:
                    return inside_box(object_point.X, object_point.Y);

                case Inside.Circle:
                    Vector3 center = new Vector3(frame_bounds.Width / 2, frame_bounds.Height / 2, 0);
                    float length = Vector3.Distance(object_point, center);
                    float half_diameter = Vector3.Distance(Vector3.Zero, center);
                    return length < half_diameter;

                case Inside.Foreground:
                    Color[] colors = new Color[1];
                    picture.GetData(colors, (int)(object_point.Y * frame_bounds.Width + object_point.X), 1);
                    return colors[0].A > 0;
            }
            return false;
        }

        protected bool inside_box(float x, float y)
        {
            return (0 < x && x < frame_bounds.Width &&
                    0 < y && y < frame_bounds.Height);
        }

        // Return the matrix which turns a world point into an object point.
        protected Matrix inverse_placement_transform()
        {
            return Matrix.Invert(placement_transform());
        }

        // Return the matrix which turns a world point into an object point.
        protected Matrix placement_transform()
        {
            Vector3 origin3D = new Vector3(origin.X, origin.Y, 0);
            Matrix result = Matrix.CreateTranslation(-origin3D) *  // move center of rotation
                            Matrix.CreateScale(scale, -scale, 1) * // flip y axis                            
                            Matrix.CreateRotationZ(-angle) *       // turn
                            Matrix.CreateTranslation(position);    // move to top-left point
            return result;
        }
        #endregion

        #region collision_detection
        public bool spheres_meet(Sprite other)
        {
            Matrix placement = placement_transform();
            Vector3 center = Vector3.Transform(new Vector3(picture.Width / 2, picture.Height / 2, 0), placement);
            Vector3 corner = Vector3.Transform(new Vector3(0, 0, 0), placement);
            float radius = (center - corner).Length();

            Matrix other_placement = other.placement_transform();
            Vector3 other_center = Vector3.Transform(new Vector3(other.picture.Width / 2, other.picture.Height / 2, 0), other_placement);
            Vector3 other_corner = Vector3.Transform(new Vector3(0, 0, 0), other_placement);
            float other_radius = (other_center - other_corner).Length();

            float distance = (center - other_center).Length();

            return distance < radius + other_radius;
        }

        public bool pixel_collision(Sprite other)
        {
            if (spheres_meet(other))
            {
                Matrix other_onto_this = other.placement_transform() * inverse_placement_transform();

                int w = picture.Width;
                int h = picture.Height;
                Color[] colors = new Color[w * h];
                picture.GetData(colors, 0, w * h);

                int ow = other.picture.Width;
                int oh = other.picture.Height;
                Color[] other_colors = new Color[ow * oh];
                other.picture.GetData(other_colors, 0, ow * oh);

                Vector3 this_pos;
                Vector3 other_pos = new Vector3();
                for (int x = 0; x < ow; x++)
                    for (int y = 0; y < oh; y++)
                        if (other_colors[x + y * ow].A != 0)
                        {
                            other_pos.X = x;
                            other_pos.Y = y;
                            this_pos = Vector3.Transform(other_pos, other_onto_this);
                            if (inside_box(this_pos.X, this_pos.Y))
                                if (colors[(int)this_pos.X + (int)this_pos.Y * w].A > 0)
                                    return true;
                        }
            }
            return false;
        }
        #endregion
        public void next_frame()
        {
            // this gets mod-ed to proper range, in Animation_Index property (below)
            Animation_index = Animation_index + 1;
        }

        #region properties
        public Texture2D Picture
        {
            set { picture = value; }
            get { return picture; }
        }

        public Vector3 Position
        {
            set { position = value; }
            get { return position; }
        }

        public float Scale
        {
            set { scale = value; }
            get { return scale; }
        }

        public bool Is_Invincible
        {
            set { is_invincible = value; }
            get { return is_invincible; }
        }
        public float Depth
        {
            set { depth = value; }
            get { return depth; }
        }

        public Color Tint
        {
            set { tint = value; }
            get { return tint; }
        }

        public float Angle
        {
            set { angle = value; }
            get { return angle; }
        }

        public float Opacity
        {
            set { opacity = value; }
            get { return opacity; }
        }

        public Vector2 Origin
        {
            set { origin = value; }
            get { return origin; }
        }

        public int Health
        {
            set { health = value; }
            get { return health; }
        }

        public int Damage
        {
            set { damage = value; }
            get { return damage; }
        }

        public int Invincibility
        {
            set { invincibility = value; }
            get { return invincibility; }
        }

        public int Animation_index
        {
            set { animation_index = value % frameset_count; }
            get { return animation_index; }
        }

        public Rectangle Frame_bounds
        {
            set { frame_bounds = value; }
            get { return frame_bounds; }
        }

        public int Frameset_column_count
        {
            set { frameset_column_count = value; }
            get { return frameset_column_count; }
        }

        public int Frameset_count
        {
            set
            {
                Log.write_line("Frameset_count: value=" + value);
                frameset_count = value;
            }
            get { return frameset_count; }
        }

        public float Radius
        {
            get
            {
                Matrix placement = placement_transform();
                Vector3 center = Vector3.Transform(new Vector3(picture.Width / 2, picture.Height / 2, 0), placement);
                Vector3 corner = Vector3.Transform(new Vector3(0, 0, 0), placement);
                float radius = (center - corner).Length();

                return radius;

                /*
                if (picture.Width > picture.Height)
                {
                    return picture.Width * scale;
                }
                else
                {
                    return picture.Height * scale;
                }
                 * */
            }
        }
        #endregion
        #region drawing
        public void Draw(SpriteBatch batch)
        {
            Draw(batch, new Vector2(position.X, position.Y), 0, new Vector2(1, 1), 0);
        }

        public void Draw(SpriteBatch batch,
                         Vector2 p_position,
                         float p_angle,
                         Vector2 p_scale,
                         float p_depth)
        {
            //            // Move the sprite according to the translation (tx ty) passed in
            //           Vector2 new_position = new Vector2(position.X + tx,
            //                                               position.Y + ty);

            // Now, work out the frame within the sprite that should
            // be drawn (for animation of sprite sheets)
            int row = (animation_index % frameset_count) / frameset_column_count;
            int col = (animation_index % frameset_count) % frameset_column_count;

            int x_offset = frame_bounds.Width * col;
            int y_offset = frame_bounds.Height * row;
            Rectangle displayed_bounds = new Rectangle(x_offset, y_offset,
                                                       frame_bounds.Width,
                                                       frame_bounds.Height);

            // And draw.
            batch.Draw(
                picture,            // the sprite texture
                p_position,         // location to draw the picture
                displayed_bounds,   // bounds of our sprite within texture
                tint,               // tint (its alpha channel handles opacity too)
                angle + p_angle,    // rotation angle (radians)
                origin,             // the center of the texture (for rotation)
                scale * p_scale,    // size factor
                SpriteEffects.None, // draw sprite normally
                depth + p_depth     // depth, for layering effects
               );
        }
        #endregion

        public override string ToString()
        {
            String result = "Sprite (";
            result += " position=" + position;
            result += " tint=" + tint;
            result += " origin=" + origin;
            result += " scale=" + scale;
            result += " depth=" + depth;
            result += " anim_index=" + animation_index;
            return result;
        }
    }
}
