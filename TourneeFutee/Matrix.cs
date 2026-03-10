namespace TourneeFutee
{
    public class Matrix
    {
        int nbRows;
        int nbColumns;
        float defaultValue;
        List<List<float>> matrix;


        /* Crée une matrice de dimensions `nbRows` x `nbColums`.
         * Toutes les cases de cette matrice sont remplies avec `defaultValue`.
         * Lève une ArgumentOutOfRangeException si une des dimensions est négative
         */
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0)
            { throw new ArgumentOutOfRangeException($"Le nombre de lignes doit être positif."); } //Lève une exception si nbRows est négatif

            if (nbColumns < 0)
            {
                throw new ArgumentOutOfRangeException($"Le nombre de colonnes doit être positif."); //Lève une exception si nbColumns est négatif
            }

            this.nbRows = nbRows;
            this.nbColumns = nbColumns;
            this.defaultValue = defaultValue;
            this.matrix = new List<List<float>>(this.nbRows);

            for (int i = 0; i < this.nbRows; i++) //Ajout de la valeur par défaut dans chaque case
            {
                List<float> row = new List<float>(this.nbColumns); //Crée une ligne dans la liste qui contient le nombre nbColumns de cases

                for (int j = 0; j < this.nbColumns; j++)
                {
                    row.Add(this.defaultValue); //Remplissage chaque case avec la valeur par défaut
                }

                matrix.Add(row); //Ajout de la ligne à la matrice matrix

            }
        }

        // Propriété : valeur par défaut utilisée pour remplir les nouvelles cases
        // Lecture seule
        public float DefaultValue
        {
            get { return this.defaultValue; }
                 // pas de set
        }

        // Propriété : nombre de lignes
        // Lecture seule
        public int NbRows
        {
            get { return this.nbRows; }
                 // pas de set
        }

        // Propriété : nombre de colonnes
        // Lecture seule
        public int NbColumns
        {
            get { return this.nbColumns; } 
                 // pas de set
        }

        /* Insère une ligne à l'indice `i`. Décale les lignes suivantes vers le bas.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `i` = NbRows, insère une ligne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
         */
        public void AddRow(int i)
        {
            if (i < 0) //Lève une exception si i est négatif
            {
                throw new ArgumentOutOfRangeException(nameof(i), $"L'indice {i} ne peut pas être négatif.");
            }
            List<float> newRow = new List<float>();

            for (int j = 0; j < this.nbColumns; j++)
            {
                newRow.Add(this.defaultValue);
            }

            this.matrix.Insert(i, newRow);
            this.nbRows++;
        }

        /* Insère une colonne à l'indice `j`. Décale les colonnes suivantes vers la droite.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `j` = NbColums, insère une colonne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
         */
        public void AddColumn(int j)
        {
            if (j < 0)
            { throw new ArgumentOutOfRangeException(nameof(j), $"L'indice {j} doit être positif."); } //Lève une exception si l'indice j est négatif

            if (j > this.nbColumns)
            { throw new ArgumentOutOfRangeException(nameof(j), $"L'indice {j} doit être infèrieur au nombre de colonnes."); } //Lève une exception si l'indice j est suppérieur au nombre de colonnes


            foreach (List<float> row in this.matrix)
            {
                row.Insert(j, defaultValue); //Insère une case à l'indice j et l'initialise à la valeur par défaut
            }
            this.nbColumns++; //Incrémentation du nombre de colonnes
        }

        // Supprime la ligne à l'indice `i`. Décale les lignes suivantes vers le haut.
        // Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
        public void RemoveRow(int i)
        {
            if (i < 0)
            { throw new ArgumentOutOfRangeException(nameof(i), $"L'indice {i} ne peut pas être négatif."); } //Lève une exception si i est négatif

            if (i > this.nbRows)
            { throw new ArgumentOutOfRangeException(nameof(i), $"L'indice {i} ne peut pas dépasser le nombre de lignes."); } //Lève une excpetion si i est supérieur au nombre de lignes

            
            this.matrix.RemoveAt(i); //Enlève la ligne d'indice i de matrix
            this.nbRows--; //Décrémente le nombre de lignes suite à la suppression
            
        }

        // Supprime la colonne à l'indice `j`. Décale les colonnes suivantes vers la gauche.
        // Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
        public void RemoveColumn(int j)
        {
            if (j < 0)
            { throw new ArgumentOutOfRangeException(nameof(j),$"L'indice {j} ne peut pas être négatif."); } //Lève une exception si l'indice j est négatif

            if (j > this.nbColumns)
            { throw new ArgumentOutOfRangeException(nameof(j), $"L'indice {j} ne doit pas dépasser le nombre de colonnes."); } //Lève une exception si l'indice j est supérieur au nombre de colonnes

            foreach (List<float> row in this.matrix)
            {
                row.RemoveAt(j); //Supprime de la matrice 
            }
            this.nbColumns--;
        }

        // Renvoie la valeur à la ligne `i` et colonne `j`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public float GetValue(int i, int j)
        {
            if (i < 0 || i >= this.nbRows || j < 0 || j >= this.nbColumns)
            {
                throw new ArgumentOutOfRangeException("Les indices invalides"); //Lève une exception si les indices sont out of range
            }
            return this.matrix[i][j]; //Renvoie la valeur à la ligne i et la colonne j
        }

        // Affecte la valeur à la ligne `i` et colonne `j` à `v`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public void SetValue(int i, int j, float v)
        {
            if (i < 0 || i >= this.nbRows || j < 0 || j >= this.nbColumns)
            {
                throw new ArgumentOutOfRangeException("Indice(s) invalides");
            }

            this.matrix[i][j] = v;
        }

        // Affiche la matrice
        public void Print()
        {
            foreach (List<float> ligne in this.matrix)
            {
                foreach (float e in ligne)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
