﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class AStar  {
	GridMap map = GridMap.Map;
	StablePriorityQueue<Node> unvisited;
	HashSet<Node> visited;
	Dictionary<Node,Node> parent;

	//Get source and target Nodes from graph
	Node source;
	Node target;
	Node current;
	Vector2 targetDist;
	public void Search()
	{
		Debug.Log("Started A*");
		SetUp();
		Debug.Log("Finished setting up");
		Traverse();
		Debug.Log("Finished traversing");
	}

	// Add nodes to queue, set distances to infinity etc
	// Basic initialization
	public void SetUp()
	{
		/*		-> Creating a queue of size row* columns just in case we have to go through every node in the graph
		 * This is highly unlikely though since we are using A*
		 *		-> We use a HashSet for our visited list for rapid access times in the Contains() functionality. ie
		 *	to find out if a node exists in this visited set or not. This is so much faster than using Lists which
		 *	provide the same functionality but are much slower
		 *	
		 *		-> We use a Dictionary as another optimization technique. They have a very decent access time when
		 *	accessing data based on keys. This will help us trace our path rapidly and make changes to existing 
		 *	members easily. The option to make changes to existing nodes faster is the reason we are not using 
		 *	basic linked list. If not for that, linked lists would have been the fastest possible option.
		 *	
		 */
			 
		unvisited = new StablePriorityQueue<Node>(map.rows*map.columns);
		visited = new HashSet<Node>();
		parent = new Dictionary<Node, Node>();
		//Find source and start nodes
		source = map.graph[
							(int)map.start.transform.position.x,
							(int)map.start.transform.position.z
							];
		target = map.graph[
						(int)map.goal.transform.position.x,
						(int)map.goal.transform.position.z
							];
		targetDist = new Vector2(target.x, target.y);
		//Add start node to unvisited
		// Source has 0 gCost but we need to calculate it's hCost
		setHCost(source);
		unvisited.Enqueue(source, source.fCost);
	}

	public void Traverse()
	{
		//loop through while we still have nodes in unvisited
		while (unvisited.Count > 0)
		{
			current = unvisited.Dequeue();
			visited.Add(current);

			// We found the target node and now we can return
			if (current == target)
			{
				GeneratePath();
				Debug.Log("I actually came here in A*");
				return;
			}

			// traverse neighbors and calculate costs
			// Ideally in this loop you also check if the neighbor is traversable or not
			// But since we were smart and never included non traversable tiles as nodes we 
			// Do not need to worry about them since they will never show up as a neighbor
			foreach (Node neighbor in current.neighbors)
			{
				if (visited.Contains(neighbor))
				{
					// We already visited this one, so skip
					continue;
				}
				//If we are visiting a node for the very first time and it doesn't belong to the unvisited set
				// either, we need to set the gCost of that node to infinity because we do not know it yet
				// We already checked that it is not in visited so has to be the very first time we visit it
				if (!unvisited.Contains(neighbor))
				{
					neighbor.gCost = Mathf.Infinity;
				}
				// Cost incurred to go from current to neighbor accounting for
				// movement over rivers, ice, grass etc
				// If we never visited neighbor then it is automatically added
				float moveCost = current.gCost + current.GetCostToEnter(neighbor);
				if (moveCost < neighbor.gCost || !visited.Contains(neighbor))
				{
					// This means we found a shorter path to the neighbor so 
					// We use this now instead of the other one

					// To make the new path we update the fCost of the neighbor
					// With new values
					neighbor.gCost = moveCost;
					setHCost(neighbor);
					//Set/update parent now
					if(!parent.ContainsKey(neighbor))
					{
						parent.Add(neighbor, current);
					}
					else
					{
						parent[neighbor] = current;
					}

					//Now we add neighbor to unvisited list if it wasn't there
					// already
					if (!unvisited.Contains(neighbor))
					{
						unvisited.Enqueue(neighbor, neighbor.fCost);
					}
				}
			}
		}
	}

	public void GeneratePath()
	{
		map.currentPath = new List<Node>();
		Node n = target;
		map.currentPath.Add(n);
		while(n != source)
		{
			map.currentPath.Add(parent[n]);
			n = parent[n]; 
		}

		//Debug
		foreach(Node node in map.currentPath)
		{
			Debug.Log("Node: " + node.toStringVector());
		}
	}


	/// <summary>
	/// This was made for the sole purpose of aiding us in inheriting the same algorithm multiple times
	/// with different heuristic calculations. If not for that, the method would probably not exist
	/// </summary>
	/// <param name="n">Node for which you want the cost returned</param>
	/// <returns></returns>
	/// In A* we can use Euclidian Distance to get gCost and move cost from start to current tile for hCost
	/// This is a simple heuristc that should work decently
	public float GetCost(Node n)
	{
		return n.fCost;
	}
	public void setHCost(Node n)
	{
		n.hCost = Mathf.Abs( Vector2.Distance(new Vector2(n.x, n.y), targetDist) );
	}
}