using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegionQT<T> where T : MonoBehaviour
{
    const int BUCKETSIZE = 4;

    class Node<U> where U : MonoBehaviour
    {
        public Node<U> NW;
        public Node<U> NE;
        public Node<U> SE;
        public Node<U> SW;

        public Vector2 Point;
        public U[] Bucket;

        public Node() { }

        public Node(List<U> objs)
        {
            Bucket = objs.ToArray();
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
        //_sw = new List<T>();
        //_nw = new List<T>();
        //_se = new List<T>();
        //_ne = new List<T>();
        _results = new List<T>();
    }

    public void Build(List<T> agents)
    {
        //TODO: recycle nodes

        _root = CreateNode(agents);
    }

    Node<T> CreateNode(List<T> agents)
    {
        if (agents.Count <= BUCKETSIZE)
        {
            return new Node<T>(agents); //nodes can be reused from a pool
        }

        Vector2 midPoint;
        midPoint.x = agents.Aggregate(0f, (agg, agent) => agg + agent.transform.position.x); //can be in for loop
        midPoint.y = agents.Aggregate(0f, (agg, agent) => agg + agent.transform.position.z);
        midPoint.x /= agents.Count;
        midPoint.y /= agents.Count;

        var node = new Node<T>();
        node.Point = midPoint;

        var _sw = new List<T>();
        var _nw = new List<T>();
        var _se = new List<T>();
        var _ne = new List<T>();

        for (int i = 0; i < agents.Count; i++)
        {
            var agent = agents[i];
            var pos = agent.transform.position;
            if (pos.x > midPoint.x)
            {
                if (pos.z > midPoint.y)
                    _ne.Add(agent);
                else
                    _se.Add(agent);
            }
            else
            {
                if (pos.z > midPoint.y)
                    _nw.Add(agent);
                else
                    _sw.Add(agent);
            }
        }

        if (_nw.Count > 0)
            node.NW = CreateNode(_nw);
        if (_ne.Count > 0)
            node.NE = CreateNode(_ne);
        if (_sw.Count > 0)
            node.SW = CreateNode(_sw);
        if (_se.Count > 0)
            node.SE = CreateNode(_se);

        return node;
    }


    public T[] AllInRegion(Vector2 pos, float radius)
    {
        _results.Clear();

        FindObjsInRegion(_root, pos, radius, radius * radius);

        return _results.ToArray();
    }

    void FindObjsInRegion(Node<T> node, Vector2 pos, float distance, float sqrDistance)
    {
        if (node.Bucket != null)
        {
            for (int i = 0; i < node.Bucket.Length; i++)
            {
                var dist = Vector2.SqrMagnitude(pos - node.Bucket[i].GetPos());
                if (dist < sqrDistance)
                    _results.Add(node.Bucket[i]);
            }
            return;
        }

        Direction directions = Direction.None;
        var pointsToCheck = new Vector2[]
        {
            new Vector2(pos.x + distance, pos.y+distance),
            new Vector2(pos.x + distance, pos.y-distance),
            new Vector2(pos.x - distance, pos.y-distance),
            new Vector2(pos.x - distance, pos.y+distance),
        };

        for (int i = 0; i < pointsToCheck.Length; i++)
        {
            var point = pointsToCheck[i];
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

    Node<T> _root;

    List<T> _results;
}

public static class MonoBehaviourExt
{
    public static Vector2 GetPos(this MonoBehaviour mono)
    {
        var pos = mono.transform.position;
        return new Vector2(pos.x, pos.z);
    }
}