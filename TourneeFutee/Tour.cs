namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce
    public class Tour
    {      
        List<(string source, string destination)> segment;
        float cost;

        public Tour(List<(string source, string destination)> segment, float cost)
        {
            this.segment = new List<(string source, string destination)>();
            this.cost = cost;
        }
        // propriétés

        // Coût total de la tournée
        public float Cost
        {
            get { return this.cost; }   
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get { return this.segment.Count; }   
        }


        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            return false;   // TODO : implémenter 
        }


        // Affiche les informations sur la tournée : coût total et trajets
        public void Print()
        {
            // TODO : implémenter 
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }
}
