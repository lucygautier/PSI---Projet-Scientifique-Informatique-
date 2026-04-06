using System.Net.Sockets;

namespace TourneeFutee
{
    // Résout le problème de voyageur de commerce défini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        Graph graph;
        float bestCost;

        // Instancie le planificateur en spécifiant le graphe modélisant un problème de voyageur de commerce
        public Little(Graph graph)
        {
            this.graph = graph;
        }

        // Trouve la tournée optimale dans le graphe `this.graph`
        // (c'est à dire le cycle hamiltonien de plus faible coût)
        public Tour ComputeOptimalTour()
        {
            int order = graph.Order; //Récupération du nombre de sommets du graphe
            List<string> cityNames = new List<string>(); //Création d' une liste pour stocker les noms des villes

            for (int i = 0; i < order; i++) //Remplissage de la liste avec les noms des sommets
            { 
                cityNames.Add(graph.GetVertexName(i)); 
            }
            Matrix matrix = CopyMatrix(graph.GetAdjacencyMatrix()); //Copie de la matrice des distances du graphe pour ne pas modifier le graphe d'origine

            for (int i = 0; i < order; i++) //Remplissage de la diagonale à +∞ dans la copie de la matrice
            { 
                matrix.SetValue(i, i, float.PositiveInfinity); 
            }
            bestCost = float.PositiveInfinity; //Initialisation du meilleur coût 
            return Solve(matrix,new List<string>(cityNames),new List<string>(cityNames),new List<(string, string)>(),0,order);
        }

        // --- Méthodes utilitaires réalisant des étapes de l'algorithme de Little


        // Réduit la matrice `m` et revoie la valeur totale de la réduction
        // Après appel à cette méthode, la matrice `m` est *modifiée*.
        public static float ReduceMatrix(Matrix m)
        {
            float sum = 0.0f;

            for (int i = 0; i < m.NbRows; i++)
            {
                float minLine = float.PositiveInfinity;

                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) < minLine)
                    {
                        minLine = m.GetValue(i, j);
                    }
                }

                if (!float.IsPositiveInfinity(minLine) && minLine > 0)
                {
                    for (int j = 0; j < m.NbColumns; j++)
                    {
                        if (!float.IsPositiveInfinity(m.GetValue(i, j)))
                        {
                            m.SetValue(i, j, m.GetValue(i, j) - minLine);
                        }
                    }

                    sum += minLine;
                }
            }

            for (int i = 0; i < m.NbColumns; i++)
            {
                float minColumn = float.PositiveInfinity;

                for (int j = 0; j < m.NbRows; j++)
                {
                    if (m.GetValue(j, i) < minColumn)
                    {
                        minColumn = m.GetValue(j, i);
                    }
                }

                if (!float.IsPositiveInfinity(minColumn) && minColumn > 0)
                {
                    for (int j = 0; j < m.NbRows; j++)
                    {
                        if (!float.IsPositiveInfinity(m.GetValue(j, i)))
                        {
                            m.SetValue(j, i, m.GetValue(j, i) - minColumn);
                        }
                    }
                    sum += minColumn;
                }
            }
            return sum;
        }

        // Renvoie le regret de valeur maximale dans la matrice de coûts `m` sous la forme d'un tuple `(int i, int j, float value)`
        // où `i`, `j`, et `value` contiennent respectivement la ligne, la colonne et la valeur du regret maximale
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            int n = m.NbRows;
            float maxRegret = float.MinValue;
            int bestI = -1, bestJ = -1;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (m.GetValue(i, j) == 0)
                    {
                        float minLine = float.PositiveInfinity;
                        float minColumn = float.PositiveInfinity;
                       
                        for (int k = 0; k < n; k++) //Minimum de ligne (hors j)
                        {
                            if (k != j && !float.IsPositiveInfinity(m.GetValue(i, k)))
                            {
                                if (m.GetValue(i, k) < minLine)
                                {
                                    minLine = m.GetValue(i, k);
                                }
                            }
                        }
                
                        for (int k = 0; k < n; k++) //Minomum de colonne (hors i)
                        {
                            if (k != i && !float.IsPositiveInfinity(m.GetValue(k, j)))
                            {
                                if (m.GetValue(k, j) < minColumn)
                                {
                                    minColumn = m.GetValue(k, j);
                                }
                            }
                        }

                        if (float.IsPositiveInfinity(minLine)) minLine = 0;
                        if (float.IsPositiveInfinity(minColumn)) minColumn = 0;
                        float regret = minLine + minColumn;                      
                        if (regret > maxRegret)
                        {
                            maxRegret = regret;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }
            }
            return (bestI, bestJ, maxRegret);
        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (var s in includedSegments)
            {
                map[s.source] = s.destination;
            }
            int count = 1;
            string current = segment.destination;

            while (current != segment.source)
            {
                if (!map.ContainsKey(current))
                { 
                    return false; 
                }
                current = map[current];
                count++;
            }
            return count < nbCities;
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

        private Tour Solve(Matrix m,List<string> rowCities,List<string> colCities,List<(string source, string destination)> includedSegments,float currentBound,int nbCities)
        {
            currentBound += ReduceMatrix(m);

            if (currentBound >= bestCost)
            {
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);
            }

            if (m.NbRows == 1)
            {
                List<(string source, string destination)> finalSegments =new List<(string source, string destination)>(includedSegments);
                finalSegments.Add((rowCities[0], colCities[0]));
                return this.BuildTourIfValid(finalSegments, nbCities);
            }

            if (m.NbRows == 2)
            {
                List<(string source, string destination)> candidate1 = new List<(string source, string destination)>(includedSegments);
                candidate1.Add((rowCities[0], colCities[0]));
                candidate1.Add((rowCities[1], colCities[1]));
                Tour tour1 = this.BuildTourIfValid(candidate1, nbCities);
                List<(string source, string destination)> candidate2 = new List<(string source, string destination)>(includedSegments);
                candidate2.Add((rowCities[0], colCities[1]));
                candidate2.Add((rowCities[1], colCities[0]));
                Tour tour2 = this.BuildTourIfValid(candidate2, nbCities);
                return tour1.Cost <= tour2.Cost ? tour1 : tour2;
            }

            (int ri, int rj, float regret) = GetMaxRegret(m);

            if (ri < 0 || rj < 0 || float.IsNegativeInfinity(regret))
            {
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);
            }
            string source = rowCities[ri];
            string destination = colCities[rj];
            Matrix includeMatrix = CopyMatrix(m);
            List<string> includeRows = new List<string>(rowCities);
            List<string> includeCols = new List<string>(colCities);
            List<(string source, string destination)> includeSegments = new List<(string source, string destination)>(includedSegments);
            includeSegments.Add((source, destination));
            includeMatrix.RemoveRow(ri);
            includeMatrix.RemoveColumn(rj);
            includeRows.RemoveAt(ri);
            includeCols.RemoveAt(rj);

            for (int i = 0; i < includeMatrix.NbRows; i++)
            {
                for (int j = 0; j < includeMatrix.NbColumns; j++)
                {
                    if (IsForbiddenSegment((includeRows[i], includeCols[j]), includeSegments, nbCities))
                    {
                        includeMatrix.SetValue(i, j, float.PositiveInfinity);
                    }
                }
            }

            Tour includeTour = Solve(includeMatrix,includeRows,includeCols,includeSegments,currentBound, nbCities);
            Matrix excludeMatrix = CopyMatrix(m);
            excludeMatrix.SetValue(ri, rj, float.PositiveInfinity);
            Tour excludeTour = Solve(excludeMatrix, new List<string>(rowCities),new List<string>(colCities),new List<(string source, string destination)>(includedSegments),currentBound,nbCities);
            return includeTour.Cost <= excludeTour.Cost ? includeTour : excludeTour;
        }

        private Tour BuildTourIfValid(List<(string source, string destination)> segments, int nbCities)
        {
            if (segments.Count != nbCities)
            {
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);
            }

            HashSet<string> sources = new HashSet<string>();
            HashSet<string> destinations = new HashSet<string>();

            foreach ((string source, string destination) segment in segments)
            {
                if (sources.Contains(segment.source) || destinations.Contains(segment.destination))
                {
                    return new Tour(new List<(string, string)>(), float.PositiveInfinity);
                }
                sources.Add(segment.source);
                destinations.Add(segment.destination);
            }

            if (!this.IsHamiltonianCycle(segments, nbCities))
            {
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);
            }

            float totalCost = this.ComputeTourCost(segments);

            if (float.IsPositiveInfinity(totalCost))
            {
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);
            }

            if (totalCost < bestCost)
            {
                bestCost = totalCost;
            }
            return new Tour(segments, totalCost);
        }

        private bool IsHamiltonianCycle(List<(string source, string destination)> segments, int nbCities)
        {
            Dictionary<string, string> successors = new Dictionary<string, string>();

            foreach ((string source, string destination) segment in segments)
            {
                successors[segment.source] = segment.destination;
            }

            string start = segments[0].source;
            string current = start;
            int count = 0;

            do
            {
                if (!successors.ContainsKey(current))
                {
                    return false;
                }

                current = successors[current];
                count++;
            }
            while (current != start && count <= nbCities);
            return current == start && count == nbCities;
        }

        private float ComputeTourCost(List<(string source, string destination)> segments)
        {
            float totalCost = 0.0f;

            foreach ((string source, string destination) segment in segments)
            {
                try
                {
                    totalCost += this.graph.GetEdgeWeight(segment.source, segment.destination);
                }
                catch (ArgumentException)
                {
                    return float.PositiveInfinity;
                }
            }
            return totalCost;
        }
        private static Matrix CopyMatrix(Matrix matrix)
        {
            Matrix copy = new Matrix(matrix.NbRows, matrix.NbColumns);
            for (int i = 0; i < matrix.NbRows; i++)
            {
                for (int j = 0; j < matrix.NbColumns; j++)
                { 
                    copy.SetValue(i, j, matrix.GetValue(i, j)); 
                }
            }
            return copy;
        }
    }
}
