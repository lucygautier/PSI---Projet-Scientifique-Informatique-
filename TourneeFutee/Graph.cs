using System;
using System.Xml.Linq;

namespace TourneeFutee
{
    public class Graph
    {

        bool directed;
        float noEdgeValue;
        Matrix adjacencyMatrix;
        Dictionary<string, int> vertexIndices; //Associe le nom du sommet à son indice
        Dictionary<string, float> vertexValues;
        Dictionary<int, string> indexToVertex; //Associe l'indice au nom (pour getNeighbors)



        // --- Construction du graphe ---

        // Contruit un graphe (`directed`=true => orienté)
        // La valeur `noEdgeValue` est le poids modélisant l'absence d'un arc (0 par défaut)
        public Graph(bool directed, float noEdgeValue = 0)
        {
            this.directed = directed;
            this.noEdgeValue = noEdgeValue;
            this.vertexIndices = new Dictionary<string, int>();
            this.vertexValues = new Dictionary<string, float>();
            this.indexToVertex = new Dictionary<int, string>();
            this.adjacencyMatrix = new Matrix(0, 0, noEdgeValue);
        }


        // --- Propriétés ---

        // Propriété : ordre du graphe
        // Lecture seule
        public int Order
        {
            get { return this.vertexIndices.Count; }  //L'odre du graphe (nombre de sommets) correspond au nombre d'éléments contenus dans le dictionnaire des indices des arcs
                                                      // pas de set
        }

        // Propriété : graphe orienté ou non
        // Lecture seule
        public bool Directed
        {
            get { return this.directed; }
            // pas de set
        }


        // --- Gestion des sommets ---

        // Ajoute le sommet de nom `name` et de valeur `value` (0 par défaut) dans le graphe
        // Lève une ArgumentException s'il existe déjà un sommet avec le même nom dans le graphe
        public void AddVertex(string name, float value = 0)
        {
            if (vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet {name} existe déjà.");
            }

            int index = vertexIndices.Count;

            adjacencyMatrix.AddRow(adjacencyMatrix.NbRows);
            adjacencyMatrix.AddColumn(adjacencyMatrix.NbColumns);

            vertexIndices.Add(name, index);
            vertexValues.Add(name, value);
            indexToVertex.Add(index, name);
            /*if (vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet {name} existe déjà."); //Lève une exception si le sommet existe déjà
            }
            vertexIndices.Add(name, vertexIndices.Count); //Ajoute le sommet de nom name dans le graphe et l'associe à un indice
            vertexValues.Add(name, value); //Ajoute le sommet de nom name et sa valeur dans le graphe
            indexToVertex.Add(vertexIndices.Count, name); //Ajoute la correspondance entre l'indice du sommet et son nom
            adjacencyMatrix.AddRow(adjacencyMatrix.NbRows); //Ajoute une nouvelle ligne dans la matrice d'adjacence (arcs sortants du sommet de nom name)
            adjacencyMatrix.AddColumn(adjacencyMatrix.NbColumns); //Ajoute une nouvelle colonne dans la matrice d'adjacence (arcs entrants du sommet de nom name)*/
        }


        // Supprime le sommet de nom `name` du graphe (et tous les arcs associés)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void RemoveVertex(string name)
        {
            if (!vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Impossible de retirer le sommet {name} car il n'existe pas.");
            }
            int index = vertexIndices[name]; //Récupère l'indice avant suppression
            vertexIndices.Remove(name); //Supprime le sommet du dictionnaire nom -> indice
            vertexValues.Remove(name);  //Supprime la valeur associée au sommet
            indexToVertex.Remove(index); //Supprime la correspondance indice -> nom
            adjacencyMatrix.RemoveRow(index); //Supprime la ligne du sommet dans la matrice
            adjacencyMatrix.RemoveColumn(index); //Supprime la colonne du sommet dans la matrice

            foreach (var key in vertexIndices.Keys.ToList())
            {
                if (vertexIndices[key] > index) //Uniquement si l'indice actuel est supérieur (après) l'indice du sommet supprimé
                {
                    int oldIndex = vertexIndices[key];
                    vertexIndices[key]--; //Décale l'indice du sommet

                    indexToVertex.Remove(oldIndex);
                    indexToVertex[oldIndex - 1] = key; //Met à jour l'autre dictionnaire aussi
                }
            }
        }

        // Renvoie la valeur du sommet de nom `name`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public float GetVertexValue(string name)
        {
            if (!vertexValues.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet {name} n'existe pas"); //Lève une excpetion si le sommet n'existe pas 
            }
            return vertexValues[name]; //Renvoie la valeur du sommet name
        }

        // Affecte la valeur du sommet de nom `name` à `value`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void SetVertexValue(string name, float value)
        {
            if (!vertexValues.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet {name} n'existe pas."); //Lève une exception si le sommet name n'existe pas 
            }
            vertexValues[name] = value;
        }


        // Renvoie la liste des noms des voisins du sommet de nom `vertexName`
        // (si ce sommet n'a pas de voisins, la liste sera vide)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public List<string> GetNeighbors(string vertexName)
        {
            List<string> neighborNames = new List<string>();

            if (!vertexIndices.ContainsKey(vertexName))
            {
                throw new ArgumentException($"Le sommet {vertexName} n'existe pas."); //Lève une exception si le sommet n'existe pas
            }
            int index = vertexIndices[vertexName]; //Indice du sommet de départ

            for (int j = 0; j < adjacencyMatrix.NbColumns; j++)
            {
                float edgeWeight = adjacencyMatrix.GetValue(index, j);

                if (edgeWeight != noEdgeValue)
                {
                    neighborNames.Add(indexToVertex[j]);
                }
            }
            return neighborNames;
        }

        // --- Gestion des arcs ---

        /* Ajoute un arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`, avec le poids `weight` (1 par défaut)
         * Si le graphe n'est pas orienté, ajoute aussi l'arc inverse, avec le même poids
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - il existe déjà un arc avec ces extrémités
         */
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            if (!vertexIndices.ContainsKey(sourceName) || !vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des deux sommets n'existe pas dans le graphe."); //Lève une exception si l'un des sommets n'est pas valide
            }
            int sourceIndex = vertexIndices[sourceName];
            int destinationIndex = vertexIndices[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) != noEdgeValue)
            {
                throw new ArgumentException($"L' arc reliant {sourceName} à {destinationName} existe déjà dans le graphe."); //Lève une excpetion si l'arc existe déjà
            }

            adjacencyMatrix.SetValue(sourceIndex, destinationIndex, weight); //Ajoute l'arc dans la matrice d'adjacence

            if (!directed)
            {
                adjacencyMatrix.SetValue(destinationIndex, sourceIndex, weight); //Ajoute l'arc reliant la destination à la soucre dans le cas d'un graohe non-orienté dans la matrice d'adjacence (l'arc apparaît deux fois)
            }
        }

        /* Supprime l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` du graphe
         * Si le graphe n'est pas orienté, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!vertexIndices.ContainsKey(sourceName))
            {
                throw new ArgumentException($"Le sommet sourceb'{sourceName}' n'existe pas dans le graphe.");
            }
            if (!vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException($"Le sommet destination '{destinationName}' n'existe pas dans le graphe.");
            }

            int sourceIndex = vertexIndices[sourceName];
            int destinationIndex = vertexIndices[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) == noEdgeValue)
            {
                throw new ArgumentException($"L'arc reliant {sourceName} à {destinationName} n'existe pas dans le graphe.");
            }
            adjacencyMatrix.SetValue(sourceIndex, destinationIndex, noEdgeValue);

            if (!directed)
            {
                adjacencyMatrix.SetValue(destinationIndex, sourceIndex, noEdgeValue);
            }
        }

        /* Renvoie le poids de l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`
         * Si le graphe n'est pas orienté, GetEdgeWeight(A, B) = GetEdgeWeight(B, A) 
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!vertexIndices.ContainsKey(sourceName))
            {
                throw new ArgumentException($"Le sommet source de nom '{sourceName}' n'existe pas dans le graphe.");
            }
            if (!vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException($"Le sommet destination de nom '{destinationName}' n'existe pas dans le graphe.");
            }

            int sourceIndex = vertexIndices[sourceName];
            int destinationIndex = vertexIndices[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) == noEdgeValue)
            {
                throw new ArgumentException($"L'arc allant de '{sourceName}' à '{destinationName}' n'existe pas dans le graphe.");
            }
            return adjacencyMatrix.GetValue(sourceIndex, destinationIndex);
        }

        /* Affecte le poids l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` à `weight` 
         * Si le graphe n'est pas orienté, affecte le même poids à l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            if (!vertexIndices.ContainsKey(sourceName))
            {
                throw new ArgumentException($"Le sommet source {sourceName} n'existe pas dans le graphe.");
            }
            if (!vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException($"Le sommet destination {destinationName} n'existe pas dans le graphe.");
            }
            int sourceIndex = vertexIndices[sourceName];
            int destinationIndex = vertexIndices[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) == noEdgeValue)
            {
                throw new ArgumentException($"L'arc reliant {sourceName} à {destinationName} n'existe pas dans le graphe.");
            }

            adjacencyMatrix.SetValue(sourceIndex, destinationIndex, weight);

            if (!directed)
            {
                adjacencyMatrix.SetValue(destinationIndex, sourceIndex, weight);
            }
        }
        //Méthode ajoutée pour pouvoir l'utiliser dans la classe Little
        public string GetVertexName(int index)
        {
            if (!indexToVertex.ContainsKey(index))
            {
                throw new ArgumentException($"Aucun sommet à l'indice {index}");
            }

            return indexToVertex[index];
        }
        public Matrix GetAdjacencyMatrix()
        {
            return this.adjacencyMatrix;
        }
    }


}
