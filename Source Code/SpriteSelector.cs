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
    public class SpriteSelector
    {
        // convenience method (and associated fields) for picking a sprite with the mouse
        public SpriteGrid selected_grid;
        public int selected_row;
        public int selected_col;
        public Sprite selected_sprite;

        public bool mouse_is_on_grid_sprite(SpriteGrid grid, float x, float y, Sprite.Inside how_inside, Matrix transform)
        {
            Log.write_line("mouse_is_on_grid_sprite: (x y)=(" + x + " " + y + ")");

            if (grid.contains_world_point(x, y, transform))
            {
                Log.write_line("grid contains point");

                Vector3 group_point = grid.get_group_coords(x, y, transform);

                int arow, acol;
                grid.to_row_col(group_point.X, group_point.Y, out arow, out acol);
                Sprite sprite = grid.get_sprite_at_row_col(arow, acol);

                Log.write_line("sprite under mouse: " + sprite);

                if (sprite.contains_world_point(x, y, transform, how_inside))
                {
                    Log.write_line("sprite contains point");

                    selected_sprite = sprite;
                    selected_row = arow;
                    selected_col = acol;
                    selected_grid = grid;
                    return true;
                }
                else
                {
                    Log.write_line("sprite does NOT contain point");
                }
            }
            else
            {
                Log.write_line("grid does NOT contain point");
            }

            return false;
        }

        public Vector3 selected_corner_location()
        {
            return selected_grid.corner_position(selected_row, selected_col);
        }
    }
}
