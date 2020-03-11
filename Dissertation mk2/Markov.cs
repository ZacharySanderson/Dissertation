using System;

namespace Dissertation_mk2
{
    public class Markov
    {
        readonly Random rand = new Random();
        public bool Aggressive;
        public bool Explorer;

        private readonly double remainAggressive;
        private readonly double remainExplorer;

        public Markov()
        {
            double p = rand.NextDouble();
            if (p < 0.5)
            {
                Aggressive = true;
                remainAggressive = 1 - p/2;
                remainExplorer = p / 2;
                Console.WriteLine("grrrr");
                Console.WriteLine("remain aggressive: " + remainAggressive);
                Console.WriteLine("remain Explorer: " + remainExplorer);
            }
            else
            { 
                Explorer = true;
                remainExplorer = p + (1 - p)/2;
                remainAggressive = 1 - remainExplorer;
                Console.WriteLine("not grrrr");
                Console.WriteLine("remain aggressive: " + remainAggressive);
                Console.WriteLine("remain Explorer: " + remainExplorer);
            }
        }

        public void Transition()
        {
            double transition = rand.NextDouble();
            if (Aggressive)
            {
                if (transition < remainAggressive)
                    return;
                Aggressive = false;
                Explorer = true;
                Console.WriteLine("not grrrr");
            }
            else
            {
                if (transition < remainExplorer)
                    return;
                Aggressive = true;
                Explorer = false;
                Console.WriteLine("grrrr");
            }
        }
    }
}
