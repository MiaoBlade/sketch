using Godot;
using System;

public class StrokeCollider
{
    public int RowCount = 0;
    float gridDim = 100;
    RowCollider entry = new RowCollider();//linklist root,next point to smallest id
    public StrokeCollider()
    {
    }
    public void InsertStrokePatch(StrokePatchCollider p)
    {
        RowCollider rs = findOrInsertRowStore(p.pos);
        rs.addStroke(p);
    }
    public void Collide(CollideObject c, bool remove_when_collide = true)
    {
        RowCollider rs = entry.next;
        while (rs != null)
        {
            rs.eraseCollide(c);
            rs = rs.next;

        }
    }
    public void Clear()
    {
        RowCollider start = entry.next;
        while (start != null)
        {
            start.clear();
            start.prev.next = null;
            start.prev = null;
            start = start.next;

        }
        RowCount = 0;
    }
    RowCollider findOrInsertRowStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.Y / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new RowCollider();
            entry.next.prev = entry;
            entry.next.ID = id;
            RowCount++;
            return entry.next;
        }
        RowCollider rs = entry.next;
        RowCollider newRS;
        while (rs != null)
        {
            if (rs.ID == id)
            {
                // found it
                return rs;
            }
            else if (rs.ID > id)
            {
                //insert here
                newRS = new RowCollider();
                newRS.ID = id;
                newRS.prev = rs.prev;
                newRS.next = rs;

                rs.prev.next = newRS;
                rs.prev = newRS;

                RowCount++;

                return newRS;
            }
            else
            {
                //jump next
                if (rs.next == null)
                {
                    break;
                }
                else
                {
                    rs = rs.next;
                }
            }
        }
        //we are the biggest,add
        newRS = new RowCollider();
        newRS.ID = id;
        newRS.prev = rs;
        rs.next = newRS;
        RowCount++;
        return newRS;
    }
}
