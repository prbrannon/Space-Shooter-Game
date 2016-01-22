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
    // This is a layout manager.
    // Allows multiple Rectangles to be placed next to each other,
    // and computes their positions, based on their relative positions
    // and separation
    public class BoxPacker
    {
        public enum Position
        {
            Top_Left, Top_Middle, Top_Right,
            Left_Middle, Right_Middle,
            Bottom_Left, Bottom_Middle, Bottom_Right
        };

        List<BoundingBox> boxes = new List<BoundingBox>();
        BoundingBox zero_box = new BoundingBox(Vector3.Zero, Vector3.Zero);
        BoundingBox collective_box;

        public BoxPacker(float x, float y)
        {
            collective_box = new BoundingBox(new Vector3(x,y, 0),
                                             new Vector3(x,y, 0));
            place_box(x,y);
        }
        
        static Vector3 point_from_pos(BoundingBox box, Position pos)
        {
            float dx = 0;
            float dy = 0;
            switch (pos) {
            case Position.Top_Left:                 dy = 1.0f; break;
            case Position.Top_Middle:    dx = 0.5f; dy = 1.0f; break;
            case Position.Top_Right:     dx = 1.0f; dy = 1.0f; break;
            case Position.Left_Middle:              dy = 0.5f; break;
            case Position.Right_Middle:  dx = 1.0f; dy = 0.5f; break;
            case Position.Bottom_Left:                         break;
            case Position.Bottom_Middle: dx = 0.5f;            break;
            case Position.Bottom_Right:  dx = 1.0f;            break;
            }
            Vector3 point = new Vector3(box.Min.X + dx * (box.Max.X - box.Min.X),
                                        box.Min.Y + dy * (box.Max.Y - box.Min.Y),
                                        0);

            return point;
        }



        public static void attach_box(BoundingBox old_box, float width, float height,
                                      out BoundingBox new_box, out BoundingBox merged_box)
        {
            BoxPacker.attach_box(old_box, width, height, out new_box, out merged_box,
                                 Position.Top_Right, Position.Top_Left);
        }

        public static void attach_box(BoundingBox old_box, float width, float height, 
                                      out BoundingBox new_box, out BoundingBox merged_box,
                                      Position old_pos, Position new_pos)
        {
            Vector3 old_point = point_from_pos(old_box, old_pos);
            BoundingBox origin_box = new BoundingBox(Vector3.Zero, new Vector3(width, height, 0));
            Vector3 new_point = point_from_pos(origin_box, new_pos);
            Vector3 corner = old_point - new_point;

            new_box = new BoundingBox(corner,
                                      corner + new Vector3(width, height, 0));

            merged_box = BoundingBox.CreateMerged(old_box, new_box);

        }


        public int place_box(float width, float height)
        {
            return place_box(width, height, last_index(), Position.Top_Right, Position.Top_Left);
        }

        public int place_box(float width, float height,
                             int neighbor_index,
                             Position neighbor_pos,
                             Position pos)
        {
            // get the neighbor next to whom we're placing the box
            BoundingBox neighbor;
            if (neighbor_index < 0)
            {
                // if there's no neighbor, start at the origin of the collective box
                neighbor = collective_box;
            }
            else
            {
                neighbor = boxes.ElementAt(neighbor_index);
            }

            BoundingBox merged_box;
            BoundingBox new_box;
            attach_box(neighbor, width, height, out new_box, out merged_box, neighbor_pos, pos);
            boxes.Add(new_box);
            collective_box = BoundingBox.CreateMerged(collective_box, new_box);

            return last_index();
        }

        int last_index()
        {
            return boxes.Count() - 1;
        }

        public BoundingBox get_box_at(int i)
        {
            return boxes.ElementAt(i);
        }

        public BoundingBox bounds()
        {
            return collective_box;
        }

        public void recenter()
        {
            Log.write_line("BoxPacker.recenter: collective_box=" + collective_box);

            float x0 = collective_box.Min.X;
            float y0 = collective_box.Min.Y;
            Vector3 offset = new Vector3(x0, y0, 0);

            collective_box = new BoundingBox();
            for (int i = 0; i < boxes.Count(); i++)
            {
                boxes[i] = new BoundingBox(boxes[i].Min - offset,
                                           boxes[i].Max - offset);
                collective_box = BoundingBox.CreateMerged(collective_box, boxes[i]);
            }
        }

        public override String ToString()
        {
            String result = "BoxPacker (" + boxes.Count() + " boxes) (bounds=" + collective_box + ")\n";
            for (int i = 0; i < boxes.Count(); i++)
                result += "  boxes[" + i + "]=" + boxes[i] + "\n";
            return result;
        }
    }
}