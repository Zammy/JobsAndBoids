using System;
using System.Collections.Generic;
using UnityEngine;

public class RegionQT<T> where T : MonoBehaviour
{
    const int BUCKETSIZE = 8;

    private class Node<U> where U : MonoBehaviour
    {
        public Node<U> NW;
        public Node<U> NE;
        public Node<U> SE;
        public Node<U> SW;

        public Vector2 Point;
        public U[] Bucket;

        public Node()
        {
            Bucket = new U[BUCKETSIZE];
        }

        internal Node<U> Fill(List<U> objs)
        {
            for (int i = 0; i < objs.Count && i < BUCKETSIZE; i++)
            {
                Bucket[i] = objs[i];
            }
            return this;
        }
    }

    [Flags]
    private enum Direction
    {
        None = 0,
        NE = 1,
        NW = 2,
        SE = 4,
        SW = 8
    }

    public RegionQT()
    {
        _results = new List<T>();
        _pointsToCheck = new Vector2[]
        {
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero
        };
        _nodePool = new Stack<Node<T>>();
        _listPool = new Stack<List<T>>();
    }

    public void Build(List<T> agents)
    {
        if (_root != null)
            RecycleNode(_root);
        _root = CreateNode(agents);
    }

    Node<T> CreateNode(List<T> agents)
    {
        if (agents.Count <= BUCKETSIZE)
        {
            return GetEmptyNode().Fill(agents);
        }

        Vector2 midPoint = Vector2.zero;
        for (int i = 0; i < agents.Count; i++)
        {
            Vector2 pos = agents[i].GetPos();
            midPoint.x += pos.x;
            midPoint.y += pos.y;
        }
        midPoint.x /= agents.Count;
        midPoint.y /= agents.Count;

        var sw = GetEmptyList();
        var nw = GetEmptyList();
        var se = GetEmptyList();
        var ne = GetEmptyList();

        for (int i = 0; i < agents.Count; i++)
        {
            var agent = agents[i];
            var pos = agent.transform.position;
            if (pos.x > midPoint.x)
            {
                if (pos.z > midPoint.y)
                    ne.Add(agent);
                else
                    se.Add(agent);
            }
            else
            {
                if (pos.z > midPoint.y)
                    nw.Add(agent);
                else
                    sw.Add(agent);
            }
        }

        var node = GetEmptyNode();
        node.Point = midPoint;

        if (nw.Count > 0)
            node.NW = CreateNode(nw);
        if (ne.Count > 0)
            node.NE = CreateNode(ne);
        if (sw.Count > 0)
            node.SW = CreateNode(sw);
        if (se.Count > 0)
            node.SE = CreateNode(se);

        RecycleList(nw);
        RecycleList(ne);
        RecycleList(sw);
        RecycleList(se);

        return node;
    }

    public T[] AllInRegion(Vector2 pos, float radius)
    {
        _results.Clear();

        FindObjsInRegion(_root, pos, radius, radius * radius);

        return _results.ToArray();
    }

    public T FindClosest(T to)
    {
        _results.Clear();

        var pos = to.GetPos();
        FindObjsInRegion(_root, pos, 20f, 400f);

        T closetsObj = default(T);
        float closetsDistance = float.MaxValue;
        for (int i = 0; i < _results.Count; i++)
        {
            var obj = _results[i];
            if (obj == to)
                continue;
            var otherPos = obj.GetPos();
            var sqrDst = Vector2.SqrMagnitude(pos - otherPos);
            if (sqrDst < closetsDistance)
            {
                closetsDistance = sqrDst;
                closetsObj = obj;
            }
        }
        return closetsObj;
    }

    void FindObjsInRegion(Node<T> node, Vector2 pos, float distance, float sqrDistance)
    {
        if (node.Bucket[0] != null)
        {
            for (int i = 0; i < node.Bucket.Length; i++)
            {
                if (node.Bucket[i] == null)
                    break;
                var dist = Vector2.SqrMagnitude(pos - node.Bucket[i].GetPos());
                if (dist < sqrDistance)
                    _results.Add(node.Bucket[i]);
            }
            return;
        }

        var directions = Direction.None;
        _pointsToCheck[0].x = pos.x + distance;
        _pointsToCheck[0].y = pos.y + distance;
        _pointsToCheck[1].x = pos.x + distance;
        _pointsToCheck[1].y = pos.y - distance;
        _pointsToCheck[2].x = pos.x - distance;
        _pointsToCheck[2].y = pos.y - distance;
        _pointsToCheck[3].x = pos.x - distance;
        _pointsToCheck[3].y = pos.y + distance;
        for (int i = 0; i < _pointsToCheck.Length; i++)
        {
            var point = _pointsToCheck[i];
            if (point.x > node.Point.x)
            {
                if (point.y > node.Point.y)
                    directions |= Direction.NE;
                else
                    directions |= Direction.SE;
            }
            else
            {
                if (point.y > node.Point.y)
                    directions |= Direction.NW;
                else
                    directions |= Direction.SW;
            }
        }

        if ((directions & Direction.NE) > 0 && node.NE != null)
            FindObjsInRegion(node.NE, pos, distance, sqrDistance);
        if ((directions & Direction.SE) > 0 && node.SE != null)
            FindObjsInRegion(node.SE, pos, distance, sqrDistance);
        if ((directions & Direction.SW) > 0 && node.SW != null)
            FindObjsInRegion(node.SW, pos, distance, sqrDistance);
        if ((directions & Direction.NW) > 0 && node.NW != null)
            FindObjsInRegion(node.NW, pos, distance, sqrDistance);
    }

    Node<T> GetEmptyNode()
    {
        if (_nodePool.Count > 0)
            return _nodePool.Pop();
        return new Node<T>();
    }

    void RecycleNode(Node<T> node)
    {
        for (int i = 0; i < BUCKETSIZE; i++)
            node.Bucket[i] = null;

        if (node.NW != null)
            RecycleNode(node.NW);
        if (node.NE != null)
            RecycleNode(node.NE);
        if (node.SE != null)
            RecycleNode(node.SE);
        if (node.SW != null)
            RecycleNode(node.SW);

        node.NW = null;
        node.NE = null;
        node.SE = null;
        node.SW = null;

        _nodePool.Push(node);
    }

    List<T> GetEmptyList()
    {
        if (_listPool.Count > 0)
            return _listPool.Pop();
        return new List<T>(8);
    }

    void RecycleList(List<T> list)
    {
        list.Clear();
        _listPool.Push(list);
    }

    Node<T> _root;

    //caches
    List<T> _results;
    Vector2[] _pointsToCheck;
    Stack<Node<T>> _nodePool;
    Stack<List<T>> _listPool;
}

public static class MonoBehaviourExt
{
    public static Vector2 GetPos(this MonoBehaviour mono)
    {
        var pos = mono.transform.position;
        return new Vector2(pos.x, pos.z);
    }
}