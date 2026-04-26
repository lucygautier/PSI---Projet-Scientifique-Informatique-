using System;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    /// <summary>
    /// Service de persistance permettant de sauvegarder et charger
    /// des graphes et des tournées dans une base de données MySQL.
    /// </summary>
    public class ServicePersistance
    {
        // ─────────────────────────────────────────────────────────────────────
        // Attributs privés
        // ─────────────────────────────────────────────────────────────────────

        private readonly string _connectionString;
        private MySqlConnection conn;

        // TODO : si vous avez besoin de maintenir une connexion ouverte,
        //        ajoutez un attribut MySqlConnection ici.

        // ─────────────────────────────────────────────────────────────────────
        // Constructeur
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Instancie un service de persistance et se connecte automatiquement
        /// à la base de données <paramref name="dbname"/> sur le serveur
        /// à l'adresse IP <paramref name="serverIp"/>.
        /// Les identifiants sont définis par <paramref name="user"/> (utilisateur)
        /// et <paramref name="pwd"/> (mot de passe).
        /// </summary>
        /// <param name="serverIp">Adresse IP du serveur MySQL.</param>
        /// <param name="dbname">Nom de la base de données.</param>
        /// <param name="user">Nom d'utilisateur.</param>
        /// <param name="pwd">Mot de passe.</param>
        /// <exception cref="Exception">Levée si la connexion échoue.</exception>
        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
          // TODO : initialiser et ouvrir la connexion à la base de données
        // Exemple :
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";
            //Test de la connexion
            using (MySqlConnection testConn = new MySqlConnection(_connectionString))
            {
                testConn.Open();
            }

            // vraie connexion utilisée par la classe
            conn = OpenConnection();

            return;
            // TODO : tester la connexion dès la construction
            //        (ouvrir puis fermer une connexion pour valider les paramètres)
            throw new NotImplementedException("Constructeur non implémenté.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes publiques
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sauvegarde le graphe <paramref name="g"/> en base de données
        /// (sommets et arcs inclus) et renvoie son identifiant.
        /// </summary>
        /// <param name="g">Le graphe à sauvegarder.</param>
        /// <returns>Identifiant du graphe en base de données (AUTO_INCREMENT).</returns>
        public uint SaveGraph(Graph g)
        {
            // TODO : implémenter la sauvegarde du graphe
            //
            // Ordre recommandé :
            //   1. INSERT dans la table Graphe -> récupérer l'id avec LAST_INSERT_ID()
            //   2. Pour chaque sommet de g : INSERT dans Sommet (valeur + graphe_id)
            //      -> conserver la correspondance sommet C# <-> id BdD
            //   3. Pour chaque arc de la matrice d'adjacence (poids != +inf) :
            //      INSERT dans Arc (sommet_source_id, sommet_dest_id, poids, graphe_id)
            //
            // Exemple pour récupérer l'id généré :
            //   uint id = Convert.ToUInt32(cmd.ExecuteScalar());

            using (MySqlConnection conn = OpenConnection())
            {
                //Insértion du graphe
                string sqlGraphe = @"INSERT INTO Graphe (nom, nb_sommets, est_oriente)
                                    VALUES (@nom, @nb_sommets, @est_oriente);
                                    SELECT LAST_INSERT_ID();";

                MySqlCommand cmdGraphe = new MySqlCommand(sqlGraphe, conn);
                cmdGraphe.Parameters.AddWithValue("@nom", "Graphe");
                cmdGraphe.Parameters.AddWithValue("@nb_sommets", g.Order);
                cmdGraphe.Parameters.AddWithValue("@est_oriente", 0);

                uint grapheId = Convert.ToUInt32(cmdGraphe.ExecuteScalar());

                //Dictionnaire pour faire le lien entre indice C# et id SQL
                Dictionary<int, uint> idsSommets = new Dictionary<int, uint>();

                //Insértion des sommets
                for (int i = 0; i < g.Order; i++)
                {
                    string nomSommet = g.GetVertexName(i);

                    string sqlSommet = @"INSERT INTO Sommet (graphe_id, nom, valeur, indice)
                                         VALUES (@graphe_id, @nom, @valeur, @indice);
                                         SELECT LAST_INSERT_ID();";

                    MySqlCommand cmdSommet = new MySqlCommand(sqlSommet, conn);
                    cmdSommet.Parameters.AddWithValue("@graphe_id", grapheId);
                    cmdSommet.Parameters.AddWithValue("@nom", nomSommet);
                    cmdSommet.Parameters.AddWithValue("@valeur", 0);
                    cmdSommet.Parameters.AddWithValue("@indice", i);

                    uint sommetId = Convert.ToUInt32(cmdSommet.ExecuteScalar());
                    idsSommets.Add(i, sommetId);
                }

                //Insertion des arcs
                Matrix matrice = g.GetAdjacencyMatrix();

                for (int i = 0; i < g.Order; i++)
                {
                    for (int j = 0; j < g.Order; j++)
                    {
                        float poids = matrice.GetValue(i, j);

                        if (i != j && !float.IsPositiveInfinity(poids) && poids != 0)
                        {
                            string sqlArc = @"INSERT INTO Arc (graphe_id, sommet_source, sommet_dest, poids)
                                            VALUES (@graphe_id, @source, @dest, @poids);";

                            MySqlCommand cmdArc = new MySqlCommand(sqlArc, conn);
                            cmdArc.Parameters.AddWithValue("@graphe_id", grapheId);
                            cmdArc.Parameters.AddWithValue("@source", idsSommets[i]);
                            cmdArc.Parameters.AddWithValue("@dest", idsSommets[j]);
                            cmdArc.Parameters.AddWithValue("@poids", poids);
                            cmdArc.ExecuteNonQuery();
                        }
                    }
                }
                return grapheId;
            }
            throw new NotImplementedException("SaveGraph non implémenté.");
        }

        /// <summary>
        /// Charge depuis la base de données le graphe identifié par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Graph"/>.
        /// </summary>
        /// <param name="id">Identifiant du graphe à charger.</param>
        /// <returns>Instance de <see cref="Graph"/> reconstituée.</returns>
        public Graph LoadGraph(uint id)
        {
            // TODO : implémenter le chargement du graphe
            //
            // Ordre recommandé :
            //   1. SELECT dans Graphe WHERE id = @id -> récupérer IsOriented, etc.
            //   2. SELECT dans Sommet WHERE graphe_id = @id -> reconstruire les sommets
            //      (respecter l'ordre d'insertion pour que les indices de la matrice
            //       correspondent à ceux sauvegardés)
            //   3. SELECT dans Arc WHERE graphe_id = @id -> reconstruire la matrice
            //      d'adjacence en utilisant les correspondances sommet_id <-> indice

            using (MySqlConnection conn = OpenConnection())
            {
                // 1. Charger le graphe
                string sqlGraphe = "SELECT est_oriente FROM Graphe WHERE id = @id;";
                MySqlCommand cmdGraphe = new MySqlCommand(sqlGraphe, conn);
                cmdGraphe.Parameters.AddWithValue("@id", id);

                object result = cmdGraphe.ExecuteScalar();

                if (result == null)
                {
                    return null;
                }
                bool estOriente = Convert.ToBoolean(result);

                Graph g = new Graph(estOriente);

                //Chargement des sommets
                Dictionary<uint, int> idSqlVersIndice = new Dictionary<uint, int>();

                string sqlSommets = @"SELECT id, nom, valeur, indice
                                    FROM Sommet
                                    WHERE graphe_id = @id
                                    ORDER BY indice;";

                MySqlCommand cmdSommets = new MySqlCommand(sqlSommets, conn);
                cmdSommets.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmdSommets.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uint sommetId = Convert.ToUInt32(reader["id"]);
                        string nom = reader["nom"].ToString();
                        float valeur = 0;

                        if (reader["valeur"] != DBNull.Value)
                        {
                            valeur = Convert.ToSingle(reader["valeur"]);
                        }
                        int indice = Convert.ToInt32(reader["indice"]);
                        g.AddVertex(nom, valeur);
                        idSqlVersIndice.Add(sommetId, indice);
                    }
                }

                //Chargement des arcs
                string sqlArcs = @"SELECT sommet_source, sommet_dest, poids
                                FROM Arc
                                WHERE graphe_id = @id;";

                MySqlCommand cmdArcs = new MySqlCommand(sqlArcs, conn);
                cmdArcs.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmdArcs.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uint sourceId = Convert.ToUInt32(reader["sommet_source"]);
                        uint destId = Convert.ToUInt32(reader["sommet_dest"]);
                        float poids = Convert.ToSingle(reader["poids"]);
                        int indiceSource = idSqlVersIndice[sourceId];
                        int indiceDest = idSqlVersIndice[destId];
                        string nomSource = g.GetVertexName(indiceSource);
                        string nomDest = g.GetVertexName(indiceDest);
                        g.SetEdgeWeight(nomSource, nomDest, poids);
                    }
                }
                return g;
            }
            throw new NotImplementedException("LoadGraph non implémenté.");
        }

        /// <summary>
        /// Sauvegarde la tournée <paramref name="t"/> (effectuée dans le graphe
        /// identifié par <paramref name="graphId"/>) en base de données
        /// et renvoie son identifiant.
        /// </summary>
        /// <param name="graphId">Identifiant BdD du graphe dans lequel la tournée a été calculée.</param>
        /// <param name="t">La tournée à sauvegarder.</param>
        /// <returns>Identifiant de la tournée en base de données (AUTO_INCREMENT).</returns>
        public uint SaveTour(uint graphId, Tour t)
        {
            // TODO : implémenter la sauvegarde de la tournée
            //
            // Ordre recommandé :
            //   1. INSERT dans Tournee (cout_total, graphe_id) -> récupérer l'id
            //   2. Pour chaque sommet de la séquence (avec son numéro d'ordre) :
            //      INSERT dans EtapeTournee (tournee_id, numero_ordre, sommet_id)
            //
            // Attention : conserver l'ordre des étapes est essentiel pour
            //             pouvoir reconstruire la tournée fidèlement au chargement.

            throw new NotImplementedException("SaveTour non implémenté.");
        }

        /// <summary>
        /// Charge depuis la base de données la tournée identifiée par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Tour"/>.
        /// </summary>
        /// <param name="id">Identifiant de la tournée à charger.</param>
        /// <returns>Instance de <see cref="Tour"/> reconstituée.</returns>
        public Tour LoadTour(uint id)
        {
            // TODO : implémenter le chargement de la tournée
            //
            // Ordre recommandé :
            //   1. SELECT dans Tournee WHERE id = @id -> récupérer cout_total et graphe_id
            //   2. SELECT dans EtapeTournee JOIN Sommet WHERE tournee_id = @id
            //      ORDER BY numero_ordre -> reconstruire la séquence ordonnée de sommets
            //   3. Construire et retourner l'instance Tour

            throw new NotImplementedException("LoadTour non implémenté.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes utilitaires privées (à compléter selon vos besoins)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée et retourne une nouvelle connexion MySQL ouverte.
        /// Encadrez toujours l'appel dans un bloc using pour garantir la fermeture.
        /// </summary>
        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
