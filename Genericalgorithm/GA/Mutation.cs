using System;
using LogModule;

namespace Genericalgorithm
{
    public static class Mutation
    {
        // Function Uniform of class Mutation
        // change a bit of offspring chromosome for mutation
        public static void mutation(this Chromosome child, Random rand)
        {
            try
            {
                // Random Number for choose 2 bit between 0 ~ (offspring.Length - 1)
                // if(offspring.Length == 8)
                //                           (0)×-------------×(offspring.Length-1)
                //                             |_|_|_|_|_|_|_|_|
                //                              0 1 2 3 4 5 6 7
                //
                // change 2 bit locate (Greedy Mutation)
                // before Greedy Mutate:
                // chromosome Child =      |_|_|_|_|_|_|_|_| ...
                //                          0 1 2 3 4 5 6 7 
                // Greedy Mutating:
                // Select 2 bit (1 & 4)       *     *
                // chromosome Child =      |_|_|_|_|_|_|_|_| ...            (Step 1)
                //                          0 1 2 3 4 5 6 7 
                // After Greedy Mutation:    
                // Changed 2 bit (1 & 4)      *     *
                // chromosome Child =      |_|_|_|_|_|_|_|_| ...            (Step 2)
                //                          0 4 2 3 1 5 6 7 
                //
                // Step 1: -------------- Select 2 bit by Random Number -----------------------
                int bit0 = rand.Next(0, child.Tour.Length - 1);
                int bit1;
                do
                {
                    bit1 = rand.Next(0, child.Tour.Length - 1);
                }
                // if bit0 == bit1 then no mutate because selected bit change by self
                while (bit1 == bit0);
                // -------------------------------------------------------------------------------
                // Step 2: +++++++++++++++++++ Change selected bit's +++++++++++++++++++++++++++++
                //
                //         buffer <---- bit0
                int buffer = child.Tour[bit0];
                //
                //         bit0   <---- bit1
                child.Tour[bit0] = child.Tour[bit1];
                //
                //         bit1   <---- buffer
                child.Tour[bit1] = buffer;
                // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
