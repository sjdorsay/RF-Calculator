using System;
using System.Collections.Generic;
using System.Linq;

namespace rf_tools
{
    // Used for storing positional and rotational information
    public class LTSpiceCoords
    {
        public int X = 0;
        public int Y = 0;
        public int ROT = 0;

        public LTSpiceCoords(int x, int y, int rotation)
        {
            X = x;
            Y = y;
            ROT = rotation;
        }

        public override string ToString() => $"({X}, {Y}) @ {ROT}";
    }

    // Used for storing positional information
    public class LTSpicePos
    {
        public int X = 0;
        public int Y = 0;

        public LTSpicePos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X}, {Y})";
    }

    // LTSpice component model
    internal class LTSpiceModel
    {
        // Positional data
        public LTSpiceCoords pos;

        public int sizeX, sizeY;
        public int margin;
        public int nPins;

        // Node data
        public int[] pinNodes;
        public LTSpicePos[] pinOffsets;

        // Identity data
        readonly string type;
        readonly string name;
        readonly string value = "";
        readonly string value2 = "";

        public LTSpiceModel(string name, string type, int nPins, LTSpiceCoords pos, string value)
        {
            this.name = name;
            this.type = type;

            if (1 > nPins)
                throw new ArgumentException("Parameter cannot be less than 1.", nameof(nPins));

            this.nPins = nPins;
            this.pos = pos;

            this.value = value;
            
            pinNodes = new int[nPins];
            pinOffsets = new LTSpicePos[nPins];
        }

        public void Move(int dX, int dY)
        {
            pos.X += dX;
            pos.Y += dY;
        }

        public LTSpicePos Get_Pin_Pos(int n)
        {
            if (nPins - 1 < n)
                throw new ArgumentException("Pin # cannot be greater than the number of pins.", nameof(nPins));

            // Initialze the position variable
            LTSpicePos pinPos = new LTSpicePos(pos.X, pos.Y);

            // Convert degrees to radians
            double angle = (Math.PI * pos.ROT) / 180;
            
            // Rotate the pin locations based on the component location
            pinPos.X += (int)(pinOffsets[n].X * Math.Cos(angle));
            pinPos.X -= (int)(pinOffsets[n].Y * Math.Sin(angle));

            // Rotate the pin locations based on the component location
            pinPos.Y += (int)(pinOffsets[n].X * Math.Sin(angle));
            pinPos.Y += (int)(pinOffsets[n].Y * Math.Cos(angle));

            return pinPos;
        }

        public override string ToString()
        {
            string line1 = string.Format("SYMBOL {0} {1} {2} R{3}\n", type, pos.X, pos.Y, pos.ROT);
            string line2 = string.Format("SYMATTR InstName {0}\n", name);
            string line3 = string.Format("SYMATTR Value {0}\n", value);
            string line4 = string.Format("SYMATTR Value2 {0}\n", value2);

            return line1 + line2 + line3 + line4;
        }
    }

    // LTSpice node class
    // The node contains all components attached to a node.
    // Each component will be connected to a maximum of the pin count of the components
    internal class LTSpiceNode
    {
        // Hold all the connected components
        readonly List<LTSpiceModel> components = new List<LTSpiceModel>();
        public int id;

        public LTSpiceNode(int id)
        {
            this.id = id;
        }

        public void AddComponent(LTSpiceModel newComponent)
        {
            components.Add(newComponent);
        }

        public List<LTSpiceModel> GetComponents()
        {
            return components;
        }

        public string Connect()
        {
            string text = "";

            int startX, startY;
            int stopX, stopY;

            // Sort connected components based on x position
            components.Sort((x, y) => x.pos.X.CompareTo(y.pos.X));
            LTSpiceModel startComp = this.components[0];

            // Initialize starting position
            startX = 0;
            startY = 0;
            
            // Initialize stop position
            stopX = 0;
            stopY = 0;

            // Loop over the number of pins in the first component
            for (int i = 0; i < startComp.nPins; i++)
            {

                if (this.id == startComp.pinNodes[i])
                {
                    startX = startComp.Get_Pin_Pos(i).X;
                    startY = startComp.Get_Pin_Pos(i).Y;
                }
            }

            // Loop over all components connected to node
            foreach (LTSpiceModel comp in components)
            {
                // For each component, loop over the available pins
                for (int i = 0; i < comp.nPins; i++)
                {
                    // Check if the node of the current pin matches the id of the node
                    if (this.id == comp.pinNodes[i])
                    {
                        stopX = comp.Get_Pin_Pos(i).X;
                        stopY = comp.Get_Pin_Pos(i).Y;

                        // Check if the node of the current pin is ground
                        if (0 == this.id)
                        {
                            text += string.Format("FLAG {0} {1} 0\n", stopX, stopY);
                        }

                        // If the current pin node is NOT ground, connect wires
                        else
                        {
                            // Vertical upwards first
                            if (stopY < startY)
                            {
                                text += string.Format("WIRE {0} {1} {2} {3}\n", startX, startY, startX, stopY);

                                startY = stopY;
                            }

                            // Horizontal rightwards second
                            if (stopX > startX)
                            {
                                text += string.Format("WIRE {0} {1} {2} {3}\n", startX, startY, stopX, startY);

                                startX = stopX;
                            }

                            // Vertical downwards third
                            if (stopY > startY)
                            {
                                text += string.Format("WIRE {0} {1} {2} {3}\n", startX, startY, startX, stopY);

                                startY = stopY;
                            }

                            // Horizontal leftwards last
                            if (stopX < startX)
                            {
                                text += string.Format("WIRE {0} {1} {2} {3}\n", startX, startY, stopX, startY);

                                startX = stopX;
                            }
                        }
                    }
                }

                startX = stopX;
                startY = stopY;
            }

            return text;
        }
    }

    public class LTSpiceAdapter
    {
        readonly List<LTSpiceNode> nodes = new List<LTSpiceNode>(); // N Pin Model
        readonly List<LTSpiceModel> components = new List<LTSpiceModel>(); // N Pin Model
        readonly List<string> parameters = new List<string>();
        readonly List<string> simulations = new List<string>();

        private int compCount = 0;

        // Add LTSpice component model to assigned nodes
        private void AddComponent(LTSpiceModel component)
        {
            // Create temporary node object
            LTSpiceNode tNode;

            
            for (int i = 0; i < component.nPins; i++)
            {
                // Check whether current node id exists
                if (!NodeExists(component.pinNodes[i]))
                {
                    // Add new node object with current id
                    AddNewNode(component.pinNodes[i]);
                }

                // Add component to appropriate nodes
                tNode = nodes.Find(p => p.id == component.pinNodes[i]);
                tNode.AddComponent(component);
            }

            components.Add(component);
            compCount += 1;
        }

        public void AddParameter(int x, int y, string param, float value)
        {
            string parameter = string.Format("TEXT {0} {1} Left 2 !.param {2}={3}\n", x, y, param, value);

            parameters.Add(parameter);
        }

        // Add AC source using N pin model
        // Parameters:
        // - n:   Integer array of nodes
        // - pos: LTSpiceCoords position and rotation
        // - value: string representation of component value
        // - TO DO: MIISSING PIN OFFSETS
        public void AddSource(int[] n, LTSpiceCoords pos)
        {
            string value = "1\nSYMATTR Value2 AC 1\nSYMATTR SpiceLine Rser=50";

            string refDes = string.Format("V{0}", compCount);
            LTSpiceModel source = new LTSpiceModel(refDes, "voltage", 2, pos, value)
            {
                // Symbol width is 64 but the pins are at the center of the position, not offset
                sizeX = 0,
                sizeY = 80,

                margin = 16,
            };
            
            // The offset values are calculated based on the pin location in reference to the component origin
            source.pinOffsets[0] = new LTSpicePos(0, 16);
            source.pinOffsets[1] = new LTSpicePos(0, 96);

            // Assign nodes to pins
            source.pinNodes[0] = n[0];
            source.pinNodes[0] = n[1];
            source.pinNodes = n;

            AddComponent(source);
        }

        // Add resistor using N pin model
        // - n:     Integer array of nodes
        // - pos:   LTSpiceCoords position and rotation
        // - value: String representation of component value
        public void AddResistor(int[] n, LTSpiceCoords pos, string value)
        {
            // Set reference designator based on number of components
            string refDes = string.Format("R{0}", compCount);

            // Create LTSpice model
            LTSpiceModel res = new LTSpiceModel(refDes, "res", 2, pos, value)
            {
                sizeX = 32,
                sizeY = 80,

                margin = 16,
            };

            // The offset values are calculated based on the pin location in reference to the component origin
            res.pinOffsets[0] = new LTSpicePos(16, 16);
            res.pinOffsets[1] = new LTSpicePos(16, 96);

            // Assign nodes to pins
            res.pinNodes[0] = n[0];
            res.pinNodes[1] = n[1];

            // Add new resistor model to list of components
            AddComponent(res);
        }

        // Add capacitor using N Pin Model
        // - n:     Integer array of nodes
        // - pos:   LTSpiceCoords position and rotation
        // - value: String representation of component value
        // - TO DO: MISSING OFFSETS
        public void AddCapacitor(int[] n, LTSpiceCoords pos, string value)
        {
            // Set reference designator based on number of components
            string refDes = string.Format("C{0}", compCount);

            // Create LTSpice model
            LTSpiceModel cap = new LTSpiceModel(refDes, "cap", 2, pos, value)
            {
                sizeX = 32,
                sizeY = 64,

                margin = 0,
            };

            // The offset values are calculated based on the pin location in reference to the component origin
            cap.pinOffsets[0] = new LTSpicePos(16, 16);
            cap.pinOffsets[1] = new LTSpicePos(16, 96);

            // Assign nodes to pins
            cap.pinNodes = n;

            // Add new capacitor model to list of components
            AddComponent(cap);
        }

        // Add inductor using N Pin Model
        // - n:     Integer array of nodes
        // - pos:   LTSpiceCoords position and rotation
        // - value: String representation of component value
        // - TO DO: MISSING OFFSETS
        public void AddInductor(int[] n, LTSpiceCoords pos, string value)
        {
            // Set reference designator based on number of components
            string refDes = string.Format("L{0}", compCount);

            // Create LTSpice model
            LTSpiceModel ind = new LTSpiceModel(refDes, "ind", 2, pos, value)
            {
                sizeX = 32,
                sizeY = 80,

                margin = 16,
            };

            // The offset values are calculated based on the pin location in reference to the component origin
            ind.pinOffsets[0] = new LTSpicePos(16, 16);
            ind.pinOffsets[1] = new LTSpicePos(16, 96);

            // Assign nodes to pins
            ind.pinNodes = n;

            // Add new inductor model to list of components
            AddComponent(ind);
        }

        // Add transmission line using N Pin Model
        // - n:     Integer array of nodes
        // - pos:   LTSpiceCoords position and rotation
        // - value: String representation of component value
        // - TO DO: MISSING OFFSETS
        public void AddTransmissionLine(int[] n, LTSpiceCoords pos, string value)
        {
            // Set reference designator based on number of components
            string refDes = string.Format("TL{0}", compCount);

            // Create LTSpice model
            LTSpiceModel tline = new LTSpiceModel(refDes, "tline", 4, pos, value)
            {
                sizeX = 32,
                sizeY = 80,

                margin = 16,
            };

            // The offset values are calculated based on the pin location in reference to the component origin
            tline.pinOffsets[0] = new LTSpicePos(16, 16);
            tline.pinOffsets[1] = new LTSpicePos(16, 96);
            tline.pinOffsets[2] = new LTSpicePos(16, 96);
            tline.pinOffsets[3] = new LTSpicePos(16, 96);

            // Assign nodes to pins
            tline.pinNodes[0] = n[0];
            tline.pinNodes[1] = n[1];
            tline.pinNodes[2] = n[2];
            tline.pinNodes[3] = n[3];

            // Add new tline model to list of components
            AddComponent(tline);
        }

        public void AddACSim(int x, int y, float start, float stop, int Npts)
        {
            string sim;

            sim = string.Format("TEXT {0} {1} Left 2 !.ac dec {2} {3} {4}\n", x, y, Npts, start, stop);

            simulations.Add(sim);
        }

        public void AddNetSim(int x, int y, float start, float stop, int Npts)
        {
            string sim;

            sim = string.Format("TEXT {0} {1} Left 2 !.net I(R{2}) V0\n", x, y + 16, compCount - 1);
            sim += string.Format("TEXT {0} {1} Left 2 !.step param run 1 100 1\n", x, y + 32);

            AddACSim(x, y, start, stop, Npts);
            simulations.Add(sim);
        }

        // Check if node exists
        private bool NodeExists(int node)
        {
            // Find any nodes with id = node
            bool exists = nodes.Any(p => p.id == node);

            return exists;
        }

        // Add new LTSpice node
        private void AddNewNode(int node)
        {
            // Add new LTSpice node with id = node to node list
            nodes.Add(new LTSpiceNode(node));
        }

        public void PrintNode(int node)
        {
            string text = "\n";

            text += nodes[node].Connect();

            foreach (LTSpiceModel comp in nodes[node].GetComponents())
            {
                text += comp.ToString();
            }

            Console.Write(text);
        }

        public override string ToString()
        {
            string text = "Version 4\n";
            text += "SHEET 1 312 120\n";

            // Add connections
            foreach (LTSpiceNode node in nodes)
            {
                text += node.Connect();
            }
            
            // Add components
            foreach (LTSpiceModel comp in components)
            {
                text += comp.ToString();
            }

            // Add simulation
            foreach (string sim in simulations)
            {
                text += sim;
            }

            // Add parameters
            foreach (string param in parameters)
            {
                text += param;
            }

            return text;
        }
    }
}
