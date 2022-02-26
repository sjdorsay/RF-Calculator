using System;
using System.Collections.Generic;
using System.Linq;

namespace rf_tools
{
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

    internal class LTSpiceModel
    {
        // Positional data
        public int posX, posY;
        public int rotation;

        public int sizeX, sizeY;
        public int margin;

        // Node data
        public int pin1Node;
        public int pin2Node;

        // Identity data
        readonly string type;
        readonly string name;
        readonly string value = "";
        readonly string value2 = "";

        public LTSpiceModel(string name, string type, int posX, int posY, int rotation, string value)
        {
            this.name = name;
            this.type = type;

            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;

            this.value = value;
        }

        public void Move(int dX, int dY)
        {
            posX += dX;
            posY += dY;
        }

        public int Get_Pin1_X()
        {
            int pin1x;
            double angle = Math.PI * rotation / 180;

            pin1x = posX;
            pin1x += (int)(0.5 * sizeX * Math.Cos(angle));
            pin1x -= (int)(margin * Math.Sin(angle));

            return pin1x;
        }

        public int Get_Pin1_Y()
        {
            int pin1y;
            double angle = Math.PI * rotation / 180;

            pin1y = posY;
            pin1y += (int)(0.5 * sizeX * Math.Sin(angle));
            pin1y += (int)(margin * Math.Cos(angle));

            return pin1y;
        }

        public int Get_Pin2_X()
        {
            int pin2x;
            double angle = Math.PI * rotation / 180;

            pin2x = posX;
            pin2x += (int)(0.5 * sizeX * Math.Cos(angle));
            pin2x -= (int)((margin + sizeY) * Math.Sin(angle));

            return pin2x;
        }

        public int Get_Pin2_Y()
        {
            int pin2y;
            double angle = Math.PI * rotation / 180;

            pin2y = posY;
            pin2y += (int)(0.5 * sizeX * Math.Sin(angle));
            pin2y += (int)((margin + sizeY) * Math.Cos(angle));

            return pin2y;
        }

        public override string ToString()
        {
            string line1 = string.Format("SYMBOL {0} {1} {2} R{3}\n", type, posX, posY, rotation);
            string line2 = string.Format("SYMATTR InstName {0}\n", name);
            string line3 = string.Format("SYMATTR Value {0}\n", value);
            string line4 = string.Format("SYMATTR Value2 {0}\n", value2);

            return line1 + line2 + line3 + line4;
        }
    }

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

            // Sort based on x position
            components.Sort((x, y) => x.posX.CompareTo(y.posX));
            LTSpiceModel startComp = components[0];

            startX = 0;
            startY = 0;

            if (startComp.pin1Node == id)
            {
                // Pin 1 is at the top centre
                startX = startComp.Get_Pin1_X();
                startY = startComp.Get_Pin1_Y();
            }

            if (startComp.pin2Node == id)
            {
                // Pin 1 is at the top centre
                startX = startComp.Get_Pin2_X();
                startY = startComp.Get_Pin2_Y();
            }

            foreach (LTSpiceModel comp in components)
            {
                stopX = 0;
                stopY = 0;

                if (comp.pin1Node == id)
                {
                    // Pin 1 is at the top centre
                    stopX = comp.Get_Pin1_X();
                    stopY = comp.Get_Pin1_Y();
                }

                if (comp.pin2Node == id)
                {
                    // Pin 1 is at the top centre
                    stopX = comp.Get_Pin2_X();
                    stopY = comp.Get_Pin2_Y();
                }

                if (0 == id)
                {
                    text += string.Format("FLAG {0} {1} 0\n", stopX, stopY);
                }
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

                startX = stopX;
                startY = stopY;
            }

            return text;
        }
    }

    public class LTSpiceAdapter
    {
        readonly List<LTSpiceNode> nodes = new List<LTSpiceNode>();
        readonly List<LTSpiceModel> components = new List<LTSpiceModel>();
        readonly List<string> parameters = new List<string>();
        readonly List<string> simulations = new List<string>();

        private int compCount = 0;

        private void AddComponent(int n0, int n1, LTSpiceModel component)
        {
            LTSpiceNode tNode;
            compCount += 1;

            // do the nodes exist?
            if (!NodeExists(n0))
            {
                AddNewNode(n0);
            }

            if (!NodeExists(n1))
            {
                AddNewNode(n1);
            }

            component.pin1Node = n0;
            component.pin2Node = n1;

            // Add component to appropriate nodes
            tNode = nodes.Find(p => p.id == n0);
            tNode.AddComponent(component);

            tNode = nodes.Find(p => p.id == n1);
            tNode.AddComponent(component);

            components.Add(component);
        }

        public void AddParameter(int x, int y, string param, float value)
        {
            string parameter = string.Format("TEXT {0} {1} Left 2 !.param {2}={3}\n", x, y, param, value);

            parameters.Add(parameter);
        }

        public void AddSource(int n0, int n1, LTSpiceCoords pos)
        {
            string value = "1\nSYMATTR Value2 AC 1\nSYMATTR SpiceLine Rser=50";

            string refDes = string.Format("V{0}", compCount);
            LTSpiceModel source = new LTSpiceModel(refDes, "voltage", pos.X, pos.Y, pos.ROT, value)
            {
                // Symbol width is 64 but the pins are at the center of the position, not offset
                sizeX = 0,
                sizeY = 80,

                margin = 16,
            };

            AddComponent(n0, n1, source);
        }

        public void AddResistor(int n0, int n1, LTSpiceCoords pos, string value)
        {
            string refDes = string.Format("R{0}", compCount);
            LTSpiceModel res = new LTSpiceModel(refDes, "res", pos.X, pos.Y, pos.ROT, value)
            {
                sizeX = 32,
                sizeY = 80,

                margin = 16,
            };

            AddComponent(n0, n1, res);
        }

        public void AddCapacitor(int n0, int n1, LTSpiceCoords pos, string value)
        {
            string refDes = string.Format("C{0}", compCount);
            LTSpiceModel cap = new LTSpiceModel(refDes, "cap", pos.X, pos.Y, pos.ROT, value)
            {
                sizeX = 32,
                sizeY = 64,

                margin = 0,
            };

            AddComponent(n0, n1, cap);
        }

        public void AddInductor(int n0, int n1, LTSpiceCoords pos, string value)
        {
            string refDes = string.Format("L{0}", compCount);
            LTSpiceModel ind = new LTSpiceModel(refDes, "ind", pos.X, pos.Y, pos.ROT, value)
            {
                sizeX = 32,
                sizeY = 80,

                margin = 16,
            };

            AddComponent(n0, n1, ind);
        }

        public void AddTransmissionLine(int n0, int n1, LTSpiceCoords pos, string value)
        {
            string refDes = string.Format("TL{0}", compCount);
            LTSpiceModel ind = new LTSpiceModel(refDes, "tline", pos.X, pos.Y, pos.ROT, value)
            {
                sizeX = 32,
                sizeY = 80,

                margin = 16,
            };

            AddComponent(n0, n1, ind);
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

        private bool NodeExists(int node)
        {
            bool exists = nodes.Any(p => p.id == node);

            return exists;
        }

        private void AddNewNode(int node)
        {
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
