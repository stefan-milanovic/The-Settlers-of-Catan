using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class GraphNode
{
    public bool Visited { get; set; }
    public WorldPath Road { get; set; }
    public List<WorldPath> RoadNeighbours { get; set; }
    
    public List<GraphNode> Neighbours { get; set; }
}
