using Godot;
using System;
using System.Collections.Generic;
class CellStore
{
    public int elemCount = 0;
    public int ID = 0;
    public CellStore next = null;
    public CellStore prev = null;
    public List<StrokeElement> elems = new List<StrokeElement>();
    public void addStroke(StrokeElement elem)
    {
        elems.Add(elem);
        elemCount++;
    }
}
class RowStore
{
    public int elemCount = 0;
    public int CellCount = 0;
    float gridDim = 100;
    public int ID = 0;
    public RowStore next = null;
    public RowStore prev = null;
    public CellStore entry = new CellStore();
    public void clear()
    {

    }
    public void addStroke(StrokeElement elem)
    {
        elemCount++;
        CellStore cs = findOrInsertCellStore(elem.pos);
        cs.addStroke(elem);
    }
    CellStore findOrInsertCellStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.X / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new CellStore();
            entry.next.prev = entry;
            entry.next.ID = id;
            CellCount++;
            return entry.next;
        }
        CellStore cs = entry.next;
        CellStore newCS;
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
                newCS = new CellStore();
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
        newCS = new CellStore();
        newCS.ID = id;
        newCS.prev = cs;
        cs.next = newCS;
        CellCount++;
        return newCS;
    }
}
public class StrokeStore
{
    float gridDim = 100;
    int bufferStride = 8;
    public int elemCount = 0;
    public int RowCount = 0;

    public int capacity = 8 * 1024;
    public float[] buffer;
    public StrokeElement lastStrokeElement;

    RowStore entry = new RowStore();//linklist root,next point to smallest id
    public StrokeStore()
    {
        buffer = new float[capacity * bufferStride];
    }

    public void addStroke(StrokeElement elem)
    {
        if ((elemCount + 1) * bufferStride >= buffer.Length)
        {
            //resize
            capacity *= 2;
            Array.Resize<float>(ref buffer, capacity * bufferStride);
        }
        lastStrokeElement=elem;

        var trans = Transform2D.Identity.Translated(elem.pos).RotatedLocal(elem.dir).ScaledLocal(elem.size);
        int pos = elemCount * bufferStride;
        buffer[pos] = trans.X.X;
        buffer[pos + 1] = trans.Y.X;
        buffer[pos + 2] = 0;
        buffer[pos + 3] = trans.Origin.X;
        buffer[pos + 4] = trans.X.Y;
        buffer[pos + 5] = trans.Y.Y;
        buffer[pos + 6] = 0;
        buffer[pos + 7] = trans.Origin.Y;

        elem.ID = elemCount;

        RowStore rs = findOrInsertRowStore(elem.pos);
        rs.addStroke(elem);
        elemCount++;
    }

    public void clear()
    {
        RowStore start = entry.next;
        while (start != null)
        {
            start.clear();
            start.prev.next = null;
            start.prev = null;
            start = start.next;

        }
        RowCount = 0;
        elemCount = 0;
    }

    public void eraseCollide()
    {

    }
    RowStore findOrInsertRowStore(Vector2 pos)
    {
        int id = Mathf.FloorToInt(pos.Y / gridDim);
        if (entry.next == null)
        {
            //we dont have any rowstore,just add new one
            entry.next = new RowStore();
            entry.next.prev = entry;
            entry.next.ID = id;
            RowCount++;
            return entry.next;
        }
        RowStore rs = entry.next;
        RowStore newRS;
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
                newRS = new RowStore();
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
        newRS = new RowStore();
        newRS.ID = id;
        newRS.prev = rs;
        rs.next = newRS;
        RowCount++;
        return newRS;
    }
}
