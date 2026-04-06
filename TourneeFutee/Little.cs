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

            return Solve( //Algorithme récursif pour trouver le meilleur chemin
                matrix,
                new List<string>(cityNames),
                new List<string>(cityNames),
                new List<(string, string)>(),
                0,
                order
            );
        }

        // --- Méthodes utilitaires réalisant des étapes de l'algorithme de Little


        // Réduit la matrice `m` et revoie la valeur totale de la réduction
        // Après appel à cette méthode, la matrice `m` est *modifiée*.
        public static float ReduceMatrix(Matrix m)
        {
            float sum = 0.0f;

            for (int i = 0; i < m.NbRows; i++)
            {
                float minLine = float.MaxValue;
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) < minLine)
                    {
                        minLine = m.GetValue(i, j);
                    }
                }
                for (int j = 0; j < m.NbColumns; j++)
                {
                    m.SetValue(i, j, m.GetValue(i, j) - minLine);
                }
                sum += minLine;
            }

            for (int i = 0; i < m.NbColumns; i++)
            {
                float minColumn = float.MaxValue;
                for (int j = 0; j < m.NbRows; j++)
                {
                    if (m.GetValue(j, i) < minColumn)
                    {
                        minColumn = m.GetValue(j, i);
                    }
                }
                for (int j = 0; j < m.NbRows; j++)
                {
                    m.SetValue(j, i, m.GetValue(j, i) - minColumn);
                }
                sum += minColumn;
            }

            return sum;
        }

        // Renvoie le regret de valeur maximale dans la matrice de coûts `m` sous la forme d'un tuple `(int i, int j, float value)`
        // où `i`, `j`, et `value` contiennent respectivement la ligne, la colonne et la valeur du regret maximale
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            float maxRegret = float.MinValue;
            int indice_i = 0, indice_j = 0;
            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) == 0.0f)
                    {
                        float minLine = float.MaxValue;
                        float minColumn = float.MaxValue;
                        for (int k = 0; k < m.NbColumns; k++)
                        {
                            if (k != j && m.GetValue(i, k) < minLine)
                            {
                                minLine = m.GetValue(i, k);
                            }
                        }

                        for (int k = 0; k < m.NbRows; k++)
                        {
                            if (k != i && m.GetValue(k, j) < minColumn)
                            {
                                minColumn = m.GetValue(k, j);
                            }
                        }
                        float regret = minLine + minColumn;
                        if (regret > maxRegret)
                        {
                            maxRegret = regret;
                            indice_i = i;
                            indice_j = j;
                        }
                    }
                }
            }
            return (indice_i, indice_j, maxRegret);
        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {
            int count = 1;
            string current = segment.destination;

            while (current != segment.source)
            {
                var next = includedSegments.Find(s => s.source == current);
                if (next == default)
                    return false;
                current = next.destination;
                count++;
            }

            return count < nbCities;
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

        private Tour Solve(Matrix m, List<string> rowCities, List<string> colCities,
            List<(string source, string destination)> includedSegments, float currentBound, int nbCities)
        {
            currentBound += ReduceMatrix(m);

            if (currentBound >= bestCost)
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);

            // Cas de base : matrice 2x2
            if (m.NbRows == 2)
            {
                List<(string, string)> segs = new List<(string, string)>(includedSegments);
                float cost1 = m.GetValue(0, 0) + m.GetValue(1, 1);
                float cost2 = m.GetValue(0, 1) + m.GetValue(1, 0);

                if (cost1 <= cost2)
                {
                    segs.Add((rowCities[0], colCities[0]));
                    segs.Add((rowCities[1], colCities[1]));
                }
                else
                {
                    segs.Add((rowCities[0], colCities[1]));
                    segs.Add((rowCities[1], colCities[0]));
                }

                float totalCost = currentBound + Math.Min(cost1, cost2);
                if (totalCost < bestCost) bestCost = totalCost;
                return new Tour(segs, totalCost);
            }

            // Regret maximal
            (int ri, int rj, float _) = GetMaxRegret(m);
            string source = rowCities[ri];
            string dest = colCities[rj];

            Matrix incM = CopyMatrix(m);
            List<string> incRows = new List<string>(rowCities);
            List<string> incCols = new List<string>(colCities);
            List<(string source, string destination)> incSegs = new List<(string source, string destination)>(includedSegments);
            incSegs.Add((source, dest));

            incM.RemoveRow(ri);
            incM.RemoveColumn(rj);
            incRows.RemoveAt(ri);
            incCols.RemoveAt(rj);

            // Eviter les trajets parasites
            for (int i = 0; i < incM.NbRows; i++)
                for (int j = 0; j < incM.NbColumns; j++)
                    if (IsForbiddenSegment((incRows[i], incCols[j]), incSegs, nbCities))
                        incM.SetValue(i, j, float.PositiveInfinity);

            Tour includeTour = Solve(incM, incRows, incCols, incSegs, currentBound, nbCities);

            Matrix excM = CopyMatrix(m);
            excM.SetValue(ri, rj, float.PositiveInfinity);

            Tour excludeTour = Solve(excM, new List<string>(rowCities), new List<string>(colCities),
                new List<(string source, string destination)>(includedSegments), currentBound, nbCities);

            return includeTour.Cost <= excludeTour.Cost ? includeTour : excludeTour;
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
