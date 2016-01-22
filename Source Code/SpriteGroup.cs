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
    // This class implements a container for several Sprites,
    // all contained within a bounding box.
    //
    // The following operations are available:
    // - add a sprite to the set.
    // - remove a sprite from the set.
    // - return the sprite at a given (x y) position
    // - draw all the sprites in the set
    //
    // Usually adding a sprite will require giving it a position,
    // but the class can infer a position by placing sprites row
    // by row.
    //
    public class SpriteGroup
    {
        protected int group_ID;

        protected BoundingBox bounds;
        protected List<Sprite> sprites;

        private BoxPacker row_packer;

        protected static int max_group_ID = 0;

        public SpriteGroup() : this(new BoundingBox())
        {
        }

        public SpriteGroup(BoundingBox bounds)
        {
            group_ID = max_group_ID++;
            this.bounds = bounds;
            row_packer = new BoxPacker(0, 0);
            sprites = new List<Sprite>();
        }

        public int add_sprite(Texture2D picture)
        {
            Sprite sprite = new Sprite(picture, Vector2.Zero, group_ID);
            return add_sprite(sprite, false);
        }

        public virtual Vector3 new_sprite_position(int width, int height)
        {
            // where would sprite's right side be if we added it to active row?
            BoundingBox new_bounds;
            BoundingBox new_box;
            BoxPacker.attach_box(row_packer.bounds(), width, height,
                                 out new_bounds, out new_box);
            if (new_bounds.Max.X > bounds.Max.X)
            {   // new sprite runs off the right side of this row.
                // Start a new row.
                row_packer = new BoxPacker(row_packer.bounds().Min.X,
                                           row_packer.bounds().Max.Y);
            }
            int i = row_packer.place_box(width, height);
            BoundingBox bb = row_packer.get_box_at(i);
            return bb.Min - Vector3.UnitY * height;
        }

        public virtual int add_sprite(Sprite sprite, bool position_known)
        {
            if (!position_known)
                sprite.Position = new_sprite_position(sprite.Frame_bounds.Width,
                                                      sprite.Frame_bounds.Height);
            sprites.Add(sprite);
            return last_index();
        }

        public Sprite remove_sprite(int index)
        {
            Sprite temp = sprites[index];
            sprites.RemoveAt(index);
            return temp;
        }

        public void remove_all()
        {
            sprites.Clear();
        }

        public Sprite get_sprite(int index)
        {
            return sprites[index];
        }

        public virtual Sprite get_sprite_at(float x, float y, Matrix group_transform, Sprite.Inside how_inside)
        {
            Matrix inverse_group_transform = Matrix.Invert(group_transform);
            for (int i = 0; i < Count; i++)
            {
                if (sprites[i].contains_world_point(x,y, inverse_group_transform, how_inside))
                    return sprites[i];
            }
            return null;
        }

        public int Count
        {
            set { ;}
            get { return sprites.Count(); }
        }

        public int Group_ID
        {
            set { group_ID = value; }
            get { return group_ID; }
        }

        protected int last_index()
        {
            return Count - 1;
        }

        public Vector3 get_group_coords(float x, float y, Matrix group_transform)
        {
            Vector3 world_point = new Vector3(x, y, 0);
            return Vector3.Transform(world_point, Matrix.Invert(group_transform));
        }

        public bool contains_world_point(float x, float y, Matrix group_transform)
        {
            Vector3 group_point = get_group_coords(x, y, group_transform);
            return bounds.Contains(group_point) == ContainmentType.Contains;
        }


        public void Draw(SpriteBatch batch)
        {
            Draw(batch, Camera.Identity, Matrix.Identity, 0);
        }

        public void Draw(SpriteBatch batch,
                         Camera camera,
                         Matrix transformation,
                         float depth)
        {
            // First, unpack the transformation into three matrices
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            transformation.Decompose(out scale,
                                     out rotation,
                                     out translation);

//            Log.write_line("SpriteGroup.Draw");
//            Log.write_line("scale      =" + scale);
//            Log.write_line("rotation   =" + rotation);
//            Log.write_line("translation=" + translation);

            // and now, out of the the matrix, fish out the
            // the detailed parameters that specify it.  We need to do this
            // (although it's kinda clunky) because SpriteBatch doesn't
            // use Matrices, but relies on these parameters instead.
            //
            // So we need to move from the 3D way of specifying transformations
            // to the simpler 2D way that SpriteBatch uses.
            Vector3 axis = new Vector3(rotation.X, rotation.Y, rotation.Z);
            float angle = 0;
            if (axis.Length() > 0)
                 angle = -(float)Math.Acos(rotation.W) * 2 + MathHelper.Pi;

            Vector2 sprite_position = new Vector2(0, 0);
            Vector2 sprite_scale = new Vector2(1, 1);

            foreach (Sprite sprite in sprites)
            {
                Vector3 new_position = Vector3.Transform(sprite.Position,
                                                         transformation);
                Vector3 screen_position = camera.get_screen_point(new_position);
                sprite_position.X = screen_position.X;
                sprite_position.Y = screen_position.Y;
                sprite_scale.X = scale.X * camera.x_scale;
                sprite_scale.Y = scale.Y * camera.y_scale;

//                Log.write_line("SpriteGroup.Draw: sprite=" + sprite);
//                Log.write_line("angle=" + angle + " position=" + sprite_position + " scale=" + sprite_scale);

                sprite.Draw(batch, sprite_position, angle, sprite_scale, depth);
            }
        }

        public override String ToString()
        {
            String result = "SpriteGroup (ID=" + group_ID + ")\n";
            result += " bounds=" + bounds + "\n";
            for (int i = 0; i < sprites.Count(); i++)
                result += "  sprite[" + i + "]: " + sprites[i] + "\n";
            return result;
        }
    }
}
