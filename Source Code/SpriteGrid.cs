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
    // Describes a set of sprites, laid out on a regular grid
    public class SpriteGrid : SpriteGroup
    {
        
        int grid_w, grid_h;
        float cell_width, cell_height;
        private bool fit_sprites_to_cells = false;
        private bool rows_increase_downwards = true;

        public SpriteGrid(BoundingBox box, int grid_w, int grid_h) : base(box)
        {
            this.grid_w = grid_w;
            this.grid_h = grid_h;

            float width  = box.Max.X - box.Min.X;
            float height = box.Max.Y - box.Min.Y;
            cell_width  =  width  / grid_w; 
            cell_height  = height / grid_h;
        }

        public bool Fit_Sprites_To_Cells
        {
            get { return fit_sprites_to_cells; }
            set { fit_sprites_to_cells = value; }
        }

        public bool Rows_Increase_Downwards
        {
            get { return rows_increase_downwards; }
            set { rows_increase_downwards = value; }
        }

        public override Vector3 new_sprite_position(int width, int height)
        {
            int row, col;
            get_row_col(last_index() + 1, out row, out col);
            return corner_position(row, col);
        }

        public override int add_sprite(Sprite sprite, bool position_known)
        {
            sprite.Position = new_sprite_position(sprite.Frame_bounds.Width,
                                                  sprite.Frame_bounds.Height);
            if (fit_sprites_to_cells)
            {
                float x_scale = cell_width  / sprite.Frame_bounds.Width  * sprite.Scale;
                float y_scale = cell_height / sprite.Frame_bounds.Height * sprite.Scale;
                sprite.Scale = MathHelper.Min(x_scale, y_scale);
            }
            sprites.Add(sprite);
            return last_index();
        }

        public Sprite remove_sprite(int row, int col)
        {
            return base.remove_sprite(row * grid_w + col);
        }

        // input (x y): group coords of a point
        // output (row col): grid coords of point
        public void to_row_col(float x, float y, out int row, out int col)
        {
            if (rows_increase_downwards)
            {
                Log.write_line("(bounds.Max.Y - y) / cell_height)=" + (bounds.Max.Y - y) / cell_height);

                row = (int)((bounds.Max.Y - y) / cell_height);
            }
            else
            {
                row = (int)((y - bounds.Min.Y) / cell_height);
            }
            col = (int)((x - bounds.Min.X) / cell_width);

            Log.write_line("to_row_col: (x y)=(" + x + " " + y + "), bounds=" + bounds);
            Log.write_line("cell (w h)=(" + cell_width + " " + cell_height + ") (row col)=(" + row + " " + col + ")");

        }

        // input (index): cell index (increases left-to-right, row by row)
        // output (row col): grid coords of cell
        private void get_row_col(int index, out int row, out int col)
        {
            row = index / grid_w;
            col = index % grid_w;
        }

        // input (row col): grid coords of a cell
        // output (x y): group coords of the cell's top-left corner.
        public Vector3 corner_position(int row, int col)
        {
            float x = bounds.Min.X + col * cell_width;
            float y;
            if (rows_increase_downwards)
            {
                y = bounds.Max.Y - row * cell_height;
            }
            else
            {
                y = bounds.Min.Y + (row + 1) * cell_height;
            }

//            Log.write_line("SpriteGrid.corner_position: (row col)=(" + row + " " + col + ") (x y)=(" + x + " " + y + ")");

            return new Vector3(x,y,0);
        }

        public Sprite get_sprite_at_row_col(int row, int col)
        {
            int index = row * grid_w + col;
            return get_sprite(index);
        }

        // input (x y): world coords of a point
        // returns : the sprite under the point, or null if none
        public override Sprite get_sprite_at(float x, float y, Matrix group_transform, Sprite.Inside how_inside)
        {
            Vector3 group_point = get_group_coords(x,y, group_transform);
            int row, col;
            to_row_col(group_point.X, group_point.Y, out row, out col);
            if (0 <= row && row < grid_w &&
                0 <= col && col < grid_h)
            {
                int index = row * grid_w + col;
                Matrix inverse_group_transform = Matrix.Invert(group_transform);
                if (sprites[index].contains_world_point(x,y, inverse_group_transform, how_inside))
                    return sprites[index];
            }
            return null;
        }

        
        public int n_rows
        {
            get { return grid_h; }
            set { grid_h = value; }
        }

        public int n_cols
        {
            get { return grid_w; }
            set { grid_w = value; }
        }

        public override string ToString()
        {
            String result = "  SpriteGrid cell(w h)=(" + cell_width + " " + cell_height
                           + ") grid(w h)=(" + grid_w + " " + grid_h + "), ";
            return result + base.ToString();
        }
    }
}
