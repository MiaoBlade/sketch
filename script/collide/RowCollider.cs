using Godot;
using System;
using System.Collections.Generic;
class RowCollider
{
    public int elemCount = 0;
    public int CellCount = 0;
    float gridDim = 100;
    public int ID = 0;
    public RowCollider next = null;
    public RowCollider prev = null;
    public CellCollider entry = new CellCollider();
    public void clear()
    {
        CellCollider start = entry.next;
        while (start != null)
        {
            start.clear();
            start.prev.next = null;
            start.prev = null;
            start = start.next;

        }
        CellCount = 0;
        elemCount = 0;
    }
    public void eraseCollide(CollideObject co)
    {
        float lbound = ID * gridDim;
        float hbound = lbound + gridDim;

        if (co.cy + co.radius < lbound)
        {
            return;
        }
        if (co.cy - co.radius > hbound)
        {
            return;
        }
        if (co.start.Y + co.rs < lbound && co.end.Y + co.re < lbound)
        {
            return;
        }
        if (co.start.Y - co.rs > hbound && co.end.Y - co.re > hbound)
        {
            return;
        }
        CellCollider cs = entry.next;
        while (cs != null)
        {
            cs.eraseCollide(co);
            cs = cs.next;

        }
    }
    public void addStroke(StrokeElement elem)
    {
        elemCount++;
        CellCollider cs = findOrInsertCellStore(elem.pos);
        cs.addStroke(elem);
    }
    CellCollider findOrInsertCellStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.X / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new CellCollider();
            entry.next.prev = entry;
            entry.next.ID = id;
            CellCount++;
            return entry.next;
        }
        CellCollider cs = entry.next;
        CellCollider newCS;
        while (cs != null)
        {
            if (cs.ID == id)
            {
                // found it
                return cs;
            }
            else if (cs.ID > id)
            {
                //insert here
                newCS = new CellCollider();
                newCS.ID = id;
                newCS.prev = cs.prev;
                newCS.next = cs;

                cs.prev.next = newCS;
                cs.prev = newCS;
                CellCount++;
                return newCS;
            }
            else
            {
                //jump next
                if (cs.next == null)
                {
                    break;
                }
                else
                {
                    cs = cs.next;
                }
            }
        }
        //we are the biggest,add
        newCS = new CellCollider();
        newCS.ID = id;
        newCS.prev = cs;
        cs.next = newCS;
        CellCount++;
        return newCS;
    }
}